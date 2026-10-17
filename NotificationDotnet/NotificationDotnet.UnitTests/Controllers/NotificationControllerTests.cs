using AutoFixture;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationDotnet.Controllers;
using NotificationDotnet.Models;
using NotificationDotnet.Services;

namespace NotificationDotnet.UnitTests.Controllers;

[TestClass]
public class NotificationControllerTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<INotificationService> _notificationService = null!;
    private NotificationController _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _notificationService = _mockRepository.Create<INotificationService>();
        _sut = new NotificationController(_notificationService.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    [TestMethod]
    public async Task Notification_ValidMessage_ProcessesAndReturnsOk()
    {
        var message = _fixture.Create<NotificationMessage>();
        _notificationService.Setup(s => s.Process(message)).Returns(Task.CompletedTask);

        var result = await _sut.Notification(message);

        result.Should().BeOfType<OkResult>();
        _notificationService.Verify(s => s.Process(message), Times.Once);
    }

    [TestMethod]
    public async Task Notification_ServiceFails_PropagatesException()
    {
        var message = _fixture.Create<NotificationMessage>();
        _notificationService.Setup(s => s.Process(message)).ThrowsAsync(new InvalidOperationException());

        var act = () => _sut.Notification(message);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
