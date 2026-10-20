using AutoFixture;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UniqueNumberGenerator.Controllers;
using UniqueNumberGenerator.Models;
using UniqueNumberGenerator.Services;

namespace UniqueNumberGenerator.UnitTests.Controllers;

[TestClass]
public class UniqueNumberControllerTests
{
    private readonly Fixture _fixture = new();
    private MockRepository _mockRepository = null!;
    private Mock<IUniqueNumberService> _uniqueNumberService = null!;
    private UniqueNumberController _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _uniqueNumberService = _mockRepository.Create<IUniqueNumberService>();
        _sut = new UniqueNumberController(_uniqueNumberService.Object);
    }

    [TestCleanup]
    public void Cleanup() => _mockRepository.VerifyAll();

    [TestMethod]
    public async Task Create_ServiceReturnsNumber_ReturnsOkWithNumber()
    {
        var number = _fixture.Create<long>();
        _uniqueNumberService.Setup(s => s.GetNextNumber()).ReturnsAsync(number);

        var result = await _sut.Create();

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(new UniqueNumberResponse(number));
        _uniqueNumberService.Verify(s => s.GetNextNumber(), Times.Once);
    }
}
