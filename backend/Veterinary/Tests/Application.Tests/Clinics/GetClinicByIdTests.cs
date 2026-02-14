//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Application.Tests.Helpers;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using VeterinaryApi.Common.Abstracions;
//using VeterinaryApi.Common.Errors;
//using VeterinaryApi.Domain.Clinics;
//using VeterinaryApi.Features.Clinics;
//using Xunit;

//namespace Application.Tests.Clinics;

//public class GetClinicByIdTests
//{
//    private readonly Mock<IApplicationDbContext> _mockDbContext;

//    public GetClinicByIdTests()
//    {
//        _mockDbContext = new Mock<IApplicationDbContext>();
//    }

//    //private GetClinicById.GetClinicByIdQueryHandler CreateHandler()
//    //{
//    //    return new GetClinicById.GetClinicByIdQueryHandler(_mockDbContext.Object);
//    //}

//    private void SetupClinicsDbSet(List<Clinic> clinics)
//    {
//        var mockDbSet = DbSetMockHelper.CreateMockDbSet(clinics);
//        _mockDbContext.Setup(db => db.Clinics).Returns(mockDbSet.Object);
//    }

//    [Fact]
//    public async Task Handle_WhenClinicExists_ShouldReturnSuccessWithClinicData()
//    {
//        // Arrange
//        var clinicId = Guid.NewGuid();
//        var doctorId = Guid.NewGuid();
//        var name = "Test Clinic";
//        var phone = "123-456-7890";
//        var address = "123 Main St";

//        var clinic = Clinic.Create(doctorId, name, phone, address, 5);

//        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(clinic, clinicId);

