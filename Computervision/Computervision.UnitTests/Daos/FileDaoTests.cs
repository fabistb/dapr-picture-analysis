using System.Net;
using System.Text.Json;
using AutoFixture;
using AwesomeAssertions;
using Computervision.Daos;
using Computervision.Models;
using Dapr.Client;
using Moq;

namespace Computervision.UnitTests.Daos;

[TestClass]
public class FileDaoTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<DaprClient> _daprClient = null!;
    private FileDao _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _daprClient = _mockRepository.Create<DaprClient>();
        _sut = new FileDao(_daprClient.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    private void SetupInvoke(string fileReference, HttpResponseMessage response)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost/api/v1.0/File/{fileReference}");
        _daprClient
            .Setup(d => d.CreateInvokeMethodRequest(HttpMethod.Get, "file-service", $"api/v1.0/File/{fileReference}"))
            .Returns(request);
        _daprClient
            .Setup(d => d.InvokeMethodWithResponseAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    [TestMethod]
    public async Task GetPicture_SuccessResponse_ReturnsBase64()
    {
        var fileReference = _fixture.Create<string>();
        var base64 = _fixture.Create<string>();
        SetupInvoke(fileReference, new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new FileResponse(base64)))
        });

        var result = await _sut.GetPicture(fileReference);

        result.Should().Be(base64);
    }

    [TestMethod]
    public async Task GetPicture_ErrorResponse_Throws()
    {
        var fileReference = _fixture.Create<string>();
        SetupInvoke(fileReference, new HttpResponseMessage(HttpStatusCode.NotFound));

        var act = () => _sut.GetPicture(fileReference);

        await act.Should().ThrowAsync<Exception>();
    }
}
