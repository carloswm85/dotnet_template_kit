using NLayerTemplate.Data.Model;
using NLayerTemplate.Repository;
using NLayerTemplate.Service.Dtos.ContosoUniversity;
using NLayerTemplate.Service.Services.ExampleServices;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NLayerTemplate.Testing.Unit.Services;

public class ContosoUniversityServiceUnitTests
{
    [Fact]
    public async Task GetTestStudentAsync_AsNoTracking_ReturnsMappedTestStudentDto()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<TestStudent>>();
        var mockUow = new Mock<IUnitOfWork>();
        var mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<ContosoUniversityService>>();

        var student = new TestStudent
        {
            TestStudentId = 1,
            GovernmentId = "12345678",
            LastName = "Doe",
            FirstMidName = "John",
        };

        mockRepo
            .Setup(r =>
                r.GetByIdAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(student);
        mockUow.Setup(u => u.TestStudentRepository).Returns(mockRepo.Object);

        var expectedDto = new TestStudentDto
        {
            Id = 1,
            GovernmentId = "12345678",
            LastName = "Doe",
            FirstMidName = "John",
        };
        mockMapper
            .Setup(m => m.Map<TestStudentDto>(It.IsAny<TestStudent>()))
            .Returns(
                (TestStudent s) =>
                    new TestStudentDto
                    {
                        Id = s.TestStudentId,
                        GovernmentId = s.GovernmentId,
                        LastName = s.LastName,
                        FirstMidName = s.FirstMidName,
                    }
            );

        var service = new ContosoUniversityService(
            mockLogger.Object,
            mockUow.Object,
            mockMapper.Object
        );

        // Act
        var result = await service.GetTestStudentAsync(
            1,
            asNoTracking: true,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result!.Id);
        Assert.Equal(expectedDto.GovernmentId, result.GovernmentId);
        mockRepo.Verify(
            r => r.GetByIdAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateTestStudentAsync_AddsTestStudentAndReturnsId()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<TestStudent>>();
        var mockUow = new Mock<IUnitOfWork>();
        var mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<ContosoUniversityService>>();

        var dto = new TestStudentDto
        {
            GovernmentId = "ABC-12-3456",
            LastName = "Smith",
            FirstMidName = "Anna",
        };
        var mappedTestStudent = new TestStudent
        {
            GovernmentId = "ABC-12-3456",
            LastName = "Smith",
            FirstMidName = "Anna",
        };

        mockMapper.Setup(m => m.Map<TestStudent>(It.IsAny<TestStudentDto>())).Returns(mappedTestStudent);

        // When AddAsync is called, set the ID to simulate database behavior.
        mockRepo
            .Setup(r => r.AddAsync(It.IsAny<TestStudent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback<TestStudent, CancellationToken>((s, _) => s.TestStudentId = 42);

        mockUow.Setup(u => u.TestStudentRepository).Returns(mockRepo.Object);
        mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new ContosoUniversityService(
            mockLogger.Object,
            mockUow.Object,
            mockMapper.Object
        );

        // Act
        var createdId = await service.CreateTestStudentAsync(
            dto,
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal(42, createdId);
        mockRepo.Verify(
            r => r.AddAsync(It.IsAny<TestStudent>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
