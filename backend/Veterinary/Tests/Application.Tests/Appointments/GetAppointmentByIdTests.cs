////using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;
//using Application.Tests.Helpers;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using VeterinaryApi.Common.Abstracions;
//using VeterinaryApi.Common.Errors;
//using VeterinaryApi.Domain;
//using VeterinaryApi.Domain.Animals;
//using VeterinaryApi.Domain.Appointments;
//using VeterinaryApi.Domain.Clients;
//using VeterinaryApi.Features.Appointments;
//using Xunit;

//namespace Application.Tests.Appointments;

//public class GetAppointmentByIdTests
//{
//    private readonly Mock<IApplicationDbContext> _mockDbContext;

//    public GetAppointmentByIdTests()
//    {
//        _mockDbContext = new Mock<IApplicationDbContext>();
//    }

//    private GetAppointmentById.GetAppointmentByIdQueryHandler CreateHandler()
//    {
//        return new GetAppointmentById.GetAppointmentByIdQueryHandler(_mockDbContext.Object);
//    }

//    private void SetupAppointmentsDbSet(List<Appointment> appointments)
//    {
//        var mockDbSet = DbSetMockHelper.CreateMockDbSet(appointments);
//        _mockDbContext.Setup(db => db.Appointments).Returns(mockDbSet.Object);
//    }

//    private Appointment CreateAppointmentWithRelations(
//        Guid id,
//        Guid clinicId,
//        Guid animalId,
//        Guid clientId,
//        string clientName,
//        string animalName,
//        DateTime appointmentDate,
//        AppointmentStatus status)
//    {
//        var appointment = Appointment.Create(
//            animalId,
//            clinicId,
//            appointmentDate,
//            TimeSpan.FromHours(10),
//            "Main Clinic",
//            "Regular checkup");

//        var idProperty = typeof(Appointment).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(appointment, id);

//        var statusProperty = typeof(Appointment).GetProperty("Status", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
//        statusProperty!.SetValue(appointment, status);

//        var client = Client.Create(clinicId, clientName.Split(' ')[0], clientName.Split(' ').Length > 1 ? clientName.Split(' ')[1] : "", "123-456-7890");
//        var clientIdProperty = typeof(Client).BaseType!.GetProperty("Id");
//        clientIdProperty!.SetValue(client, clientId);

//        var animal = Animal.Create(clinicId, clientId, animalName, "Dog", "Labrador", Gender.Male, DateTime.UtcNow.AddYears(-2), "Brown", AnimalStatus.Active);
//        var animalIdProperty = typeof(Animal).BaseType!.GetProperty("Id");
//        animalIdProperty!.SetValue(animal, animalId);

//        var animalClientProperty = typeof(Animal).GetProperty("Client");
//        animalClientProperty!.SetValue(animal, client);

//        var appointmentAnimalProperty = typeof(Appointment).GetProperty("Animal");
//        appointmentAnimalProperty!.SetValue(appointment, animal);

//        return appointment;
//    }

//    [Fact]
//    public async Task Handle_WhenAppointmentExists_ShouldReturnSuccessWithAppointmentData()
//    {
//        // Arrange
//        var appointmentId = Guid.NewGuid();
//        var clinicId = Guid.NewGuid();
//        var animalId = Guid.NewGuid();
//        var clientId = Guid.NewGuid();
//        var appointmentDate = DateTime.UtcNow.AddDays(1);

//        var appointment = CreateAppointmentWithRelations(
//            appointmentId, clinicId, animalId, clientId,
//            "John Doe", "Max", appointmentDate, AppointmentStatus.Confirmed);

//        SetupAppointmentsDbSet([appointment]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.NotNull(result.Value);
//        Assert.Equal(appointmentId, result.Value.Id);
//        Assert.Equal(clinicId, result.Value.ClinicId);
//        Assert.Equal(clientId, result.Value.ClientId);
//        Assert.Equal("John Doe", result.Value.ClientName);
//        Assert.Equal("Max", result.Value.AnimalName);
//        Assert.Equal("Confirmed", result.Value.Status);
//    }

//    [Fact]
//    public async Task Handle_WhenAppointmentNotFound_ShouldReturnFailure()
//    {
//        // Arrange
//        var nonExistentId = Guid.NewGuid();
//        SetupAppointmentsDbSet([]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(nonExistentId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.False(result.IsSuccess);
//        Assert.NotNull(result.Error);
//        Assert.Equal("Appointment.AppointmentNotFound", result.Error.Code);
//        Assert.Equal(ErrorType.NotFound, result.Error.Type);
//        Assert.Contains(nonExistentId.ToString(), result.Error.Description);
//    }

//    [Fact]
//    public async Task Handle_WithMultipleAppointments_ShouldReturnCorrectAppointment()
//    {
//        // Arrange
//        var appointmentId1 = Guid.NewGuid();
//        var appointmentId2 = Guid.NewGuid();
//        var appointmentId3 = Guid.NewGuid();

//        var appointment1 = CreateAppointmentWithRelations(
//            appointmentId1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
//        var appointment2 = CreateAppointmentWithRelations(
//            appointmentId2, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "Jane Smith", "Buddy", DateTime.UtcNow.AddDays(2), AppointmentStatus.Rescheduled);
//        var appointment3 = CreateAppointmentWithRelations(
//            appointmentId3, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "Bob Johnson", "Rex", DateTime.UtcNow.AddDays(3), AppointmentStatus.Cancelled);

//        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId2);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.NotNull(result.Value);
//        Assert.Equal(appointmentId2, result.Value.Id);
//        Assert.Equal("Jane Smith", result.Value.ClientName);
//        Assert.Equal("Buddy", result.Value.AnimalName);
//        Assert.Equal("Rescheduled", result.Value.Status);
//    }

