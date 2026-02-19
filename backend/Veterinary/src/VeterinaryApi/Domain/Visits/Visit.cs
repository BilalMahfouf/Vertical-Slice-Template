using Microsoft.VisualBasic;
using System.Diagnostics;
using VeterinaryApi.Domain.Animals;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Clients;
using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Visits;

public class Visit : Entity
{
    public Guid AnimalId { get; private set; }

    public Guid OwnerId { get; private set; }

    public Guid? AppointmentId { get; private set; }
    public VisitType VisitType { get; private set; }
    public List<string>? Symptoms { get; private set; }
    public List<string>? Diagnosis { get; private set; }
    public List<string>? Treatment { get; private set; }
    public string? Notes { get; private set; }

    public decimal PaymentAmount { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public Animal Animal { get; private set; } = null!;
    public Client Owner { get; private set; } = null!;
    public Appointment? Appointment { get; private set; }

    private Visit()
    {
    }

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
        PaymentStatus paymentStatus = PaymentStatus.Pending
        )
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

        return visit;
    }
    private void ValidatePaymentAmount(decimal paymentAmount)
    {
        if (paymentAmount < MinPaymentAmount)
        {
            throw new DomainException(VisitErrors.InvalidPaymentAmount);
        }
    }
    internal static decimal MinPaymentAmount = 0;
    private List<string>? GetStrings(List<string>? strList)
    {
        return strList?
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .ToList();
    }

    private void UpdatePayment(decimal paymentAmount,PaymentStatus paymentStatus)
    {
        ValidatePaymentAmount(paymentAmount);
        if(paymentStatus is PaymentStatus.Paid)
        {
            throw new DomainException(VisitErrors.PaymentAlreadyPayed);
        }

        PaymentAmount = paymentAmount;
        PaymentStatus = paymentStatus;
    }
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
