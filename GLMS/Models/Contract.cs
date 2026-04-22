using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Models
{
    // ENUMS MUST BE DEFINED FIRST
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }

    public enum ServiceLevel
    {
        Standard,
        Premium,
        Enterprise
    }

    // THEN THE CLASS
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

        [Required]
        [Display(Name = "Contract Number")]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        [Display(Name = "Service Level")]
        public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Contract Value (USD)")]
        public decimal ContractValueUSD { get; set; }

        [StringLength(500)]
        [Display(Name = "Special Terms")]
        public string? SpecialTerms { get; set; }

        [Display(Name = "Signed Agreement")]
        public string? SignedAgreementPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // For file upload
        [NotMapped]
        public IFormFile? SignedAgreement { get; set; }

        // Navigation Properties
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        [NotMapped]
        public bool IsActive => Status == ContractStatus.Active &&
                                 StartDate <= DateTime.Today &&
                                 EndDate >= DateTime.Today;

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            ContractStatus.Active => "success",
            ContractStatus.Draft => "secondary",
            ContractStatus.Expired => "danger",
            ContractStatus.OnHold => "warning",
            _ => "secondary"
        };

        [NotMapped]
        public string ContractNumberWithClient => $"{ContractNumber} - {Client?.Name ?? "No Client"}";
    }
}