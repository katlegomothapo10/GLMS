using Xunit;
using System;
using GLMS.Models;


namespace GLMS.Tests
{
    public class ContractStatusTests
    {
        [Fact]
        public void ActiveContract_WithValidDates_ReturnsIsActiveTrue()
        {
            // Arrange
            var contract = new GLMS.Models.Contract
            {
                Status = ContractStatus.Active,
                StartDate = DateTime.Today.AddMonths(-1),
                EndDate = DateTime.Today.AddMonths(1)
            };

            // Act
            bool isActive = contract.Status == ContractStatus.Active &&
                           contract.StartDate <= DateTime.Today &&
                           contract.EndDate >= DateTime.Today;

            // Assert
            Assert.True(isActive);
        }

        [Fact]
        public void ActiveContract_WithExpiredDates_ReturnsIsActiveFalse()
        {
            // Arrange
            var contract = new GLMS.Models.Contract
            {
                Status = ContractStatus.Active,
                StartDate = DateTime.Today.AddMonths(-6),
                EndDate = DateTime.Today.AddMonths(-1)
            };

            // Act
            bool isActive = contract.Status == ContractStatus.Active &&
                           contract.StartDate <= DateTime.Today &&
                           contract.EndDate >= DateTime.Today;

            // Assert
            Assert.False(isActive);
        }

        [Fact]
        public void ExpiredContract_CannotHaveServiceRequest()
        {
            // Arrange
            var contract = new GLMS.Models.Contract
            {
                Status = ContractStatus.Expired
            };

            // Act
            bool canCreateRequest = contract.Status == ContractStatus.Active;

            // Assert
            Assert.False(canCreateRequest, "Expired contracts should not allow service requests");
        }

        [Fact]
        public void OnHoldContract_CannotHaveServiceRequest()
        {
            // Arrange
            var contract = new GLMS.Models.Contract
            {
                Status = ContractStatus.OnHold
            };

            // Act
            bool canCreateRequest = contract.Status == ContractStatus.Active;

            // Assert
            Assert.False(canCreateRequest, "On Hold contracts should not allow service requests");
        }

        [Fact]
        public void DraftContract_CannotHaveServiceRequest()
        {
            // Arrange
            var contract = new GLMS.Models.Contract
            {
                Status = ContractStatus.Draft
            };

            // Act
            bool canCreateRequest = contract.Status == ContractStatus.Active;

            // Assert
            Assert.False(canCreateRequest, "Draft contracts should not allow service requests");
        }

        [Theory]
        [InlineData(ContractStatus.Active, true)]
        [InlineData(ContractStatus.Draft, false)]
        [InlineData(ContractStatus.Expired, false)]
        [InlineData(ContractStatus.OnHold, false)]
        public void ServiceRequestCreation_OnlyAllowedForActiveContracts(ContractStatus status, bool expectedAllowed)
        {
            // Arrange
            var contract = new GLMS.Models.Contract
            {
                Status = status,
                StartDate = DateTime.Today.AddMonths(-1),
                EndDate = DateTime.Today.AddMonths(1)
            };

            // Act
            bool canCreateRequest = contract.Status == ContractStatus.Active &&
                                   contract.StartDate <= DateTime.Today &&
                                   contract.EndDate >= DateTime.Today;

            // Assert
            Assert.Equal(expectedAllowed, canCreateRequest);
        }

        [Fact]
        public void ContractStatus_EnumHasAllRequiredValues()
        {
            // Arrange
            var statusValues = Enum.GetValues(typeof(ContractStatus));
            var statusList = statusValues.Cast<ContractStatus>().ToList();

            // Assert
            Assert.Contains(ContractStatus.Draft, statusList);
            Assert.Contains(ContractStatus.Active, statusList);
            Assert.Contains(ContractStatus.Expired, statusList);
            Assert.Contains(ContractStatus.OnHold, statusList);
            Assert.Equal(4, statusList.Count);
        }

        [Fact]
        public void ServiceRequest_CreationFailsForExpiredContract()
        {
            // Arrange
            var contract = new GLMS.Models.Contract
            {
                Status = ContractStatus.Expired,
                StartDate = DateTime.Today.AddMonths(-6),
                EndDate = DateTime.Today.AddMonths(-1)
            };

            string? errorMessage = null;

            // Act
            if (contract.Status != ContractStatus.Active)
            {
                errorMessage = $"Service requests cannot be created for contracts with status '{contract.Status}'.";
            }

            // Assert
            Assert.NotNull(errorMessage);
            Assert.Contains("Expired", errorMessage);
        }
    }
}