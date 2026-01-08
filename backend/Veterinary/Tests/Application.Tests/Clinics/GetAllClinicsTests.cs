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
//using VeterinaryApi.Common.Errors;
//using VeterinaryApi.Common.Paginations;
//using VeterinaryApi.Domain.Clinics;
//using VeterinaryApi.Domain.Users;
//using VeterinaryApi.Features.Clinics;
//using Xunit;

//namespace Application.Tests.Clinics;

//public class GetAllClinicsTests
//{
//    private readonly Mock<IApplicationDbContext> _mockDbContext;

//    public GetAllClinicsTests()
//    {
//        _mockDbContext = new Mock<IApplicationDbContext>();
//    }

//    private GetAllClinics.GetAllClinicsQueryHandler CreateHandler()
//    {
//        return new GetAllClinics.GetAllClinicsQueryHandler(_mockDbContext.Object);
//    }

//    private void SetupClinicsDbSet(List<Clinic> clinics)
//    {
//        var mockDbSet = DbSetMockHelper.CreateMockDbSet(clinics);
//        _mockDbContext.Setup(db => db.Clinics).Returns(mockDbSet.Object);
//    }

//    private Clinic CreateClinicWithId(Guid id, Guid doctorId, string name, string phone, string address, int staffCount, string doctorName)
//    {
//        var clinic = Clinic.Create(doctorId, name, phone, address, staffCount);

//        var idProperty = typeof(Clinic).BaseType!.GetProperty("Id");
//        idProperty!.SetValue(clinic, id);

//        var nameParts = doctorName.Split(' ', 2);
//        var firstName = nameParts.Length > 0 ? nameParts[0] : "Doctor";
//        var lastName = nameParts.Length > 1 ? nameParts[1] : "";
        
//        var doctor = User.Create(firstName, lastName, "doctor@test.com", "password123", UserRoles.Doctor);
//        var doctorProperty = typeof(User).BaseType!.GetProperty("Id");
//        doctorProperty!.SetValue(doctor, doctorId);

//        var clinicDoctorProperty = typeof(Clinic).GetProperty("Doctor");
//        clinicDoctorProperty!.SetValue(clinic, doctor);

//        return clinic;
//    }

//    private GetAllClinics.Request CreateRequest(int? pageSize = null, int? page = null, string? search = null, string? sortColumn = null, string? sortOrder = null)
//    {
//        var tableRequest = TableRequest.Create(pageSize, page, search, sortColumn, sortOrder);
        
//        var requestType = typeof(GetAllClinics.Request);
//        var request = (GetAllClinics.Request)Activator.CreateInstance(requestType)!;
        
//        var pageSizeProperty = typeof(TableRequest).GetProperty("PageSize");
//        pageSizeProperty!.SetValue(request, tableRequest.PageSize);
        
//        var pageProperty = typeof(TableRequest).GetProperty("Page");
//        pageProperty!.SetValue(request, tableRequest.Page);
        
//        var searchProperty = typeof(TableRequest).GetProperty("search");
//        searchProperty!.SetValue(request, tableRequest.search);
        
//        var sortColumnProperty = typeof(TableRequest).GetProperty("SortColumn");
//        sortColumnProperty!.SetValue(request, tableRequest.SortColumn);
        
//        var sortOrderProperty = typeof(TableRequest).GetProperty("SortOrder");
//        sortOrderProperty!.SetValue(request, tableRequest.SortOrder);
        
//        return request;
//    }

//    [Fact]
//    public async Task Handle_WithClinicsInDatabase_ShouldReturnPagedList()
//    {
//        // Arrange
//        var clinic1 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 1", "111-1111", "Address 1", 5, "Dr. Smith");
//        var clinic2 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 2", "222-2222", "Address 2", 8, "Dr. Jones");
//        var clinic3 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 3", "333-3333", "Address 3", 10, "Dr. Brown");

//        SetupClinicsDbSet([clinic1, clinic2, clinic3]);

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1);

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.NotNull(result.Value);
//        Assert.Equal(3, result.Value.TotalCount);
//        Assert.Equal(3, result.Value.Item.Count());
//    }

//    [Fact]
//    public async Task Handle_WhenNoClinicsExist_ShouldReturnFailure()
//    {
//        // Arrange
//        SetupClinicsDbSet([]);

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1);

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.False(result.IsSuccess);
//        Assert.NotNull(result.Error);
//        Assert.Equal("Clinic.ClinicsNotFound", result.Error.Code);
//    }

//    [Fact]
//    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
//    {
//        // Arrange
//        var clinics = new List<Clinic>();
//        for (int i = 1; i <= 15; i++)
//        {
//            var clinic = CreateClinicWithId(
//                Guid.NewGuid(),
//                Guid.NewGuid(),
//                $"Clinic {i}",
//                $"{i}11-1111",
//                $"Address {i}",
//                i,
//                $"Dr. Name {i}");
//            clinics.Add(clinic);
//        }

