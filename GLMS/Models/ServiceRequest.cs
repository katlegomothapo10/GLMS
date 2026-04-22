using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Models
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        InProgress,
        Completed,
        Cancelled
    }

    public class ServiceRequest
    {
        [Key]
        public int ServiceRequestId { get; set; }

        [Required]
        [Display(Name = "Request Number")]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        public int ContractId { get; set; }

        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Service Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost (USD)")]
        public decimal CostUSD { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost (ZAR)")]
        public decimal CostZAR { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [Display(Name = "Exchange Rate Used")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRateUsed { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Special Instructions")]
        public string? SpecialInstructions { get; set; }

        [Display(Name = "Requested Date")]
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ServiceRequestLog>? Logs { get; set; }
    }

    public class ServiceRequestLog
    {
        [Key]
        public int LogId { get; set; }

        public int ServiceRequestId { get; set; }

        [ForeignKey("ServiceRequestId")]
        public virtual ServiceRequest? ServiceRequest { get; set; }

        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? PerformedBy { get; set; }
    }
}