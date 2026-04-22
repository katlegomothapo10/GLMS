using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GLMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "ClientId", "Address", "ContactPerson", "CreatedAt", "Email", "Name", "Phone", "Region" },
                values: new object[,]
                {
                    { 1, "123 Main St, Johannesburg", "John Doe", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "contact@globalimporters.com", "Global Importers Ltd", "+27 11 234 5678", "Africa" },
                    { 2, "45 Berliner Str, Berlin", "Anna Schmidt", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@eurotech.eu", "EuroTech Solutions", "+49 30 1234 5678", "Europe" },
                    { 3, "88 Marina Blvd, Singapore", "Tan Wei Ming", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sales@aptrading.sg", "Asia Pacific Trading", "+65 6789 1234", "Asia" }
                });

            migrationBuilder.InsertData(
                table: "Contracts",
                columns: new[] { "ContractId", "ClientId", "ContractNumber", "ContractValueUSD", "CreatedAt", "EndDate", "ServiceLevel", "SignedAgreementPath", "SpecialTerms", "StartDate", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, "CT-2024-001", 50000m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, "Priority handling for all shipments", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null },
                    { 2, 2, "CT-2024-002", 25000m, new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, "Standard terms apply", new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, null },
                    { 3, 3, "CT-2024-003", 100000m, new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, null, "Enterprise SLA with 24/7 support", new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null }
                });

            migrationBuilder.InsertData(
                table: "ServiceRequests",
                columns: new[] { "ServiceRequestId", "CompletedDate", "ContractId", "CostUSD", "CostZAR", "Description", "ExchangeRateUsed", "RequestNumber", "RequestedDate", "SpecialInstructions", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, 1, 5000m, 96250m, "Container shipping JNB to DUR", 19.25m, "SR-202401-001", new DateTime(2024, 1, 15, 10, 30, 0, 0, DateTimeKind.Utc), null, 3, null },
                    { 2, null, 1, 12500m, 240625m, "Urgent air freight to CPT", 19.25m, "SR-202401-002", new DateTime(2024, 1, 20, 14, 0, 0, 0, DateTimeKind.Utc), null, 2, null },
                    { 3, null, 3, 25000m, 482500m, "Bulk cargo to Singapore", 19.30m, "SR-202402-001", new DateTime(2024, 2, 5, 9, 15, 0, 0, DateTimeKind.Utc), null, 1, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Contracts",
                keyColumn: "ContractId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ServiceRequests",
                keyColumn: "ServiceRequestId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ServiceRequests",
                keyColumn: "ServiceRequestId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ServiceRequests",
                keyColumn: "ServiceRequestId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Contracts",
                keyColumn: "ContractId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Contracts",
                keyColumn: "ContractId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 3);
        }
    }
}
