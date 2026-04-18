namespace FinalTest.Api.Domain;

public enum PaymentMethod
{
    Cash,
    Finance,
    Lease
}

public class Sale
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public int CustomerId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal SalePrice { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public Car Car { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
