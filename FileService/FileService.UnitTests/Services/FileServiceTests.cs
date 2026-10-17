using System.Text.Json;
using AutoFixture;
using AwesomeAssertions;
using Dapr.Client;
using FileService.Models;
using Moq;
using SUT = FileService.Services.FileService;

namespace FileService.UnitTests.Services;

[TestClass]
public class FileServiceTests
{
    private const string BindingName = "file-entry-storage-binding";

    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<DaprClient> _daprClient = null!;
    private SUT _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _daprClient = _mockRepository.Create<DaprClient>();
        _sut = new SUT(_daprClient.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    private void SetupCreateBinding()
    {
        _daprClient
            .Setup(d => d.InvokeBindingAsync(It.IsAny<BindingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BindingResponse(new BindingRequest(BindingName, "create"), Array.Empty<byte>(),
                new Dictionary<string, string>()));
    }

    [TestMethod]
    public async Task Save_ValidRequest_ReturnsFileNameWithFileType()
    {
        SetupCreateBinding();
        var request = new FileRequest(_fixture.Create<string>(), "png");

        var result = await _sut.Save(request);

        result.FileName.Should().EndWith(".png");
        Guid.TryParse(Path.GetFileNameWithoutExtension(result.FileName), out _).Should().BeTrue();
    }

    [TestMethod]
    public async Task Save_ValidRequest_InvokesCreateBindingWithBlobNameAndPayload()
    {
        var request = new FileRequest(_fixture.Create<string>(), "jpg");
        BindingRequest? captured = null;
        _daprClient
            .Setup(d => d.InvokeBindingAsync(It.IsAny<BindingRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BindingRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new BindingResponse(new BindingRequest(BindingName, "create"), Array.Empty<byte>(),
                new Dictionary<string, string>()));

        var result = await _sut.Save(request);

        captured.Should().NotBeNull();
        captured!.BindingName.Should().Be(BindingName);
        captured.Operation.Should().Be("create");
        captured.Metadata["blobName"].Should().Be(result.FileName);
        JsonSerializer.Deserialize<string>(captured.Data.Span).Should().Be(request.Base64);
        _daprClient.Verify(
            d => d.InvokeBindingAsync(It.IsAny<BindingRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Save_CalledTwice_ReturnsUniqueFileNames()
    {
        SetupCreateBinding();
        var request = new FileRequest(_fixture.Create<string>(), "png");

        var first = await _sut.Save(request);
        var second = await _sut.Save(request);

        first.FileName.Should().NotBe(second.FileName);
    }

    [TestMethod]
    public async Task Get_ExistingFile_ReturnsBase64OfBlobData()
    {
        var fileName = _fixture.Create<string>();
        var data = _fixture.CreateMany<byte>(32).ToArray();
        BindingRequest? captured = null;
        _daprClient
            .Setup(d => d.InvokeBindingAsync(It.IsAny<BindingRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BindingRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(new BindingResponse(new BindingRequest(BindingName, "get"), data,
                new Dictionary<string, string>()));

        var result = await _sut.Get(fileName);

        result.Base64.Should().Be(Convert.ToBase64String(data));
        result.FileType.Should().BeNull();
        captured!.BindingName.Should().Be(BindingName);
        captured.Operation.Should().Be("get");
        captured.Metadata["blobName"].Should().Be(fileName);
    }
}
