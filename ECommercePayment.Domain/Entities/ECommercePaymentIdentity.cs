using System.ComponentModel.DataAnnotations;

namespace ECommercePayment.Domain.Entities
{
    public class ECommercePaymentIdentity
    {
        [Key]
        public int Id { get; set; }
        public string Grant_Type { get; set; }
        public string Scope { get; set; }
        public string Client_ID { get; set; }
        public string Client_Secret { get; set; }
        [Required]
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
