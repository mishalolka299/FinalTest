namespace FinalTest.Api.Domain;

public class Car
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public decimal Mileage { get; set; }
    public decimal Price { get; set; }
    public string VIN { get; set; } = string.Empty;
    public CarStatus Status { get; set; } = CarStatus.Available;
    public FuelType FuelType { get; set; }

    public ICollection<Sale> Sales { get; set; } = [];
}
