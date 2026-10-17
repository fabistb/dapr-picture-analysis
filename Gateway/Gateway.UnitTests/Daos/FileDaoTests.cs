using System.Net;
using System.Text.Json;
using AutoFixture;
using AwesomeAssertions;
using Dapr.Client;
using Gateway.Daos;
using Gateway.Models;
using Gateway.Models.Exceptions;
using Moq;

namespace Gateway.UnitTests.Daos;

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

    private void SetupInvoke(string fileType, string base64, HttpResponseMessage response)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/v1.0/File");
        _daprClient
            .Setup(d => d.CreateInvokeMethodRequest(HttpMethod.Post, "file-service", "api/v1.0/File",
                It.IsAny<IReadOnlyCollection<KeyValuePair<string, string>>>(),
                It.Is<FileRequest>(r => r.Base64 == base64 && r.FileType == fileType)))
            .Returns(request);
        _daprClient
            .Setup(d => d.InvokeMethodWithResponseAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    [TestMethod]
    public async Task Save_SuccessResponse_ReturnsFileResponse()
    {
        var fileType = _fixture.Create<string>();
        var base64 = _fixture.Create<string>();
        var expected = _fixture.Create<FileResponse>();
        SetupInvoke(fileType, base64, new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(expected))
        });

        var result = await _sut.Save(fileType, base64);

        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task Save_CaseInsensitivePropertyNames_ReturnsFileResponse()
    {
        var fileType = _fixture.Create<string>();
        var base64 = _fixture.Create<string>();
        SetupInvoke(fileType, base64, new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"fileName\":\"abc.png\"}")
        });

        var result = await _sut.Save(fileType, base64);

        result.FileName.Should().Be("abc.png");
    }

    [TestMethod]
    public async Task Save_ErrorResponse_ThrowsServiceInvocationException()
    {
        var fileType = _fixture.Create<string>();
        var base64 = _fixture.Create<string>();
        SetupInvoke(fileType, base64, new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var act = () => _sut.Save(fileType, base64);

        (await act.Should().ThrowAsync<ServiceInvocationException>())
            .WithMessage("*InternalServerError*");
    }
}
