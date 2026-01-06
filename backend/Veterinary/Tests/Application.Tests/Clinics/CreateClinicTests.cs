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

//public class CreateClinicTests
//{
//    private readonly Mock<IApplicationDbContext> _mockDbContext;
//    private readonly Mock<DbSet<Clinic>> _mockClinicDbSet;

//    public CreateClinicTests()
//    {
//        _mockDbContext = new Mock<IApplicationDbContext>();
//        _mockClinicDbSet = new Mock<DbSet<Clinic>>();
//        _mockDbContext.Setup(db => db.Clinics).Returns(_mockClinicDbSet.Object);
//    }

//    private CreateClinic.CreateClinicCommandHandler CreateHandler()
//    {
//        return new CreateClinic.CreateClinicCommandHandler(_mockDbContext.Object);
//    }

//    [Fact]
//    public async Task Handle_WithValidData_ShouldCreateClinicAndReturnSuccess()
//    {
//        // Arrange
//        var doctorId = Guid.NewGuid();
//        var name = "Test Clinic";
//        var phone = "123-456-7890";
//        var address = "123 Main St";
//        var staffCount = 5;

//        Clinic? capturedClinic = null;
//        _mockClinicDbSet
//            .Setup(db => db.Add(It.IsAny<Clinic>()))
//            .Callback<Clinic>(clinic => capturedClinic = clinic);

//        _mockDbContext
//            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(1);

//        var handler = CreateHandler();
//        var command = new CreateClinic.CreateClinicCommand(doctorId, name, phone, address, staffCount);

//        // Act
//        var result = await handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.NotNull(result.Value);
//        Assert.NotEqual(Guid.Empty, result.Value.clinicId);
//        Assert.NotNull(capturedClinic);
//        Assert.Equal(doctorId, capturedClinic.DoctorId);
//        Assert.Equal(name, capturedClinic.Name);
//        Assert.Equal(phone, capturedClinic.Phone);
//        Assert.Equal(address, capturedClinic.Address);
//        Assert.Equal(staffCount, capturedClinic.StaffCount);

//        _mockClinicDbSet.Verify(db => db.Add(It.IsAny<Clinic>()), Times.Once);
//        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithValidData_ShouldReturnClinicIdInResponse()
//    {
//        // Arrange
//        var doctorId = Guid.NewGuid();
//        var name = "Valid Clinic Name";
//        var phone = "555-1234";
//        var address = "456 Oak Ave";
//        var staffCount = 10;

//        Clinic? capturedClinic = null;
//        _mockClinicDbSet
//            .Setup(db => db.Add(It.IsAny<Clinic>()))
//            .Callback<Clinic>(clinic => capturedClinic = clinic);

//        _mockDbContext
//            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(1);

//        var handler = CreateHandler();
//        var command = new CreateClinic.CreateClinicCommand(doctorId, name, phone, address, staffCount);

//        // Act
//        var result = await handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.NotNull(capturedClinic);
//        Assert.Equal(capturedClinic.Id, result.Value.clinicId);
//    }

//    [Fact]
//    public async Task Handle_WithNameLessThanMinLength_ShouldThrowDomainException()
//    {
//        // Arrange
//        var doctorId = Guid.NewGuid();
//        var name = "AB"; // Less than 3 characters
//        var phone = "123-456-7890";
//        var address = "123 Main St";
//        var staffCount = 5;

//        var handler = CreateHandler();
//        var command = new CreateClinic.CreateClinicCommand(doctorId, name, phone, address, staffCount);

//        // Act & Assert
//        await Assert.ThrowsAsync<VeterinaryApi.Domain.Common.DomainException>(
//            () => handler.Handle(command, CancellationToken.None));

//        _mockClinicDbSet.Verify(db => db.Add(It.IsAny<Clinic>()), Times.Never);
//        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }

//    [Fact]
//    public async Task Handle_WithExactlyMinNameLength_ShouldCreateClinicSuccessfully()
//    {
//        // Arrange
//        var doctorId = Guid.NewGuid();
//        var name = "ABC"; // Exactly 3 characters (minimum)
//        var phone = "123-456-7890";
//        var address = "123 Main St";
//        var staffCount = 5;

