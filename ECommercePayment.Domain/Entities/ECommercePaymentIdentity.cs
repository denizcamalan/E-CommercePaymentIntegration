using System.ComponentModel.DataAnnotations;

namespace ECommercePayment.Domain.Entities
{
    public class ECommercePaymentIdentity : BaseEntity<Guid>
    {
        public string Grant_Type { get; set; }
        public string Scope { get; set; }
        public string Client_ID { get; set; }
        public string Client_Secret { get; set; }

    }
}