//        SetupClinicsDbSet(clinics);

//        var handler = CreateHandler();
//        var request = CreateRequest(5, 2);

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(15, result.Value.TotalCount);
//        Assert.Equal(5, result.Value.Item.Count());
//        Assert.Equal(2, result.Value.Page);
//        Assert.Equal(5, result.Value.PageSize);
//        Assert.True(result.Value.HasPreviousPage);
//        Assert.True(result.Value.HasNextPage);
//    }

//    [Fact]
//    public async Task Handle_WithSearchByClinicName_ShouldFilterResults()
//    {
//        // Arrange
//        var clinic1 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Pet Care Center", "111-1111", "Address 1", 5, "Dr. Smith");
//        var clinic2 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Animal Hospital", "222-2222", "Address 2", 8, "Dr. Jones");
//        var clinic3 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Pet Emergency Clinic", "333-3333", "Address 3", 10, "Dr. Brown");

//        SetupClinicsDbSet([clinic1, clinic2, clinic3]);

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1, "Pet");

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(2, result.Value.Item.Count());
//        Assert.Contains(result.Value.Item, c => c.ClinicName == "Pet Care Center");
//        Assert.Contains(result.Value.Item, c => c.ClinicName == "Pet Emergency Clinic");
//    }

//    [Fact]
//    public async Task Handle_WithSearchByPhone_ShouldFilterResults()
//    {
//        // Arrange
//        var clinic1 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 1", "555-1234", "Address 1", 5, "Dr. Smith");
//        var clinic2 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 2", "555-5678", "Address 2", 8, "Dr. Jones");
//        var clinic3 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 3", "123-4567", "Address 3", 10, "Dr. Brown");

//        SetupClinicsDbSet([clinic1, clinic2, clinic3]);

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1, "555");

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(2, result.Value.Item.Count());
//        Assert.All(result.Value.Item, c => Assert.Contains("555", c.Phone));
//    }

//    [Fact]
//    public async Task Handle_WithSearchByDoctorName_ShouldFilterResults()
//    {
//        // Arrange
//        var clinic1 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 1", "111-1111", "Address 1", 5, "Dr. Smith");
//        var clinic2 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 2", "222-2222", "Address 2", 8, "Dr. Jones");
//        var clinic3 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 3", "333-3333", "Address 3", 10, "Dr. Smith");

//        SetupClinicsDbSet([clinic1, clinic2, clinic3]);

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1, "Smith");

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(2, result.Value.Item.Count());
//        Assert.All(result.Value.Item, c => Assert.Contains("Smith", c.DoctorName));
//    }

//    [Fact]
//    public async Task Handle_WithSearchNoMatches_ShouldReturnEmptyList()
//    {
//        // Arrange
//        var clinic1 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 1", "111-1111", "Address 1", 5, "Dr. Smith");
//        var clinic2 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 2", "222-2222", "Address 2", 8, "Dr. Jones");

//        SetupClinicsDbSet([clinic1, clinic2]);

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1, "NonExistent");

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Empty(result.Value.Item);
//    }

//    [Fact]
//    public async Task Handle_ResponseShouldIncludeAllFields()
//    {
//        // Arrange
//        var clinicId = Guid.NewGuid();
//        var doctorId = Guid.NewGuid();
//        var clinic = CreateClinicWithId(clinicId, doctorId, "Test Clinic", "555-1234", "123 Main St", 5, "Dr. Johnson");

//        SetupClinicsDbSet([clinic]);

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1);

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        var response = result.Value.Item.First();

//        Assert.Equal(clinicId, response.Id);
//        Assert.Equal(doctorId, response.DoctorId);
//        Assert.Equal("Dr. Johnson", response.DoctorName);
//        Assert.Equal("Test Clinic", response.ClinicName);
//        Assert.Equal("555-1234", response.Phone);
//        Assert.Equal("123 Main St", response.Address);
//        Assert.Equal(5, response.StaffCount);
//        Assert.NotEqual(default(DateTime), response.CreatedOnUtc);
//    }

//    [Fact]
//    public async Task Handle_WithLastPage_ShouldNotHaveNextPage()
//    {
//        // Arrange
//        var clinics = new List<Clinic>();
//        for (int i = 1; i <= 8; i++)
//        {
//            var clinic = CreateClinicWithId(
//                Guid.NewGuid(),
//                Guid.NewGuid(),
//                $"Clinic {i}",
//                $"{i}11-1111",
//                $"Address {i}",
//                i,
//                $"Dr. Name {i}");
//            clinics.Add(clinic);
//        }

//        SetupClinicsDbSet(clinics);

//        var handler = CreateHandler();
//        var request = CreateRequest(5, 2);

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(8, result.Value.TotalCount);
//        Assert.Equal(3, result.Value.Item.Count());
//        Assert.True(result.Value.HasPreviousPage);
//        Assert.False(result.Value.HasNextPage);
//    }

