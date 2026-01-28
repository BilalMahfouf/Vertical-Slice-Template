using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Common.Errors;
using VeterinaryApi.Domain.Clinics;
using VeterinaryApi.Features.Clinics;
using Xunit;

namespace Application.Tests.Clinics;

public class UpdateClinicTests
{
    private readonly Mock<IApplicationDbContext> _mockDbContext;

    public UpdateClinicTests()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
    }

    private UpdateClinic.UpdateClinicCommandHandler CreateHandler()
    {
        return new UpdateClinic.UpdateClinicCommandHandler(_mockDbContext.Object);
    }

    private void SetupClinicsDbSet(List<Clinic> clinics)
    {
        var mockDbSet = DbSetMockHelper.CreateMockDbSet(clinics);
        _mockDbContext.Setup(db => db.Clinics).Returns(mockDbSet.Object);
    }

    [Fact]
    public async Task Handle_WhenClinicExists_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Original Name", "111-1111", "Original Address", 5);
        
        // Use reflection to set the Id since it's inherited from Entity
        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new UpdateClinic.UpdateClinicCommand(
            clinicId,
            "Updated Name",
            "222-2222",
            "Updated Address");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Name", clinic.Name);
        Assert.Equal("222-2222", clinic.Phone);
        Assert.Equal("Updated Address", clinic.Address);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenClinicNotFound_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        SetupClinicsDbSet([]);

        var handler = CreateHandler();
        var command = new UpdateClinic.UpdateClinicCommand(
            nonExistentId,
            "New Name",
            "333-3333",
            "New Address");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Clinic.ClinicNotFound", result.Error.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Contains(nonExistentId.ToString(), result.Error.Description);

        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNameTooShort_ShouldThrowDomainException()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Original Name", "111-1111", "Original Address", 5);

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        var handler = CreateHandler();
        var command = new UpdateClinic.UpdateClinicCommand(
            clinicId,
            "AB", // Less than 3 characters
            "222-2222",
            "Updated Address");

        // Act & Assert
        await Assert.ThrowsAsync<VeterinaryApi.Domain.Common.DomainException>(
            () => handler.Handle(command, CancellationToken.None));

        // Original values should remain unchanged
        Assert.Equal("Original Name", clinic.Name);
    }

    [Fact]
    public async Task Handle_WhenNameExactlyMinLength_ShouldUpdateSuccessfully()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Original Name", "111-1111", "Original Address", 5);

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new UpdateClinic.UpdateClinicCommand(
            clinicId,
            "ABC", // Exactly 3 characters
            "222-2222",
            "Updated Address");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ABC", clinic.Name);
    }

    [Fact]
    public async Task Handle_WhenEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Original Name", "111-1111", "Original Address", 5);

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        var handler = CreateHandler();
        var command = new UpdateClinic.UpdateClinicCommand(
            clinicId,
            "",
            "222-2222",
            "Updated Address");

        // Act & Assert
        await Assert.ThrowsAsync<VeterinaryApi.Domain.Common.DomainException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithMultipleClinics_ShouldUpdateCorrectClinic()
    {
        // Arrange
        var clinicId1 = Guid.NewGuid();
        var clinicId2 = Guid.NewGuid();

        var clinic1 = Clinic.Create(Guid.NewGuid(), "Clinic 1", "111-1111", "Address 1", 5);
        var clinic2 = Clinic.Create(Guid.NewGuid(), "Clinic 2", "222-2222", "Address 2", 8);

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic1, clinicId1);
        idProperty!.SetValue(clinic2, clinicId2);

        SetupClinicsDbSet([clinic1, clinic2]);

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new UpdateClinic.UpdateClinicCommand(
            clinicId1,
            "Updated Clinic 1",
            "333-3333",
            "Updated Address 1");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Clinic 1", clinic1.Name);
        Assert.Equal("333-3333", clinic1.Phone);
        Assert.Equal("Updated Address 1", clinic1.Address);

        // clinic2 should remain unchanged
        Assert.Equal("Clinic 2", clinic2.Name);
        Assert.Equal("222-2222", clinic2.Phone);
        Assert.Equal("Address 2", clinic2.Address);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationTokenToDatabase()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Original Name", "111-1111", "Original Address", 5);

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new UpdateClinic.UpdateClinicCommand(
            clinicId,
            "Updated Name",
            "222-2222",
            "Updated Address");

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockDbContext.Verify(db => db.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateAllFieldsCorrectly()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var clinic = Clinic.Create(doctorId, "Original Name", "111-1111", "Original Address", 5);

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var newName = "Completely New Name";
        var newPhone = "999-888-7777";
        var newAddress = "456 New Street, New City";

        var command = new UpdateClinic.UpdateClinicCommand(
            clinicId,
            newName,
            newPhone,
            newAddress);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, clinic.Name);
        Assert.Equal(newPhone, clinic.Phone);
        Assert.Equal(newAddress, clinic.Address);
        // DoctorId should remain unchanged
        Assert.Equal(doctorId, clinic.DoctorId);
    }

    [Fact]
    public async Task Handle_WithLongName_ShouldUpdateSuccessfully()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Original Name", "111-1111", "Original Address", 5);

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var longName = "This is a very long clinic name that should still be valid for updates";

        var command = new UpdateClinic.UpdateClinicCommand(
            clinicId,
            longName,
            "222-2222",
            "Updated Address");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(longName, clinic.Name);
    }
}
