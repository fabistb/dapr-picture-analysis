using AutoFixture;
using AwesomeAssertions;
using Computervision.Controllers;
using Computervision.Models;
using Computervision.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Computervision.UnitTests.Controllers;

[TestClass]
public class ComputervisionControllerTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<IComputervisionService> _computervisionService = null!;
    private ComputervisionController _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _computervisionService = _mockRepository.Create<IComputervisionService>();
        _sut = new ComputervisionController(_computervisionService.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    [TestMethod]
    public async Task ProcessImage_ValidMessage_CallsServiceAndReturnsOk()
    {
        var message = _fixture.Create<Message>();
        _computervisionService.Setup(s => s.ProcessImage(message.FileReference)).Returns(Task.CompletedTask);

        var result = await _sut.ProcessImage(message);

        result.Should().BeOfType<OkResult>();
    }

    [TestMethod]
    public async Task ProcessImage_ServiceThrows_ExceptionIsPropagated()
    {
        var message = _fixture.Create<Message>();
        _computervisionService.Setup(s => s.ProcessImage(message.FileReference))
            .ThrowsAsync(new InvalidOperationException());

        var act = () => _sut.ProcessImage(message);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
