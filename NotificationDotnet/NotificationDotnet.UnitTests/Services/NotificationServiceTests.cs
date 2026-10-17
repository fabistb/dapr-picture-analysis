using System.Text.Json;
using AutoFixture;
using AwesomeAssertions;
using Dapr.Client;
using Moq;
using NotificationDotnet.Models;
using NotificationDotnet.Services;

namespace NotificationDotnet.UnitTests.Services;

[TestClass]
public class NotificationServiceTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<DaprClient> _daprClient = null!;
    private NotificationService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _daprClient = _mockRepository.Create<DaprClient>();
        _sut = new NotificationService(_daprClient.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    [TestMethod]
    public async Task Process_ValidMessage_InvokesCreateBinding()
    {
        var message = _fixture.Create<NotificationMessage>();
        BindingRequest? captured = null;
        _daprClient
            .Setup(d => d.InvokeBindingAsync(It.IsAny<BindingRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BindingRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new BindingResponse(new BindingRequest("notification-storage", "create"), ReadOnlyMemory<byte>.Empty, new Dictionary<string, string>()));

        await _sut.Process(message);

        captured.Should().NotBeNull();
        captured!.BindingName.Should().Be("notification-storage");
        captured.Operation.Should().Be("create");
        JsonSerializer.Deserialize<string>(captured.Data.Span).Should().Be(message.Message);
    }

    [TestMethod]
    public async Task Process_BindingFails_ThrowsException()
    {
        var message = _fixture.Create<NotificationMessage>();
        _daprClient
            .Setup(d => d.InvokeBindingAsync(It.IsAny<BindingRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        var act = () => _sut.Process(message);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
