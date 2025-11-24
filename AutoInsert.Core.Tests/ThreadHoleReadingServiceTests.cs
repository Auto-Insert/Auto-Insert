using System.Threading.Tasks;
using AutoInsert.Core.Services;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Tests.Services
{
    public class ThreadHoleReadingServiceTests
    {
        [Fact]
        public async Task ReadThreadHolesFromFile_ValidFile_ReturnsThreadHoles()
        {
            // Arrange
            var testFilePath = Path.Combine(Path.GetTempPath(), "test_plugs.txt");
            var testData = "M6;10,5;20,3;30,1;0,0;0,0;1,0\nM8;15,2;25,4;35,6;1,0;0,0;0,0";
            File.WriteAllText(testFilePath, testData);

            // Act
            var result = await ThreadHoleReadingService.ReadThreadHolesFromFile(testFilePath);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("M6", result[0].PlugType);
            Assert.Equal(10.5, result[0].PositionX);
            Assert.Equal(20.3, result[0].PositionY);
            Assert.Equal(30.1, result[0].PositionZ);

            // Cleanup
            File.Delete(testFilePath);
        }

        [Fact]
        public async Task ReadThreadHolesFromFile_EmptyFile_ReturnsEmptyList()
        {
            // Arrange
            var testFilePath = Path.Combine(Path.GetTempPath(), "empty_plugs.txt");
            File.WriteAllText(testFilePath, string.Empty);

            // Act
            var result = await ThreadHoleReadingService.ReadThreadHolesFromFile(testFilePath);

            // Assert
            Assert.Empty(result);

            // Cleanup
            File.Delete(testFilePath);
        }

        [Fact]
        public async Task ReadThreadHolesFromFile_InvalidLine_SkipsInvalidLine()
        {
            // Arrange
            var testFilePath = Path.Combine(Path.GetTempPath(), "invalid_plugs.txt");
            var testData = "M6;10,5;20,3;30,1;0,0;0,0;1,0\nInvalidLine\nM8;15,2;25,4;35,6;1,0;0,0;0,0";
            File.WriteAllText(testFilePath, testData);

            // Act
            var result = await ThreadHoleReadingService.ReadThreadHolesFromFile(testFilePath);

            // Assert
            Assert.Equal(2, result.Count);

            // Cleanup
            File.Delete(testFilePath);
        }
    }
}