//    [Fact]
//    public async Task Handle_WithEmptyGuid_ShouldReturnNotFound()
//    {
//        // Arrange
//        var appointment = CreateAppointmentWithRelations(
//            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);

//        SetupAppointmentsDbSet([appointment]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(Guid.Empty);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.False(result.IsSuccess);
//        Assert.Equal("Appointment.AppointmentNotFound", result.Error.Code);
//    }

//    [Fact]
//    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationTokenToDatabase()
//    {
//        // Arrange
//        var appointmentId = Guid.NewGuid();
//        var appointment = CreateAppointmentWithRelations(
//            appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);

//        SetupAppointmentsDbSet([appointment]);

//        using var cancellationTokenSource = new CancellationTokenSource();
//        var cancellationToken = cancellationTokenSource.Token;

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId);

//        // Act
//        var result = await handler.Handle(query, cancellationToken);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.NotNull(result.Value);
//    }

//    [Fact]
//    public async Task Handle_ShouldReturnResponseWithAllFields()
//    {
//        // Arrange
//        var appointmentId = Guid.NewGuid();
//        var clinicId = Guid.NewGuid();
//        var animalId = Guid.NewGuid();
//        var clientId = Guid.NewGuid();
//        var appointmentDate = DateTime.UtcNow.AddDays(5);

//        var appointment = CreateAppointmentWithRelations(
//            appointmentId, clinicId, animalId, clientId,
//            "Alice Johnson", "Bella", appointmentDate, AppointmentStatus.Confirmed);

//        SetupAppointmentsDbSet([appointment]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        var response = result.Value;

//        Assert.Equal(appointmentId, response.Id);
//        Assert.Equal(clinicId, response.ClinicId);
//        Assert.Equal(clientId, response.ClientId);
//        Assert.Equal("Alice Johnson", response.ClientName);
//        Assert.Equal("Bella", response.AnimalName);
//        Assert.Equal("Confirmed", response.Status);
//        Assert.NotEqual(default(DateTime), response.CreatedOnUtc);
//    }

//    [Fact]
//    public async Task Handle_WhenQueryingNonExistentAppointmentWithOtherAppointmentsPresent_ShouldReturnNotFound()
//    {
//        // Arrange
//        var existingAppointmentId = Guid.NewGuid();
//        var nonExistentAppointmentId = Guid.NewGuid();

//        var existingAppointment = CreateAppointmentWithRelations(
//            existingAppointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);

//        SetupAppointmentsDbSet([existingAppointment]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(nonExistentAppointmentId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.False(result.IsSuccess);
//        Assert.Equal("Appointment.AppointmentNotFound", result.Error.Code);
//        Assert.Contains(nonExistentAppointmentId.ToString(), result.Error.Description);
//    }

//    [Fact]
//    public async Task Handle_WithConfirmedStatus_ShouldReturnCorrectStatus()
//    {
//        // Arrange
//        var appointmentId = Guid.NewGuid();
//        var appointment = CreateAppointmentWithRelations(
//            appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);

//        SetupAppointmentsDbSet([appointment]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal("Confirmed", result.Value.Status);
//    }

//    [Fact]
//    public async Task Handle_WithCancelledStatus_ShouldReturnCorrectStatus()
//    {
//        // Arrange
//        var appointmentId = Guid.NewGuid();
//        var appointment = CreateAppointmentWithRelations(
//            appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Cancelled);

//        SetupAppointmentsDbSet([appointment]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal("Cancelled", result.Value.Status);
//    }

//    [Fact]
//    public async Task Handle_WithRescheduledStatus_ShouldReturnCorrectStatus()
//    {
//        // Arrange
//        var appointmentId = Guid.NewGuid();
//        var appointment = CreateAppointmentWithRelations(
//            appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Rescheduled);

//        SetupAppointmentsDbSet([appointment]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal("Rescheduled", result.Value.Status);
//    }

//    [Fact]
//    public async Task Handle_WithCompletedStatus_ShouldReturnCorrectStatus()
//    {
//        // Arrange
//        var appointmentId = Guid.NewGuid();
//        var appointment = CreateAppointmentWithRelations(
//            appointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Completed);

//        SetupAppointmentsDbSet([appointment]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal("Completed", result.Value.Status);
//    }

//    [Fact]
//    public async Task Handle_QueryFirstAppointmentInList_ShouldReturnCorrectAppointment()
//    {
//        // Arrange
//        var appointmentId1 = Guid.NewGuid();
//        var appointmentId2 = Guid.NewGuid();

//        var appointment1 = CreateAppointmentWithRelations(
//            appointmentId1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "First Client", "First Animal", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
//        var appointment2 = CreateAppointmentWithRelations(
//            appointmentId2, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "Second Client", "Second Animal", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);

//        SetupAppointmentsDbSet([appointment1, appointment2]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId1);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal("First Client", result.Value.ClientName);
//        Assert.Equal("First Animal", result.Value.AnimalName);
//    }

//    [Fact]
//    public async Task Handle_QueryLastAppointmentInList_ShouldReturnCorrectAppointment()
//    {
//        // Arrange
//        var appointmentId1 = Guid.NewGuid();
//        var appointmentId2 = Guid.NewGuid();

//        var appointment1 = CreateAppointmentWithRelations(
//            appointmentId1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "First Client", "First Animal", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
//        var appointment2 = CreateAppointmentWithRelations(
//            appointmentId2, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
//            "Last Client", "Last Animal", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);

//        SetupAppointmentsDbSet([appointment1, appointment2]);

//        var handler = CreateHandler();
//        var query = new GetAppointmentById.Query(appointmentId2);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal("Last Client", result.Value.ClientName);
//        Assert.Equal("Last Animal", result.Value.AnimalName);
//    }
//}
