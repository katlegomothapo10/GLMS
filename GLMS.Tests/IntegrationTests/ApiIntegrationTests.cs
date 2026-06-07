using Xunit;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GLMS.Tests.IntegrationTests
{
    public class ApiIntegrationTests
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUrl = "http://localhost:5143";

        public ApiIntegrationTests()
        {
            _client = new HttpClient();
            _client.BaseAddress = new System.Uri(_apiBaseUrl);
            _client.Timeout = TimeSpan.FromSeconds(30);
        }

        // ========== CLIENTS TESTS ==========

        [Fact]
        public async Task Get_Clients_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/clients");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_Clients_ReturnsJsonData()
        {
            // Act
            var response = await _client.GetAsync("/api/clients");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(string.IsNullOrEmpty(content));
            Assert.Contains("[", content);
        }

        [Fact]
        public async Task Get_ClientById_ReturnsNotFoundForInvalidId()
        {
            // Act
            var response = await _client.GetAsync("/api/clients/99999");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_Client_CreatesNewClient()
        {
            // Arrange
            var newClient = new
            {
                name = "Test Integration Client",
                email = "test@integration.com",
                phone = "1234567890",
                address = "123 Test St",
                region = "Test Region",
                contactPerson = "Tester",
                createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            var json = JsonSerializer.Serialize(newClient);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/clients", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        // ========== CONTRACTS TESTS ==========

        [Fact]
        public async Task Get_Contracts_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_Contracts_ReturnsJsonData()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(string.IsNullOrEmpty(content));
        }

        [Fact]
        public async Task Get_ContractById_ReturnsNotFoundForInvalidId()
        {
            // Act
            var response = await _client.GetAsync("/api/contracts/99999");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_Contract_CreatesNewContract()
        {
            // Arrange - First ensure we have a client
            var newContract = new
            {
                clientId = 1,
                contractNumber = "",
                startDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                endDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                status = 1, // Active
                serviceLevel = 1, // Standard
                contractValueUSD = 10000,
                specialTerms = "Test Contract",
                createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            var json = JsonSerializer.Serialize(newContract);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/contracts", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Patch_ContractStatus_UpdatesStatus()
        {
            // First get a contract to update, or create one
            var getResponse = await _client.GetAsync("/api/contracts");
            var content = await getResponse.Content.ReadAsStringAsync();

            if (content == "[]")
            {
                // Create a contract first
                var newContract = new
                {
                    clientId = 1,
                    startDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    endDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    status = 1,
                    serviceLevel = 1,
                    contractValueUSD = 10000
                };
                var createJson = JsonSerializer.Serialize(newContract);
                var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
                var createResponse = await _client.PostAsync("/api/contracts", createContent);

                // Get the created contract ID from response
                var responseContent = await createResponse.Content.ReadAsStringAsync();
                var createdContract = JsonSerializer.Deserialize<dynamic>(responseContent);
                int contractId = createdContract?.GetProperty("contractId").GetInt32() ?? 1;

                // Now update its status
                var statusUpdate = new { status = 2 }; // Change to something else
                var patchJson = JsonSerializer.Serialize(statusUpdate);
                var patchContent = new StringContent(patchJson, Encoding.UTF8, "application/json");
                var patchResponse = await _client.PatchAsync($"/api/contracts/{contractId}/status", patchContent);

                Assert.Equal(System.Net.HttpStatusCode.OK, patchResponse.StatusCode);
            }
            else
            {
                // Update existing contract
                var contracts = JsonSerializer.Deserialize<JsonElement[]>(content);
                if (contracts.Length > 0)
                {
                    int contractId = contracts[0].GetProperty("contractId").GetInt32();
                    var statusUpdate = new { status = 1 };
                    var patchJson = JsonSerializer.Serialize(statusUpdate);
                    var patchContent = new StringContent(patchJson, Encoding.UTF8, "application/json");
                    var patchResponse = await _client.PatchAsync($"/api/contracts/{contractId}/status", patchContent);

                    Assert.Equal(System.Net.HttpStatusCode.OK, patchResponse.StatusCode);
                }
            }
        }

        // ========== SERVICE REQUESTS TESTS ==========

        [Fact]
        public async Task Get_ServiceRequests_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_ServiceRequests_ReturnsJsonData()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(string.IsNullOrEmpty(content));
        }

        [Fact]
        public async Task Get_ServiceRequestById_ReturnsNotFoundForInvalidId()
        {
            // Act
            var response = await _client.GetAsync("/api/servicerequests/99999");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_ServiceRequest_CreatesNewServiceRequest()
        {
            // Arrange
            var newRequest = new
            {
                contractId = 1,
                description = "Test Service Request",
                costUSD = 500,
                costZAR = 0,
                specialInstructions = "Integration Test",
                exchangeRateUsed = 0
            };

            var json = JsonSerializer.Serialize(newRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/servicerequests", content);

            // Assert - May be 200 OK or 201 Created
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Patch_ServiceRequestStatus_UpdatesStatus()
        {
            // First create a service request
            var newRequest = new
            {
                contractId = 1,
                description = "Test Status Update",
                costUSD = 100,
                costZAR = 0,
                specialInstructions = "Test",
                exchangeRateUsed = 0
            };

            var createJson = JsonSerializer.Serialize(newRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/api/servicerequests", createContent);

            if (createResponse.IsSuccessStatusCode)
            {
                var responseContent = await createResponse.Content.ReadAsStringAsync();
                var createdRequest = JsonSerializer.Deserialize<dynamic>(responseContent);
                int requestId = createdRequest?.GetProperty("serviceRequestId").GetInt32() ?? 1;

                // Update status to InProgress (2)
                var statusUpdate = new { status = 2 };
                var patchJson = JsonSerializer.Serialize(statusUpdate);
                var patchContent = new StringContent(patchJson, Encoding.UTF8, "application/json");
                var patchResponse = await _client.PatchAsync($"/api/servicerequests/{requestId}/status", patchContent);

                Assert.Equal(System.Net.HttpStatusCode.OK, patchResponse.StatusCode);
            }
        }

        // ========== ERROR HANDLING TESTS ==========

        [Fact]
        public async Task Post_InvalidClient_ReturnsBadRequest()
        {
            // Arrange - Try to create a contract with invalid client
            var invalidContract = new
            {
                clientId = 99999,
                startDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                endDate = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                status = 1,
                serviceLevel = 1,
                contractValueUSD = 10000
            };

            var json = JsonSerializer.Serialize(invalidContract);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/contracts", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_InvalidServiceRequest_ReturnsBadRequest()
        {
            // Arrange - Try to create request with invalid contract
            var invalidRequest = new
            {
                contractId = 99999,
                description = "Invalid Request",
                costUSD = 100,
                specialInstructions = "Test"
            };

            var json = JsonSerializer.Serialize(invalidRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/servicerequests", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}