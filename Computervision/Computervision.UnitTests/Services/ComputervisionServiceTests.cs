using AutoFixture;
using AwesomeAssertions;
using Computervision.Daos;
using Computervision.Models;
using Computervision.Services;
using Dapr.Client;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Moq;

namespace Computervision.UnitTests.Services;

[TestClass]
public class ComputervisionServiceTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<IFileDao> _fileDao = null!;
    private Mock<IAnalysisService> _analysisService = null!;
    private Mock<DaprClient> _daprClient = null!;
    private ComputervisionService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _fileDao = _mockRepository.Create<IFileDao>();
        _analysisService = _mockRepository.Create<IAnalysisService>();
        _daprClient = _mockRepository.Create<DaprClient>();
        _sut = new ComputervisionService(_fileDao.Object, _analysisService.Object, _daprClient.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    private void SetupAnalysis(string fileReference, params Category[] categories)
    {
        var base64 = _fixture.Create<string>();
        _fileDao.Setup(d => d.GetPicture(fileReference)).ReturnsAsync(base64);
        _analysisService.Setup(a => a.AnalyzeImage(base64)).ReturnsAsync(categories.ToList());
    }

    [TestMethod]
    public async Task ProcessImage_CatWithHighScore_PublishesNotification()
    {
        var fileReference = _fixture.Create<string>();
        SetupAnalysis(fileReference, new Category("animal_cat", 0.9));
        _daprClient
            .Setup(d => d.PublishEventAsync("messagebus", "notification-received",
                It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.ProcessImage(fileReference);

        _daprClient.Verify(d => d.PublishEventAsync("messagebus", "notification-received",
            It.Is<NotificationMessage>(m => m.Message.Contains("cat")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ProcessImage_CatNameIsCaseInsensitive_PublishesNotification()
    {
        var fileReference = _fixture.Create<string>();
        SetupAnalysis(fileReference, new Category("Animal_CAT", 0.8));
        _daprClient
            .Setup(d => d.PublishEventAsync("messagebus", "notification-received",
                It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.ProcessImage(fileReference);

        _daprClient.Verify(d => d.PublishEventAsync("messagebus", "notification-received",
            It.IsAny<NotificationMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ProcessImage_CatWithLowScore_DoesNotPublish()
    {
        var fileReference = _fixture.Create<string>();
        SetupAnalysis(fileReference, new Category("animal_cat", 0.5));

        await _sut.ProcessImage(fileReference);
    }

    [TestMethod]
    public async Task ProcessImage_NoCat_DoesNotPublish()
    {
        var fileReference = _fixture.Create<string>();
        SetupAnalysis(fileReference, new Category("outdoor_mountain", 0.99));

        await _sut.ProcessImage(fileReference);
    }

    [TestMethod]
    public async Task ProcessImage_NoCategories_DoesNotPublish()
    {
        var fileReference = _fixture.Create<string>();
        SetupAnalysis(fileReference);

        await _sut.ProcessImage(fileReference);
    }

    [TestMethod]
    public async Task ProcessImage_FileDaoThrows_ExceptionIsPropagated()
    {
        var fileReference = _fixture.Create<string>();
        _fileDao.Setup(d => d.GetPicture(fileReference)).ThrowsAsync(new InvalidOperationException());

        var act = () => _sut.ProcessImage(fileReference);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
