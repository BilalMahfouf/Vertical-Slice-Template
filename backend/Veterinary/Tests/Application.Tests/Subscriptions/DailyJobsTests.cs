using Application.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reflection;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Appointments;
using VeterinaryApi.Domain.Common;
using VeterinaryApi.Domain.Subscriptions;
using VeterinaryApi.Domain.Users;
using VeterinaryApi.Domain.Vaccinations;
using VeterinaryApi.Domain.Visits;
using VeterinaryApi.Features.Subscriptions.BackgroundJobs;
using VeterinaryApi.Infrastructure.Notifications.Jobs;
using VeterinaryApi.Infrastructure.OutboxMessages;

namespace Application.Tests.Subscriptions;

public class DailyJobsTests
{
    [Fact]
    public async Task MarkPastDueDailyJob_WhenSubscriptionIsEligible_UpdatesStatusAndSaves()
    {
        var dbMock = new Mock<IApplicationDbContext>();

        var plan = SubscriptionPlan.Create("Standard", "standard", new Money(1000, "DZD"), "month");
        var subscription = Subscription.Create(Guid.NewGuid(), plan);
        subscription.Activate();
        SetPrivateProperty(subscription, nameof(Subscription.CurrentPeriodEnd), DateTime.UtcNow.AddDays(-2));

        var subscriptions = new List<Subscription> { subscription };
        var subscriptionsSet = DbSetMockHelper.CreateMockDbSet(subscriptions);

        dbMock.Setup(x => x.Subscriptions).Returns(subscriptionsSet.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new MarkPastDueSubscriptionDailyJob(dbMock.Object, NullLogger<MarkPastDueSubscriptionDailyJob>.Instance);

        await sut.Execute(Mock.Of<Quartz.IJobExecutionContext>(c => c.CancellationToken == CancellationToken.None));

        Assert.Equal(SubscriptionStatus.PastDue, subscription.Status);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkPastDueDailyJob_WhenQueryThrows_DoesNotThrow()
    {
        var dbMock = new Mock<IApplicationDbContext>();

        dbMock.Setup(x => x.Subscriptions).Throws(new InvalidOperationException("boom"));

        var sut = new MarkPastDueSubscriptionDailyJob(dbMock.Object, NullLogger<MarkPastDueSubscriptionDailyJob>.Instance);

        var exception = await Record.ExceptionAsync(() =>
            sut.Execute(Mock.Of<Quartz.IJobExecutionContext>(c => c.CancellationToken == CancellationToken.None)));

        Assert.Null(exception);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkExpiredDailyJob_WhenSubscriptionIsEligible_UpdatesStatusAndSaves()
    {
        var dbMock = new Mock<IApplicationDbContext>();

        var plan = SubscriptionPlan.Create("Standard", "standard", new Money(1000, "DZD"), "month");
        var subscription = Subscription.Create(Guid.NewGuid(), plan);
        subscription.Activate();
        subscription.MarkPastDue();
        SetPrivateProperty(subscription, nameof(Subscription.CurrentPeriodEnd), DateTime.UtcNow.AddDays(-3));

        var subscriptions = new List<Subscription> { subscription };
        var subscriptionsSet = DbSetMockHelper.CreateMockDbSet(subscriptions);

        dbMock.Setup(x => x.Subscriptions).Returns(subscriptionsSet.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new MarkExpiredSubscriptionDailyJob(dbMock.Object, NullLogger<MarkExpiredSubscriptionDailyJob>.Instance);

        await sut.Execute(Mock.Of<Quartz.IJobExecutionContext>(c => c.CancellationToken == CancellationToken.None));

        Assert.Equal(SubscriptionStatus.Expired, subscription.Status);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkExpiredDailyJob_WhenQueryThrows_DoesNotThrow()
    {
        var dbMock = new Mock<IApplicationDbContext>();

        dbMock.Setup(x => x.Subscriptions).Throws(new InvalidOperationException("boom"));

        var sut = new MarkExpiredSubscriptionDailyJob(dbMock.Object, NullLogger<MarkExpiredSubscriptionDailyJob>.Instance);

        var exception = await Record.ExceptionAsync(() =>
            sut.Execute(Mock.Of<Quartz.IJobExecutionContext>(c => c.CancellationToken == CancellationToken.None)));

        Assert.Null(exception);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CleanPendingDailyJob_WhenSubscriptionIsEligible_DeletesAndSaves()
    {
        var dbMock = new Mock<IApplicationDbContext>();

        var plan = SubscriptionPlan.Create("Standard", "standard", new Money(1000, "DZD"), "month");
        var subscription = Subscription.Create(Guid.NewGuid(), plan);
        subscription.CreatedOnUtc = DateTime.UtcNow.AddDays(-2);

        var subscriptions = new List<Subscription> { subscription };
        var subscriptionsSet = DbSetMockHelper.CreateMockDbSet(subscriptions);

        dbMock.Setup(x => x.Subscriptions).Returns(subscriptionsSet.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new CleanPendingSubscriptionsDailyJob(dbMock.Object, NullLogger<CleanPendingSubscriptionsDailyJob>.Instance);

        await sut.Execute(Mock.Of<Quartz.IJobExecutionContext>(c => c.CancellationToken == CancellationToken.None));

        Assert.True(subscription.IsDeleted);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CleanPendingDailyJob_WhenQueryThrows_DoesNotThrow()
    {
        var dbMock = new Mock<IApplicationDbContext>();

        dbMock.Setup(x => x.Subscriptions).Throws(new InvalidOperationException("boom"));

        var sut = new CleanPendingSubscriptionsDailyJob(dbMock.Object, NullLogger<CleanPendingSubscriptionsDailyJob>.Instance);

        var exception = await Record.ExceptionAsync(() =>
            sut.Execute(Mock.Of<Quartz.IJobExecutionContext>(c => c.CancellationToken == CancellationToken.None)));

        Assert.Null(exception);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DailyRemindersJob_WhenNoEligibleEntities_DoesNotSave()
    {
        var dbMock = new Mock<IApplicationDbContext>();

        dbMock.Setup(x => x.Appointments).Returns(DbSetMockHelper.CreateMockDbSet(new List<Appointment>()).Object);
        dbMock.Setup(x => x.Vaccinations).Returns(DbSetMockHelper.CreateMockDbSet(new List<Vaccination>()).Object);
        dbMock.Setup(x => x.Visits).Returns(DbSetMockHelper.CreateMockDbSet(new List<Visit>()).Object);
        dbMock.Setup(x => x.Users).Returns(DbSetMockHelper.CreateMockDbSet(new List<User>()).Object);
        dbMock.Setup(x => x.OutboxMessages).Returns(DbSetMockHelper.CreateMockDbSet(new List<OutboxMessage>()).Object);

        var sut = new DailyRemindersJob(dbMock.Object, NullLogger<DailyRemindersJob>.Instance);

        await sut.Execute(Mock.Of<Quartz.IJobExecutionContext>(c => c.CancellationToken == CancellationToken.None));

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DailyRemindersJob_WhenQueryThrows_DoesNotThrow()
    {
        var dbMock = new Mock<IApplicationDbContext>();

        dbMock.Setup(x => x.Appointments).Throws(new InvalidOperationException("boom"));

        var sut = new DailyRemindersJob(dbMock.Object, NullLogger<DailyRemindersJob>.Instance);

        var exception = await Record.ExceptionAsync(() =>
            sut.Execute(Mock.Of<Quartz.IJobExecutionContext>(c => c.CancellationToken == CancellationToken.None)));

        Assert.Null(exception);
    }

    private static void SetPrivateProperty<TObject, TValue>(TObject target, string propertyName, TValue value)
    {
        var property = typeof(TObject).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' was not found.");
        }

        property.SetValue(target, value);
    }
}
