using System.ComponentModel.DataAnnotations;

namespace ECommercePayment.Domain.DTOs.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Client ID is required")]
        public string Client_ID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Client Secret is required")]
        public string Client_Secret { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grant Type is required")]
        public string Grant_Type { get; set; } = "client_credentials";

        public string Scope { get; set; } = "payment";
    }
}
