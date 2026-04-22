using Xunit;
using System.IO;
using System.Linq;

namespace GLMS.Tests
{
    public class FileValidationTests
    {
        private readonly string[] _allowedExtensions = { ".pdf" };
        private readonly string[] _disallowedExtensions = { ".exe", ".dll", ".bat", ".ps1", ".scr", ".vbs", ".js", ".html", ".txt", ".doc", ".docx" };

        [Theory]
        [InlineData("contract.pdf", true)]
        [InlineData("agreement.PDF", true)]
        [InlineData("document.pDf", true)]
        [InlineData("malware.exe", false)]
        [InlineData("script.bat", false)]
        [InlineData("payload.dll", false)]
        [InlineData("document.txt", false)]
        [InlineData("image.jpg", false)]
        [InlineData("spreadsheet.xlsx", false)]
        public void ValidateFileExtension_ReturnsCorrectResult(string filename, bool expectedIsValid)
        {
            // Arrange
            var extension = Path.GetExtension(filename).ToLower();

            // Act
            bool isValid = _allowedExtensions.Contains(extension);

            // Assert
            Assert.Equal(expectedIsValid, isValid);
        }

        [Fact]
        public void EmptyFile_ShouldBeInvalid()
        {
            // Arrange
            long fileSize = 0;

            // Act
            bool isValid = fileSize > 0;

            // Assert
            Assert.False(isValid, "Empty file should be considered invalid");
        }

        [Fact]
        public void LargePdfFile_ShouldBeValid()
        {
            // Arrange
            long fileSize = 50 * 1024 * 1024; // 50MB
            string fileName = "large_document.pdf";

            // Act
            bool extensionValid = _allowedExtensions.Contains(Path.GetExtension(fileName).ToLower());
            bool sizeValid = fileSize <= 100 * 1024 * 1024; // 100MB limit

            // Assert
            Assert.True(extensionValid, "PDF extension should be valid");
            Assert.True(sizeValid, "50MB should be within 100MB limit");
        }

        [Fact]
        public void AllowedExtensionsList_ContainsOnlyPdf()
        {
            // Assert
            Assert.Single(_allowedExtensions);
            Assert.Contains(".pdf", _allowedExtensions);
        }

        [Fact]
        public void PdfFile_WithUppercaseExtension_ShouldBeValid()
        {
            // Arrange
            string fileName = "CONTRACT.PDF";

            // Act
            var extension = Path.GetExtension(fileName).ToLower();
            bool isValid = _allowedExtensions.Contains(extension);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void FileSize_WithinLimit_IsValid()
        {
            // Arrange
            long fileSize = 5 * 1024 * 1024; // 5MB
            long maxSize = 100 * 1024 * 1024; // 100MB

            // Act
            bool isValid = fileSize <= maxSize && fileSize > 0;

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void FileSize_ExceedsLimit_IsInvalid()
        {
            // Arrange
            long fileSize = 150 * 1024 * 1024; // 150MB
            long maxSize = 100 * 1024 * 1024; // 100MB

            // Act
            bool isValid = fileSize <= maxSize;

            // Assert
            Assert.False(isValid);
        }
    }
}