//        _mockClinicDbSet
//            .Setup(db => db.Add(It.IsAny<Clinic>()));

//        _mockDbContext
//            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(1);

//        var handler = CreateHandler();
//        var command = new CreateClinic.CreateClinicCommand(doctorId, name, phone, address, staffCount);

//        // Act
//        var result = await handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        _mockClinicDbSet.Verify(db => db.Add(It.IsAny<Clinic>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithEmptyName_ShouldThrowDomainException()
//    {
//        // Arrange
//        var doctorId = Guid.NewGuid();
//        var name = "";
//        var phone = "123-456-7890";
//        var address = "123 Main St";
//        var staffCount = 5;

//        var handler = CreateHandler();
//        var command = new CreateClinic.CreateClinicCommand(doctorId, name, phone, address, staffCount);

//        // Act & Assert
//        await Assert.ThrowsAsync<VeterinaryApi.Domain.Common.DomainException>(
//            () => handler.Handle(command, CancellationToken.None));
//    }

//    [Fact]
//    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationTokenToDatabase()
//    {
//        // Arrange
//        var doctorId = Guid.NewGuid();
//        var name = "Test Clinic";
//        var phone = "123-456-7890";
//        var address = "123 Main St";
//        var staffCount = 5;

//        using var cancellationTokenSource = new CancellationTokenSource();
//        var cancellationToken = cancellationTokenSource.Token;

//        _mockDbContext
//            .Setup(db => db.SaveChangesAsync(cancellationToken))
//            .ReturnsAsync(1);

//        var handler = CreateHandler();
//        var command = new CreateClinic.CreateClinicCommand(doctorId, name, phone, address, staffCount);

//        // Act
//        var result = await handler.Handle(command, cancellationToken);

//        // Assert
//        Assert.True(result.IsSuccess);
//        _mockDbContext.Verify(db => db.SaveChangesAsync(cancellationToken), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithDifferentDoctorIds_ShouldCreateClinicsWithCorrectDoctorId()
//    {
//        // Arrange
//        var doctorId1 = Guid.NewGuid();
//        var doctorId2 = Guid.NewGuid();

//        var capturedClinics = new List<Clinic>();
//        _mockClinicDbSet
//            .Setup(db => db.Add(It.IsAny<Clinic>()))
//            .Callback<Clinic>(clinic => capturedClinics.Add(clinic));

//        _mockDbContext
//            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(1);

//        var handler = CreateHandler();

//        // Act
//        var command1 = new CreateClinic.CreateClinicCommand(doctorId1, "Clinic 1", "111-1111", "Address 1", 5);
//        var command2 = new CreateClinic.CreateClinicCommand(doctorId2, "Clinic 2", "222-2222", "Address 2", 8);

//        await handler.Handle(command1, CancellationToken.None);
//        await handler.Handle(command2, CancellationToken.None);

//        // Assert
//        Assert.Equal(2, capturedClinics.Count);
//        Assert.Equal(doctorId1, capturedClinics[0].DoctorId);
//        Assert.Equal(doctorId2, capturedClinics[1].DoctorId);
//    }

//    [Fact]
//    public async Task Handle_WithLongName_ShouldCreateClinicSuccessfully()
//    {
//        // Arrange
//        var doctorId = Guid.NewGuid();
//        var name = "This is a very long clinic name that should still be valid";
//        var phone = "123-456-7890";
//        var address = "123 Main St";
//        var staffCount = 5;

//        _mockClinicDbSet
//            .Setup(db => db.Add(It.IsAny<Clinic>()));

//        _mockDbContext
//            .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
//            .ReturnsAsync(1);

//        var handler = CreateHandler();
//        var command = new CreateClinic.CreateClinicCommand(doctorId, name, phone, address, staffCount);

//        // Act
//        var result = await handler.Handle(command, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//    }
//}
