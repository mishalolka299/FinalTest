using FinalTest.Api.Data;
using FinalTest.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalTest.Api.Services;

public interface ISaleService
{
    Task<Sale> CreateAsync(int carId, int customerId, decimal salePrice, PaymentMethod paymentMethod, CancellationToken cancellationToken = default);
    Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
}

public class SaleService : ISaleService
{
    private readonly AppDbContext _db;

    public SaleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Sale> CreateAsync(int carId, int customerId, decimal salePrice, 
        PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == carId, cancellationToken);
        if (car == null)
            throw new KeyNotFoundException("Car not found");

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        if (customer == null)
            throw new KeyNotFoundException("Customer not found");

        if (car.Status != CarStatus.Available && car.Status != CarStatus.Reserved)
            throw new InvalidOperationException("Only available or reserved cars can be sold");

        var maxPrice = car.Price * 1.05m;
        if (salePrice > maxPrice)
            throw new ArgumentException($"Sale price cannot exceed {maxPrice}");

        car.Status = CarStatus.Sold;

        var sale = new Sale
        {
            CarId = carId,
            CustomerId = customerId,
            SaleDate = DateTime.UtcNow,
            SalePrice = salePrice,
            PaymentMethod = paymentMethod
        };

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _db.Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId, 
        CancellationToken cancellationToken = default)
    {
        return await _db.Sales
            .Where(s => s.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }
}