//        SetupClinicsDbSet([clinic]);

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(clinicId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.NotNull(result.Value);
//        Assert.Equal(clinicId, result.Value.Id);
//        Assert.Equal(name, result.Value.Name);
//        Assert.Equal(phone, result.Value.Phone);
//        Assert.Equal(address, result.Value.Address);
//    }

//    [Fact]
//    public async Task Handle_WhenClinicNotFound_ShouldReturnFailure()
//    {
//        // Arrange
//        var nonExistentId = Guid.NewGuid();
//        SetupClinicsDbSet([]);

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(nonExistentId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.False(result.IsSuccess);
//        Assert.NotNull(result.Error);
//        Assert.Equal("Clinic.ClinicNotFound", result.Error.Code);
//        Assert.Equal(ErrorType.NotFound, result.Error.Type);
//        Assert.Contains(nonExistentId.ToString(), result.Error.Description);
//    }

//    [Fact]
//    public async Task Handle_WithMultipleClinics_ShouldReturnCorrectClinic()
//    {
//        // Arrange
//        var clinicId1 = Guid.NewGuid();
//        var clinicId2 = Guid.NewGuid();
//        var clinicId3 = Guid.NewGuid();

//        var clinic1 = Clinic.Create(Guid.NewGuid(), "Clinic 1", "111-1111", "Address 1", 5);
//        var clinic2 = Clinic.Create(Guid.NewGuid(), "Clinic 2", "222-2222", "Address 2", 8);
//        var clinic3 = Clinic.Create(Guid.NewGuid(), "Clinic 3", "333-3333", "Address 3", 10);

//        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(clinic1, clinicId1);
//        idProperty!.SetValue(clinic2, clinicId2);
//        idProperty!.SetValue(clinic3, clinicId3);

//        SetupClinicsDbSet([clinic1, clinic2, clinic3]);

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(clinicId2);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.NotNull(result.Value);
//        Assert.Equal(clinicId2, result.Value.Id);
//        Assert.Equal("Clinic 2", result.Value.Name);
//        Assert.Equal("222-2222", result.Value.Phone);
//        Assert.Equal("Address 2", result.Value.Address);
//    }

//    [Fact]
//    public async Task Handle_WithEmptyGuid_ShouldReturnNotFound()
//    {
//        // Arrange
//        var clinic = Clinic.Create(Guid.NewGuid(), "Test Clinic", "123-456-7890", "123 Main St", 5);
//        SetupClinicsDbSet([clinic]);

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(Guid.Empty);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.False(result.IsSuccess);
//        Assert.Equal("Clinic.ClinicNotFound", result.Error.Code);
//    }

//    [Fact]
//    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationTokenToDatabase()
//    {
//        // Arrange
//        var clinicId = Guid.NewGuid();
//        var clinic = Clinic.Create(Guid.NewGuid(), "Test Clinic", "123-456-7890", "123 Main St", 5);

//        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(clinic, clinicId);

//        SetupClinicsDbSet([clinic]);

//        using var cancellationTokenSource = new CancellationTokenSource();
//        var cancellationToken = cancellationTokenSource.Token;

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(clinicId);

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
//        var clinicId = Guid.NewGuid();
//        var doctorId = Guid.NewGuid();
//        var expectedName = "Full Details Clinic";
//        var expectedPhone = "555-123-4567";
//        var expectedAddress = "789 Healthcare Blvd, Suite 100";

//        var clinic = Clinic.Create(doctorId, expectedName, expectedPhone, expectedAddress, 5);

//        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(clinic, clinicId);

//        SetupClinicsDbSet([clinic]);

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(clinicId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        var response = result.Value;

//        Assert.Equal(clinicId, response.Id);
//        Assert.Equal(expectedName, response.Name);
//        Assert.Equal(expectedPhone, response.Phone);
//        Assert.Equal(expectedAddress, response.Address);
//    }

//    [Fact]
//    public async Task Handle_WhenQueryingNonExistentClinicWithOtherClinicsPresent_ShouldReturnNotFound()
//    {
//        // Arrange
//        var existingClinicId = Guid.NewGuid();
//        var nonExistentClinicId = Guid.NewGuid();

//        var existingClinic = Clinic.Create(Guid.NewGuid(), "Existing Clinic", "111-1111", "Address", 5);

//        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(existingClinic, existingClinicId);

//        SetupClinicsDbSet([existingClinic]);

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(nonExistentClinicId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.False(result.IsSuccess);
//        Assert.Equal("Clinic.ClinicNotFound", result.Error.Code);
//        Assert.Contains(nonExistentClinicId.ToString(), result.Error.Description);
//    }

//    [Fact]
//    public async Task Handle_WithSpecialCharactersInData_ShouldReturnCorrectly()
//    {
//        // Arrange
//        var clinicId = Guid.NewGuid();
//        var name = "Dr. Smith's Clinic & Associates";
//        var phone = "+1 (555) 123-4567";
//        var address = "123 Main St, Suite #100";

//        var clinic = Clinic.Create(Guid.NewGuid(), name, phone, address, 5);

//        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(clinic, clinicId);

//        SetupClinicsDbSet([clinic]);

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(clinicId);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(name, result.Value.Name);
//        Assert.Equal(phone, result.Value.Phone);
//        Assert.Equal(address, result.Value.Address);
//    }

//    [Fact]
//    public async Task Handle_QueryFirstClinicInList_ShouldReturnCorrectClinic()
//    {
//        // Arrange
//        var clinicId1 = Guid.NewGuid();
//        var clinicId2 = Guid.NewGuid();

//        var clinic1 = Clinic.Create(Guid.NewGuid(), "First Clinic", "111-1111", "First Address", 5);
//        var clinic2 = Clinic.Create(Guid.NewGuid(), "Second Clinic", "222-2222", "Second Address", 8);

//        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(clinic1, clinicId1);
//        idProperty!.SetValue(clinic2, clinicId2);

//        SetupClinicsDbSet([clinic1, clinic2]);

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(clinicId1);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal("First Clinic", result.Value.Name);
//    }

//    [Fact]
//    public async Task Handle_QueryLastClinicInList_ShouldReturnCorrectClinic()
//    {
//        // Arrange
//        var clinicId1 = Guid.NewGuid();
//        var clinicId2 = Guid.NewGuid();

//        var clinic1 = Clinic.Create(Guid.NewGuid(), "First Clinic", "111-1111", "First Address", 5);
//        var clinic2 = Clinic.Create(Guid.NewGuid(), "Last Clinic", "222-2222", "Last Address", 8);

//        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(clinic1, clinicId1);
//        idProperty!.SetValue(clinic2, clinicId2);

//        SetupClinicsDbSet([clinic1, clinic2]);

//        var handler = CreateHandler();
//        var query = new GetClinicById.Query(clinicId2);

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal("Last Clinic", result.Value.Name);
//    }
//}
