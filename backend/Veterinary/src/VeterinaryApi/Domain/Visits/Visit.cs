using Microsoft.VisualBasic;
using System.Diagnostics;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Visits;

/// <summary>
/// Aggregate root representing a single veterinary consultation visit.
/// A visit records the clinical findings, treatment, and financial settlement for one
/// encounter between a veterinarian and an animal.
/// </summary>
/// <remarks>
/// A visit may optionally be linked to a pre-existing <see cref="Appointment"/> (scheduled flow),
/// or created directly without one (walk-in / emergency flow).
/// Only one visit can reference a given appointment — enforced in the command handler.
///
/// Symptom, diagnosis, and treatment data are stored as trimmed string lists to allow
/// multiple discrete entries per field (e.g., several diagnoses per visit).
/// </remarks>
public class Visit : Entity
{
    /// <summary>Gets the foreign key of the animal that was examined during this visit.</summary>
    public Guid AnimalId { get; private set; }

    /// <summary>Gets the foreign key of the <see cref="Client"/> who owns the animal.</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Gets the optional foreign key of the <see cref="Appointment"/> that this visit fulfils.
    /// <c>null</c> for walk-in visits created without a prior booking.
    /// </summary>
    public Guid? AppointmentId { get; private set; }

    /// <summary>Gets the category/setting of this visit (e.g., clinic, field, emergency).</summary>
    public VisitType VisitType { get; private set; }

    /// <summary>Gets the list of observed clinical symptoms reported or noted during the visit.</summary>
    public List<string>? Symptoms { get; private set; }

    /// <summary>Gets the list of clinical diagnoses reached during the visit.</summary>
    public List<string>? Diagnosis { get; private set; }

    /// <summary>Gets the list of prescribed or administered treatments during the visit.</summary>
    public List<string>? Treatment { get; private set; }

    /// <summary>Gets optional free-text follow-up notes recorded by the veterinarian.</summary>
    public string? Notes { get; private set; }

    /// <summary>Gets the total payment amount charged for this visit (minimum <see cref="MinPaymentAmount"/>).</summary>
    public decimal PaymentAmount { get; private set; }

    /// <summary>Gets the current payment settlement status for this visit.</summary>
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>Navigation property to the examined <see cref="Animal"/>.</summary>
    public Animal Animal { get; private set; } = null!;

    /// <summary>Navigation property to the animal's owning <see cref="Client"/>.</summary>
    public Client Owner { get; private set; } = null!;

    /// <summary>Navigation property to the optional linked <see cref="Appointment"/>.</summary>
    public Appointment? Appointment { get; private set; }

    /// <summary>EF Core required parameterless constructor. Not for direct use.</summary>
    private Visit()
    {
    }

    /// <summary>
    /// Factory method — creates and validates a new <see cref="Visit"/> instance.
    /// </summary>
    /// <param name="animalId">The animal being examined.</param>
    /// <param name="ownerId">The owner (client) of the animal.</param>
    /// <param name="appointmentId">The optional appointment this visit is linked to.</param>
    /// <param name="visitType">The setting/urgency category of the visit.</param>
    /// <param name="symptoms">Observed symptoms (whitespace entries are filtered).</param>
    /// <param name="diagnosis">Clinical diagnoses (whitespace entries are filtered).</param>
    /// <param name="treatment">Treatments applied (whitespace entries are filtered).</param>
    /// <param name="followUpNotes">Optional free-text veterinarian notes.</param>
    /// <param name="appointmentDate">
    /// If supplied, validated to be on or after today's UTC date.
    /// Throws if the date is in the past.
    /// </param>
    /// <param name="paymentAmount">The fee charged. Defaults to <c>0</c>. Must be ≥ <see cref="MinPaymentAmount"/>.</param>
    /// <param name="paymentStatus">The initial payment state. Defaults to <see cref="PaymentStatus.Pending"/>.</param>
    /// <returns>A fully initialized <see cref="Visit"/> aggregate.</returns>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="appointmentDate"/> is in the past, or when
    /// <paramref name="paymentAmount"/> is below <see cref="MinPaymentAmount"/>.
    /// </exception>
    public static Visit Create(
        Guid animalId,
        Guid ownerId,
        Guid? appointmentId,
        VisitType visitType,
        List<string>? symptoms,
        List<string>? diagnosis,
        List<string>? treatment,
        string? followUpNotes,
        DateTime? appointmentDate = null,
        decimal paymentAmount = 0,
        PaymentStatus paymentStatus = PaymentStatus.Pending)
    {
        if (appointmentDate.HasValue)
        {
            if (appointmentDate.Value.Date < DateTime.UtcNow.Date)
            {
                throw new DomainException(
                    AppointmentErrors.OutDatedAppointment(appointmentDate.Value));
            }
        }

        var visit = new Visit();

        visit.ValidatePaymentAmount(paymentAmount);

        visit.AnimalId = animalId;
        visit.OwnerId = ownerId;
        visit.AppointmentId = appointmentId;
        visit.VisitType = visitType;

        visit.Symptoms = visit.GetStrings(symptoms);
        visit.Diagnosis = visit.GetStrings(diagnosis);
        visit.Treatment = visit.GetStrings(treatment);

        visit.Notes = string.IsNullOrWhiteSpace(followUpNotes) ? null : followUpNotes.Trim();

        visit.PaymentAmount = paymentAmount;
        visit.PaymentStatus = paymentStatus;

        if (visit.PaymentStatus is PaymentStatus.Pending)
        {
            visit.RaiseDomainEvent(
                new VisitPaymentPendingReminderDomainEvent(visit.Id));
        }

        return visit;
    }

