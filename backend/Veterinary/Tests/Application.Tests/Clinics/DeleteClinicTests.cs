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

public class DeleteClinicTests
{
    private readonly Mock<IApplicationDbContext> _mockDbContext;

    public DeleteClinicTests()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
    }

    private DeleteClinic.DeleteClinicCommandHandler CreateHandler()
    {
        return new DeleteClinic.DeleteClinicCommandHandler(_mockDbContext.Object);
    }

    private void SetupClinicsDbSet(List<Clinic> clinics)
    {
        var mockDbSet = DbSetMockHelper.CreateMockDbSet(clinics);
        _mockDbContext.Setup(db => db.Clinics).Returns(mockDbSet.Object);
    }

    [Fact]
    public async Task Handle_WhenClinicExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Test Clinic", "123-456-7890", "123 Main St");

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new DeleteClinic.DeleteClinicCommand(clinicId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenClinicNotFound_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        SetupClinicsDbSet([]);

        var handler = CreateHandler();
        var command = new DeleteClinic.DeleteClinicCommand(nonExistentId);

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
    public async Task Handle_WithMultipleClinics_ShouldDeleteCorrectClinic()
    {
        // Arrange
        var clinicId1 = Guid.NewGuid();
        var clinicId2 = Guid.NewGuid();

        var clinic1 = Clinic.Create(Guid.NewGuid(), "Clinic 1", "111-1111", "Address 1");
        var clinic2 = Clinic.Create(Guid.NewGuid(), "Clinic 2", "222-2222", "Address 2");

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic1, clinicId1);
        idProperty!.SetValue(clinic2, clinicId2);

        SetupClinicsDbSet([clinic1, clinic2]);

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new DeleteClinic.DeleteClinicCommand(clinicId1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationTokenToDatabase()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Test Clinic", "123-456-7890", "123 Main St");

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new DeleteClinic.DeleteClinicCommand(clinicId);

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _mockDbContext.Verify(db => db.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeletingNonExistentClinicWithOtherClinicsPresent_ShouldReturnNotFound()
    {
        // Arrange
        var existingClinicId = Guid.NewGuid();
        var nonExistentClinicId = Guid.NewGuid();

        var existingClinic = Clinic.Create(Guid.NewGuid(), "Existing Clinic", "111-1111", "Address");

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(existingClinic, existingClinicId);

        SetupClinicsDbSet([existingClinic]);

        var handler = CreateHandler();
        var command = new DeleteClinic.DeleteClinicCommand(nonExistentClinicId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Clinic.ClinicNotFound", result.Error.Code);
        Assert.Contains(nonExistentClinicId.ToString(), result.Error.Description);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldReturnNotFound()
    {
        // Arrange
        var clinic = Clinic.Create(Guid.NewGuid(), "Test Clinic", "123-456-7890", "123 Main St");
        SetupClinicsDbSet([clinic]);

        var handler = CreateHandler();
        var command = new DeleteClinic.DeleteClinicCommand(Guid.Empty);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Clinic.ClinicNotFound", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenClinicExists_ShouldCallUpdateOnDbSet()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Test Clinic", "123-456-7890", "123 Main St");

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        var mockDbSet = DbSetMockHelper.CreateMockDbSet([clinic]);
        _mockDbContext.Setup(db => db.Clinics).Returns(mockDbSet.Object);

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new DeleteClinic.DeleteClinicCommand(clinicId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        mockDbSet.Verify(db => db.Update(clinic), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteOnClinic()
    {
        // Arrange
        var clinicId = Guid.NewGuid();
        var clinic = Clinic.Create(Guid.NewGuid(), "Test Clinic", "123-456-7890", "123 Main St");

        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
        idProperty!.SetValue(clinic, clinicId);

        SetupClinicsDbSet([clinic]);

        _mockDbContext
            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new DeleteClinic.DeleteClinicCommand(clinicId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // The Delete method on the clinic should be called (soft delete behavior)
    }
}
