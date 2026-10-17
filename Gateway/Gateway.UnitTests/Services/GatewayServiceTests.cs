using AutoFixture;
using AwesomeAssertions;
using Dapr.Client;
using Gateway.Daos;
using Gateway.Models;
using Gateway.Services;
using Moq;

namespace Gateway.UnitTests.Services;

[TestClass]
public class GatewayServiceTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<IFileDao> _fileDao = null!;
    private Mock<DaprClient> _daprClient = null!;
    private GatewayService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _fileDao = _mockRepository.Create<IFileDao>();
        _daprClient = _mockRepository.Create<DaprClient>();
        _sut = new GatewayService(_fileDao.Object, _daprClient.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    [TestMethod]
    [DataRow("picture.png", "png")]
    [DataRow("my.holiday.picture.jpg", "jpg")]
    [DataRow("noextension", "noextension")]
    public async Task ProcessRequest_ValidRequest_SavesFileAndPublishesMessage(string fileName, string expectedFileType)
    {
        var base64 = _fixture.Create<string>();
        var fileResponse = _fixture.Create<FileResponse>();
        _fileDao.Setup(d => d.Save(expectedFileType, base64)).ReturnsAsync(fileResponse);
        _daprClient
            .Setup(d => d.PublishEventAsync("messagebus", "message-received",
                It.Is<Message>(m => m.FileReference == fileResponse.FileName), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.ProcessRequest(new GatewayRequest(fileName, base64));

        _fileDao.Verify(d => d.Save(expectedFileType, base64), Times.Once);
        _daprClient.Verify(d => d.PublishEventAsync("messagebus", "message-received",
            It.Is<Message>(m => m.FileReference == fileResponse.FileName), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ProcessRequest_SaveFails_DoesNotPublishMessage()
    {
        var request = _fixture.Build<GatewayRequest>().With(r => r.FileName, "picture.png").Create();
        _fileDao.Setup(d => d.Save("png", request.Base64)).ThrowsAsync(new InvalidOperationException());

        var act = () => _sut.ProcessRequest(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
