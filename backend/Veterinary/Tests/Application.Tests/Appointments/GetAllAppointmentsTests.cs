//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;
//using Application.Tests.Helpers;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using VeterinaryApi.Common.Abstracions;
//using VeterinaryApi.Common.Paginations;
//using VeterinaryApi.Domain;
//using VeterinaryApi.Domain.Animals;
//using VeterinaryApi.Domain.Appointments;
//using VeterinaryApi.Domain.Clients;
//using VeterinaryApi.Domain.Clinics;
//using VeterinaryApi.Features.Appointments;
//using Xunit;

//namespace Application.Tests.Appointments;

//public class GetAllAppointmentsTests
//{
//    private readonly Mock<IApplicationDbContext> _mockDbContext;

//    public GetAllAppointmentsTests()
//    {
//        _mockDbContext = new Mock<IApplicationDbContext>();
//    }

//    private GetAllAppointments.GetAllAppointmentsQueryHandler CreateHandler()
//    {
//        return new GetAllAppointments.GetAllAppointmentsQueryHandler(_mockDbContext.Object);
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
////        var appointment = Appointment.Create(
////            animalId,
////            clinicId,
////            appointmentDate,
////            TimeSpan.FromHours(10),
////            "Main Clinic",
////            "Regular checkup");

////        var idProperty = typeof(Appointment).BaseType!.GetProperty("Id");
////        idProperty!.SetValue(appointment, id);

////        var statusProperty = typeof(Appointment).GetProperty("Status", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
////        statusProperty!.SetValue(appointment, status);

////        var client = Client.Create(clinicId, clientName.Split(' ')[0], clientName.Split(' ').Length > 1 ? clientName.Split(' ')[1] : "", "123-456-7890");
////        var clientIdProperty = typeof(Client).BaseType!.GetProperty("Id");
////        clientIdProperty!.SetValue(client, clientId);

////        var animal = Animal.Create(clinicId, clientId, animalName, "Dog", "Labrador", Gender.Male, DateTime.UtcNow.AddYears(-2), "Brown", AnimalStatus.Active);
////        var animalIdProperty = typeof(Animal).BaseType!.GetProperty("Id");
////        animalIdProperty!.SetValue(animal, animalId);

////        var animalClientProperty = typeof(Animal).GetProperty("Client");
////        animalClientProperty!.SetValue(animal, client);

////        var appointmentAnimalProperty = typeof(Appointment).GetProperty("Animal");
////        appointmentAnimalProperty!.SetValue(appointment, animal);

////        return appointment;
////    }

////    private TableRequest<GetAllAppointments.Response> CreateRequest(
////        int? pageSize = null,
////        int? page = null,
////        string? search = null,
////        string? sortColumn = null,
////        string? sortOrder = null)
////    {
////        return TableRequest<GetAllAppointments.Response>.Create(pageSize, page, search, sortColumn, sortOrder);
////    }

////    [Fact]
////    public async Task Handle_WithAppointmentsInDatabase_ShouldReturnPagedList()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Jane Smith", "Buddy", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);
////        var appointment3 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Bob Johnson", "Rex", DateTime.UtcNow.AddDays(3), AppointmentStatus.Rescheduled);

////        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1);

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.NotNull(result.Value);
////        Assert.Equal(3, result.Value.TotalCount);
////        Assert.Equal(3, result.Value.Item.Count());
////    }

////    [Fact]
////    public async Task Handle_WhenNoAppointmentsExist_ShouldReturnFailure()
////    {
////        // Arrange
////        SetupAppointmentsDbSet([]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1);

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.False(result.IsSuccess);
////        Assert.NotNull(result.Error);
////        Assert.Equal("Appointment.AppointmentsNotFound", result.Error.Code);
////    }

////    [Fact]
////    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
////    {
////        // Arrange
////        var appointments = new List<Appointment>();
////        for (int i = 1; i <= 15; i++)
////        {
////            var appointment = CreateAppointmentWithRelations(
////                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////                $"Client {i}", $"Animal {i}", DateTime.UtcNow.AddDays(i), AppointmentStatus.Confirmed);
////            appointments.Add(appointment);
////        }

////        SetupAppointmentsDbSet(appointments);

////        var handler = CreateHandler();
////        var request = CreateRequest(5, 2);

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Equal(15, result.Value.TotalCount);
////        Assert.Equal(5, result.Value.Item.Count());
////        Assert.Equal(2, result.Value.Page);
////        Assert.Equal(5, result.Value.PageSize);
////        Assert.True(result.Value.HasPreviousPage);
////        Assert.True(result.Value.HasNextPage);
////    }

