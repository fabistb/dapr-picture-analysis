using AutoFixture;
using AwesomeAssertions;
using FileService.Controllers;
using FileService.Models;
using FileService.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FileService.UnitTests.Controllers;

[TestClass]
public class FileControllerTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<IFileService> _fileService = null!;
    private FileController _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _fileService = _mockRepository.Create<IFileService>();
        _sut = new FileController(_fileService.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    [TestMethod]
    public async Task Save_ValidRequest_ReturnsOkWithResponse()
    {
        var request = _fixture.Create<FileRequest>();
        var response = _fixture.Create<FileResponse>();
        _fileService.Setup(s => s.Save(request)).ReturnsAsync(response);

        var result = await _sut.Save(request);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(response);
        _fileService.Verify(s => s.Save(request), Times.Once);
    }

    [TestMethod]
    public async Task Get_ExistingFile_ReturnsOkWithFile()
    {
        var fileName = _fixture.Create<string>();
        var file = _fixture.Create<FileRequest>();
        _fileService.Setup(s => s.Get(fileName)).ReturnsAsync(file);

        var result = await _sut.Get(fileName);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(file);
        _fileService.Verify(s => s.Get(fileName), Times.Once);
    }
}
