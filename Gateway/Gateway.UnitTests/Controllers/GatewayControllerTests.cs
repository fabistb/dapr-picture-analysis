using AutoFixture;
using AwesomeAssertions;
using Gateway.Controllers;
using Gateway.Models;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Gateway.UnitTests.Controllers;

[TestClass]
public class GatewayControllerTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<IGatewayService> _gatewayService = null!;
    private GatewayController _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _gatewayService = _mockRepository.Create<IGatewayService>();
        _sut = new GatewayController(_gatewayService.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    [TestMethod]
    public async Task ReceiveRequest_ValidRequest_ProcessesRequestAndReturnsAccepted()
    {
        var request = _fixture.Create<GatewayRequest>();
        _gatewayService.Setup(s => s.ProcessRequest(request)).Returns(Task.CompletedTask);

        var result = await _sut.ReceiveRequest(request);

        result.Should().BeOfType<AcceptedResult>();
        _gatewayService.Verify(s => s.ProcessRequest(request), Times.Once);
    }

    [TestMethod]
    public async Task ReceiveRequest_ServiceThrows_PropagatesException()
    {
        var request = _fixture.Create<GatewayRequest>();
        _gatewayService.Setup(s => s.ProcessRequest(request)).ThrowsAsync(new InvalidOperationException());

        var act = () => _sut.ReceiveRequest(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