////    [Fact]
////    public async Task Handle_WithSearchByClientName_ShouldFilterResults()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Smith", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Jane Doe", "Buddy", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);
////        var appointment3 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Johnson", "Rex", DateTime.UtcNow.AddDays(3), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1, "john");

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Equal(2, result.Value.Item.Count());
////        Assert.All(result.Value.Item, a => Assert.Contains("John", a.ClientName));
////    }

////    [Fact]
////    public async Task Handle_WithSearchByAnimalName_ShouldFilterResults()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Jane Smith", "Buddy", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);
////        var appointment3 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Bob Johnson", "Max", DateTime.UtcNow.AddDays(3), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1, "max");

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Equal(2, result.Value.Item.Count());
////        Assert.All(result.Value.Item, a => Assert.Equal("Max", a.AnimalName));
////    }

////    [Fact]
////    public async Task Handle_WithSearchByStatus_ShouldFilterResults()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Jane Smith", "Buddy", DateTime.UtcNow.AddDays(2), AppointmentStatus.Cancelled);
////        var appointment3 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Bob Johnson", "Rex", DateTime.UtcNow.AddDays(3), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1, "confirmed");

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Equal(2, result.Value.Item.Count());
////        Assert.All(result.Value.Item, a => Assert.Equal("Confirmed", a.Status));
////    }

////    [Fact]
////    public async Task Handle_WithSortByDate_ShouldSortCorrectly()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Doe", "Max", DateTime.UtcNow.AddDays(3), AppointmentStatus.Confirmed);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Jane Smith", "Buddy", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
////        var appointment3 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Bob Johnson", "Rex", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1, null, "date", "asc");

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        var items = result.Value.Item.ToList();
////        Assert.Equal("Jane Smith", items[0].ClientName);
////        Assert.Equal("Bob Johnson", items[1].ClientName);
////        Assert.Equal("John Doe", items[2].ClientName);
////    }

////    [Fact]
////    public async Task Handle_WithSortByDateDescending_ShouldSortCorrectly()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Doe", "Max", DateTime.UtcNow.AddDays(3), AppointmentStatus.Confirmed);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Jane Smith", "Buddy", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
////        var appointment3 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Bob Johnson", "Rex", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1, null, "date", "desc");

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        var items = result.Value.Item.ToList();
////        Assert.Equal("John Doe", items[0].ClientName);
////        Assert.Equal("Bob Johnson", items[1].ClientName);
////        Assert.Equal("Jane Smith", items[2].ClientName);
////    }

////    [Fact]
////    public async Task Handle_WithSortByStatus_ShouldSortCorrectly()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Rescheduled);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Jane Smith", "Buddy", DateTime.UtcNow.AddDays(2), AppointmentStatus.Cancelled);
////        var appointment3 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Bob Johnson", "Rex", DateTime.UtcNow.AddDays(3), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1, null, "status", "asc");

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Equal(3, result.Value.Item.Count());
////    }

////    [Fact]
////    public async Task Handle_WithSortByClientName_ShouldSortCorrectly()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Charlie Brown", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Alice Smith", "Buddy", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);
////        var appointment3 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Bob Johnson", "Rex", DateTime.UtcNow.AddDays(3), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1, null, "clientname", "asc");

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        var items = result.Value.Item.ToList();
////        Assert.Equal("Alice Smith", items[0].ClientName);
////        Assert.Equal("Bob Johnson", items[1].ClientName);
////        Assert.Equal("Charlie Brown", items[2].ClientName);
////    }

////    [Fact]
////    public async Task Handle_ResponseShouldIncludeAllFields()
////    {
////        // Arrange
////        var appointmentId = Guid.NewGuid();
////        var clinicId = Guid.NewGuid();
////        var clientId = Guid.NewGuid();
////        var animalId = Guid.NewGuid();
////        var appointmentDate = DateTime.UtcNow.AddDays(1);

////        var appointment = CreateAppointmentWithRelations(
////            appointmentId, clinicId, animalId, clientId,
////            "John Doe", "Max", appointmentDate, AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1);

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        var response = result.Value.Item.First();

////        Assert.Equal(appointmentId, response.Id);
////        Assert.Equal(clinicId, response.ClinicId);
////        Assert.Equal(clientId, response.ClientId);
////        Assert.Equal("John Doe", response.ClientName);
////        Assert.Equal("Max", response.AnimalName);
////        Assert.Equal("Confirmed", response.Status);
////        Assert.NotEqual(default(DateTime), response.CreatedOnUtc);
////    }