//    [Fact]
//    public async Task Handle_WithFirstPage_ShouldNotHavePreviousPage()
//    {
//        // Arrange
//        var clinics = new List<Clinic>();
//        for (int i = 1; i <= 15; i++)
//        {
//            var clinic = CreateClinicWithId(
//                Guid.NewGuid(),
//                Guid.NewGuid(),
//                $"Clinic {i}",
//                $"{i}11-1111",
//                $"Address {i}",
//                i,
//                $"Dr. Name {i}");
//            clinics.Add(clinic);
//        }

//        SetupClinicsDbSet(clinics);

//        var handler = CreateHandler();
//        var request = CreateRequest(5, 1);

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.False(result.Value.HasPreviousPage);
//        Assert.True(result.Value.HasNextPage);
//    }

//    [Fact]
//    public async Task Handle_WhenCancellationRequested_ShouldPassCancellationTokenToDatabase()
//    {
//        // Arrange
//        var clinic = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Test Clinic", "123-456-7890", "123 Main St", 5, "Dr. Test");

//        SetupClinicsDbSet([clinic]);

//        using var cancellationTokenSource = new CancellationTokenSource();
//        var cancellationToken = cancellationTokenSource.Token;

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1);

//        // Act
//        var result = await handler.Handle(request, cancellationToken);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.NotNull(result.Value);
//    }

//    [Fact]
//    public async Task Handle_WithDefaultPageSize_ShouldReturnCorrectPageSize()
//    {
//        // Arrange
//        var clinics = new List<Clinic>();
//        for (int i = 1; i <= 20; i++)
//        {
//            var clinic = CreateClinicWithId(
//                Guid.NewGuid(),
//                Guid.NewGuid(),
//                $"Clinic {i}",
//                $"{i}11-1111",
//                $"Address {i}",
//                i,
//                $"Dr. Name {i}");
//            clinics.Add(clinic);
//        }

//        SetupClinicsDbSet(clinics);

//        var handler = CreateHandler();
//        var request = CreateRequest(null, null);

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(20, result.Value.TotalCount);
//        Assert.Equal(10, result.Value.Item.Count());
//        Assert.Equal(10, result.Value.PageSize);
//        Assert.Equal(1, result.Value.Page);
//    }

//    [Fact]
//    public async Task Handle_WithEmptySearchString_ShouldReturnAllClinics()
//    {
//        // Arrange
//        var clinic1 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 1", "111-1111", "Address 1", 5, "Dr. Smith");
//        var clinic2 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 2", "222-2222", "Address 2", 8, "Dr. Jones");
//        var clinic3 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 3", "333-3333", "Address 3", 10, "Dr. Brown");

//        SetupClinicsDbSet([clinic1, clinic2, clinic3]);

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1, "");

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(3, result.Value.Item.Count());
//    }

//    [Fact]
//    public async Task Handle_WithMultipleClinicsFromSameDoctor_ShouldReturnAll()
//    {
//        // Arrange
//        var doctorId = Guid.NewGuid();
//        var clinic1 = CreateClinicWithId(Guid.NewGuid(), doctorId, "Clinic 1", "111-1111", "Address 1", 5, "Dr. Smith");
//        var clinic2 = CreateClinicWithId(Guid.NewGuid(), doctorId, "Clinic 2", "222-2222", "Address 2", 8, "Dr. Smith");
//        var clinic3 = CreateClinicWithId(Guid.NewGuid(), Guid.NewGuid(), "Clinic 3", "333-3333", "Address 3", 10, "Dr. Jones");

//        SetupClinicsDbSet([clinic1, clinic2, clinic3]);

//        var handler = CreateHandler();
//        var request = CreateRequest(10, 1);

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(3, result.Value.Item.Count());
//        Assert.Equal(2, result.Value.Item.Count(c => c.DoctorId == doctorId));
//    }

//    [Fact]
//    public async Task Handle_TotalCountShouldReflectAllClinicsNotJustCurrentPage()
//    {
//        // Arrange
//        var clinics = new List<Clinic>();
//        for (int i = 1; i <= 25; i++)
//        {
//            var clinic = CreateClinicWithId(
//                Guid.NewGuid(),
//                Guid.NewGuid(),
//                $"Clinic {i}",
//                $"{i}11-1111",
//                $"Address {i}",
//                i,
//                $"Dr. Name {i}");
//            clinics.Add(clinic);
//        }

//        SetupClinicsDbSet(clinics);

//        var handler = CreateHandler();
//        var request = CreateRequest(5, 3);

//        // Act
//        var result = await handler.Handle(request, CancellationToken.None);

//        // Assert
//        Assert.True(result.IsSuccess);
//        Assert.Equal(25, result.Value.TotalCount);
//        Assert.Equal(5, result.Value.Item.Count());
//    }
//}
