using GLMS_API.Models;
using GLMS_API.Repositories;
using GLMS_API.Services;
using Moq;
using NUnit.Framework;

namespace GLMS_Web.Tests
{
    [TestFixture]
    public class ServiceRequestWorkflowTests
    {
        private Mock<IServiceRequestRepository> _repo = null!;
        private ServiceRequestService _service = null!;
        private ServiceRequestSubject _subject = null!;

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IServiceRequestRepository>();
            _subject = new ServiceRequestSubject(new List<string>());
            _service = new ServiceRequestService(_repo.Object, _subject);
        }

        // Creates a basic valid request
        private ServiceRequest ValidRequest() => new ServiceRequest
        {
            Id = 1,
            ContractId = 1,
            Description = "Test",
            Cost = 100,
            Status = "Pending"
        };

        // Tests an invalid contract
        [Test]
        public async Task InvalidContract_Error()
        {
            _repo.Setup(r => r.GetContractByIdAsync(1))
                 .ReturnsAsync((Contract?)null);

            var result = await _service.AddServiceRequestAsync(ValidRequest());

            Assert.That(result.Success, Is.False);
        }

        // Tests an expired contract
        [Test]
        public async Task ExpiredContract_Error()
        {
            _repo.Setup(r => r.GetContractByIdAsync(1))
                 .ReturnsAsync(new Contract { Id = 1, Status = "Expired" });

            var result = await _service.AddServiceRequestAsync(ValidRequest());

            Assert.That(result.Success, Is.False);
        }

        // Tests an on hold contract
        [Test]
        public async Task OnHoldContract_Error()
        {
            _repo.Setup(r => r.GetContractByIdAsync(1))
                 .ReturnsAsync(new Contract { Id = 1, Status = "On Hold" });

            var result = await _service.AddServiceRequestAsync(ValidRequest());

            Assert.That(result.Success, Is.False);
        }

        // Tests a duplicate request
        [Test]
        public async Task DuplicateRequest_Error()
        {
            _repo.Setup(r => r.GetContractByIdAsync(1))
                 .ReturnsAsync(new Contract { Id = 1, Status = "Active" });

            _repo.Setup(r => r.ContractAlreadyHasRequestAsync(1))
                 .ReturnsAsync(true);

            var result = await _service.AddServiceRequestAsync(ValidRequest());

            Assert.That(result.Success, Is.False);
        }

        // Tests a valid request
        [Test]
        public async Task ValidRequest_Success()
        {
            _repo.Setup(r => r.GetContractByIdAsync(1))
                 .ReturnsAsync(new Contract { Id = 1, Status = "Active" });

            _repo.Setup(r => r.ContractAlreadyHasRequestAsync(1))
                 .ReturnsAsync(false);

            var result = await _service.AddServiceRequestAsync(ValidRequest());

            Assert.That(result.Success, Is.True);
        }
    }
}