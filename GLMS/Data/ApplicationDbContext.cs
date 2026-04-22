using Microsoft.EntityFrameworkCore;
using GLMS.Models;

namespace GLMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<ServiceRequestLog> ServiceRequestLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.ContractNumber)
                .IsUnique();

            modelBuilder.Entity<ServiceRequest>()
                .HasIndex(r => r.RequestNumber)
                .IsUnique();

            // ==========================================
            // SEED DATA - Clients
            // ==========================================
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    ClientId = 1,
                    Name = "Global Importers Ltd",
                    Email = "contact@globalimporters.com",
                    Phone = "+27 11 234 5678",
                    Address = "123 Main St, Johannesburg",
                    Region = "Africa",
                    ContactPerson = "John Doe",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Client
                {
                    ClientId = 2,
                    Name = "EuroTech Solutions",
                    Email = "info@eurotech.eu",
                    Phone = "+49 30 1234 5678",
                    Address = "45 Berliner Str, Berlin",
                    Region = "Europe",
                    ContactPerson = "Anna Schmidt",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Client
                {
                    ClientId = 3,
                    Name = "Asia Pacific Trading",
                    Email = "sales@aptrading.sg",
                    Phone = "+65 6789 1234",
                    Address = "88 Marina Blvd, Singapore",
                    Region = "Asia",
                    ContactPerson = "Tan Wei Ming",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ==========================================
            // SEED DATA - Contracts
            // ==========================================
            modelBuilder.Entity<Contract>().HasData(
                new Contract
                {
                    ContractId = 1,
                    ContractNumber = "CT-2024-001",
                    ClientId = 1,
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 12, 31),
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLevel.Premium,
                    ContractValueUSD = 50000,
                    SpecialTerms = "Priority handling for all shipments",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Contract
                {
                    ContractId = 2,
                    ContractNumber = "CT-2024-002",
                    ClientId = 2,
                    StartDate = new DateTime(2023, 6, 1),
                    EndDate = new DateTime(2024, 5, 31),
                    Status = ContractStatus.Expired,
                    ServiceLevel = ServiceLevel.Standard,
                    ContractValueUSD = 25000,
                    SpecialTerms = "Standard terms apply",
                    CreatedAt = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Contract
                {
                    ContractId = 3,
                    ContractNumber = "CT-2024-003",
                    ClientId = 3,
                    StartDate = new DateTime(2024, 3, 1),
                    EndDate = new DateTime(2025, 2, 28),
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLevel.Enterprise,
                    ContractValueUSD = 100000,
                    SpecialTerms = "Enterprise SLA with 24/7 support",
                    CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ==========================================
            // SEED DATA - Service Requests
            // ==========================================
            modelBuilder.Entity<ServiceRequest>().HasData(
                new ServiceRequest
                {
                    ServiceRequestId = 1,
                    RequestNumber = "SR-202401-001",
                    ContractId = 1,
                    Description = "Container shipping JNB to DUR",
                    CostUSD = 5000,
                    CostZAR = 96250,
                    Status = RequestStatus.Completed,
                    ExchangeRateUsed = 19.25m,
                    RequestedDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
                },
                new ServiceRequest
                {
                    ServiceRequestId = 2,
                    RequestNumber = "SR-202401-002",
                    ContractId = 1,
                    Description = "Urgent air freight to CPT",
                    CostUSD = 12500,
                    CostZAR = 240625,
                    Status = RequestStatus.InProgress,
                    ExchangeRateUsed = 19.25m,
                    RequestedDate = new DateTime(2024, 1, 20, 14, 0, 0, DateTimeKind.Utc)
                },
                new ServiceRequest
                {
                    ServiceRequestId = 3,
                    RequestNumber = "SR-202402-001",
                    ContractId = 3,
                    Description = "Bulk cargo to Singapore",
                    CostUSD = 25000,
                    CostZAR = 482500,
                    Status = RequestStatus.Approved,
                    ExchangeRateUsed = 19.30m,
                    RequestedDate = new DateTime(2024, 2, 5, 9, 15, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}