    /// <summary>The minimum acceptable payment amount. Currently <c>0</c> (free visits are allowed).</summary>
    internal static decimal MinPaymentAmount = 0;

    /// <summary>
    /// Validates that the given payment amount meets the minimum threshold.
    /// </summary>
    /// <param name="paymentAmount">Amount to validate.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="paymentAmount"/> is below <see cref="MinPaymentAmount"/>.</exception>
    private void ValidatePaymentAmount(decimal paymentAmount)
    {
        if (paymentAmount < MinPaymentAmount)
        {
            throw new DomainException(VisitErrors.InvalidPaymentAmount);
        }
    }

    /// <summary>
    /// Filters and trims a string list, removing null, empty, and whitespace-only entries.
    /// </summary>
    /// <param name="strList">The raw string list to sanitize.</param>
    /// <returns>A filtered list, or <c>null</c> if the input is <c>null</c>.</returns>
    private List<string>? GetStrings(List<string>? strList)
    {
        return strList?
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .ToList();
    }

    /// <summary>
    /// Updates the payment amount and status for this visit.
    /// </summary>
    /// <param name="paymentAmount">The new payment amount. Must be ≥ <see cref="MinPaymentAmount"/>.</param>
    /// <param name="paymentStatus">The new payment status.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="paymentAmount"/> is negative, or when
    /// <paramref name="paymentStatus"/> is <see cref="PaymentStatus.Paid"/> (already paid).
    /// </exception>
    private void UpdatePayment(decimal paymentAmount, PaymentStatus paymentStatus)
    {
        ValidatePaymentAmount(paymentAmount);
        if (paymentStatus is PaymentStatus.Paid)
        {
            throw new DomainException(VisitErrors.PaymentAlreadyPayed);
        }

        PaymentAmount = paymentAmount;
        PaymentStatus = paymentStatus;
    }

    /// <summary>
    /// Updates the clinical details and payment information of this visit.
    /// </summary>
    /// <param name="visitType">The updated visit setting/urgency category.</param>
    /// <param name="symptoms">Updated symptom list.</param>
    /// <param name="diagnosis">Updated diagnosis list.</param>
    /// <param name="treatment">Updated treatment list.</param>
    /// <param name="followUpNotes">Updated free-text notes.</param>
    /// <param name="paymentAmount">Updated payment amount.</param>
    /// <param name="paymentStatus">Updated payment status.</param>
    /// <exception cref="DomainException">Thrown for invalid payment updates (see <see cref="UpdatePayment"/>).</exception>
    public void UpdateDetails(
        VisitType visitType,
        List<string>? symptoms,
        List<string>? diagnosis,
        List<string>? treatment,
        string? followUpNotes,
        decimal paymentAmount,
        PaymentStatus paymentStatus)
    {
        UpdatePayment(paymentAmount, paymentStatus);

        VisitType = visitType;
        Symptoms = GetStrings(symptoms);
        Diagnosis = GetStrings(diagnosis);
        Treatment = GetStrings(treatment);
        Notes = string.IsNullOrWhiteSpace(followUpNotes) ? null : followUpNotes.Trim();
    }
}