////    [Fact]
////    public async Task Handle_WithDefaultPageSize_ShouldReturnCorrectPageSize()
////    {
////        // Arrange
////        var appointments = new List<Appointment>();
////        for (int i = 1; i <= 20; i++)
////        {
////            var appointment = CreateAppointmentWithRelations(
////                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////                $"Client {i}", $"Animal {i}", DateTime.UtcNow.AddDays(i), AppointmentStatus.Confirmed);
////            appointments.Add(appointment);
////        }

////        SetupAppointmentsDbSet(appointments);

////        var handler = CreateHandler();
////        var request = CreateRequest(null, null);

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Equal(20, result.Value.TotalCount);
////        Assert.Equal(10, result.Value.Item.Count());
////        Assert.Equal(10, result.Value.PageSize);
////        Assert.Equal(1, result.Value.Page);
////    }

////    [Fact]
////    public async Task Handle_WithEmptySearchString_ShouldReturnAllAppointments()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Jane Smith", "Buddy", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);
////        var appointment3 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Bob Johnson", "Rex", DateTime.UtcNow.AddDays(3), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment1, appointment2, appointment3]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1, "");

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Equal(3, result.Value.Item.Count());
////    }

////    [Fact]
////    public async Task Handle_WithLastPage_ShouldNotHaveNextPage()
////    {
////        // Arrange
////        var appointments = new List<Appointment>();
////        for (int i = 1; i <= 8; i++)
////        {
////            var appointment = CreateAppointmentWithRelations(
////                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////                $"Client {i}", $"Animal {i}", DateTime.UtcNow.AddDays(i), AppointmentStatus.Confirmed);
////            appointments.Add(appointment);
////        }

////        SetupAppointmentsDbSet(appointments);

////        var handler = CreateHandler();
////        var request = CreateRequest(5, 2);

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Equal(8, result.Value.TotalCount);
////        Assert.Equal(3, result.Value.Item.Count());
////        Assert.True(result.Value.HasPreviousPage);
////        Assert.False(result.Value.HasNextPage);
////    }

////    [Fact]
////    public async Task Handle_WithFirstPage_ShouldNotHavePreviousPage()
////    {
////        // Arrange
////        var appointments = new List<Appointment>();
////        for (int i = 1; i <= 15; i++)
////        {
////            var appointment = CreateAppointmentWithRelations(
////                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////                $"Client {i}", $"Animal {i}", DateTime.UtcNow.AddDays(i), AppointmentStatus.Confirmed);
////            appointments.Add(appointment);
////        }

////        SetupAppointmentsDbSet(appointments);

////        var handler = CreateHandler();
////        var request = CreateRequest(5, 1);

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.False(result.Value.HasPreviousPage);
////        Assert.True(result.Value.HasNextPage);
////    }

////    [Fact]
////    public async Task Handle_WithSearchNoMatches_ShouldReturnEmptyList()
////    {
////        // Arrange
////        var appointment1 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);
////        var appointment2 = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "Jane Smith", "Buddy", DateTime.UtcNow.AddDays(2), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment1, appointment2]);

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1, "NonExistent");

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Empty(result.Value.Item);
////    }

////    [Fact]
////    public async Task Handle_TotalCountShouldReflectAllAppointmentsNotJustCurrentPage()
////    {
////        // Arrange
////        var appointments = new List<Appointment>();
////        for (int i = 1; i <= 25; i++)
////        {
////            var appointment = CreateAppointmentWithRelations(
////                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////                $"Client {i}", $"Animal {i}", DateTime.UtcNow.AddDays(i), AppointmentStatus.Confirmed);
////            appointments.Add(appointment);
////        }

////        SetupAppointmentsDbSet(appointments);

////        var handler = CreateHandler();
////        var request = CreateRequest(5, 3);

////        // Act
////        var result = await handler.Handle(request, CancellationToken.None);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.Equal(25, result.Value.TotalCount);
////        Assert.Equal(5, result.Value.Item.Count());
////    }

////    [Fact]
////    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationTokenToDatabase()
////    {
////        // Arrange
////        var appointment = CreateAppointmentWithRelations(
////            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
////            "John Doe", "Max", DateTime.UtcNow.AddDays(1), AppointmentStatus.Confirmed);

////        SetupAppointmentsDbSet([appointment]);

////        using var cancellationTokenSource = new CancellationTokenSource();
////        var cancellationToken = cancellationTokenSource.Token;

////        var handler = CreateHandler();
////        var request = CreateRequest(10, 1);

////        // Act
////        var result = await handler.Handle(request, cancellationToken);

////        // Assert
////        Assert.True(result.IsSuccess);
////        Assert.NotNull(result.Value);
////    }
////}
