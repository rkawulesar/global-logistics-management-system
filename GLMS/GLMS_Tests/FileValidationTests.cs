using GLMS_API.Factories;
using GLMS_API.Models;
using GLMS_API.Repositories;
using GLMS_API.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace GLMS_Web.Tests
{
    //Code Attribution for mocking dependencies using Moq
    //Link: https://github.com/moq/moq4/wiki/Quickstart
    [TestFixture]
    public class FileValidationTests
    {
        private ContractService _service = null!;

        [SetUp]
        public void Setup()
        {
            // Creates the service with a mocked repository 
            var mockRepo = new Mock<IContractRepository>();
            _service = new ContractService(mockRepo.Object, new ContractFactory());
        }

        // Creates a fake uploaded file for testing file validation
        private IFormFile MockFile(string fileName, long sizeBytes = 1024)
        {
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.Length).Returns(sizeBytes);
            return mock.Object;
        }

        // Creates a valid contract object to reuse in the tests
        private Contract ValidContract() => new Contract
        {
            ClientId = 1,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddYears(1),
            Status = "Active",
            ServiceLevel = "Standard"
        };

        // Tests that a valid PDF file passes validation
        [Test]
        public async Task ValidPdf_Success()
        {
            var result = await _service.ValidateContractAsync(ValidContract(), MockFile("agreement.pdf"), true);
            Assert.That(result.Success, Is.True);
        }

        // Tests that an .exe file is rejected
        [Test]
        public async Task ExeFile_Error()
        {
            var result = await _service.ValidateContractAsync(ValidContract(), MockFile("virus.exe"), true);
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Only PDF files are allowed."));
        }

        // Tests that a .docx file is rejected
        [Test]
        public async Task DocxFile_Error()
        {
            var result = await _service.ValidateContractAsync(ValidContract(), MockFile("contract.docx"), true);
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Only PDF files are allowed."));
        }

        // Tests that no file on create returns an error
        [Test]
        public async Task NullFileOnCreate_Error()
        {
            var result = await _service.ValidateContractAsync(ValidContract(), null, true);
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Please upload a signed agreement PDF."));
        }

        // Tests that a file larger than 10MB is rejected
        [Test]
        public async Task FileTooLarge_Error()
        {
            long fileSize = 11 * 1024 * 1024;
            var result = await _service.ValidateContractAsync(ValidContract(), MockFile("big.pdf", fileSize), true);
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("File size cannot exceed 10 MB."));
        }

        // Tests that an end date before the start date is rejected
        [Test]
        public async Task InvalidDates_Error()
        {
            var contract = ValidContract();
            contract.EndDate = contract.StartDate.AddDays(-1);

            var result = await _service.ValidateContractAsync(contract, MockFile("agreement.pdf"), true);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("End date cannot be earlier than start date."));
        }

        // Tests that no new file is allowed during edit
        [Test]
        public async Task NullFileOnEdit_Success()
        {
            var result = await _service.ValidateContractAsync(ValidContract(), null, false);
            Assert.That(result.Success, Is.True);
        }

        // Tests that a file exactly 10MB is accepted
        [Test]
        public async Task ExactlyTenMb_Success()
        {
            long fileSize = 10 * 1024 * 1024;
            var result = await _service.ValidateContractAsync(ValidContract(), MockFile("agreement.pdf", fileSize), true);
            Assert.That(result.Success, Is.True);
        }
    }
}