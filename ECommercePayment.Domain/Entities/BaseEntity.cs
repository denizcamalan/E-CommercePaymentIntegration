using System.ComponentModel.DataAnnotations;
using ECommercePayment.Domain.Enums;

namespace ECommercePayment.Domain.Entities;

public class BaseEntity<T>
{
    [Key]
    public T Id { get; set; }

    public DateTime Created { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? Modified { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? Deleted { get; set; }

    public string? DeletedBy { get; set; }

    public DbStatus Status { get; set; } = DbStatus.ACTIVE;

}
