using FinalTest.Api.Data;
using FinalTest.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalTest.Api.Services;

public interface ICarService
{
    Task<Car> CreateAsync(Car car, CancellationToken cancellationToken = default);
    Task<Car?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Car>> GetAllAsync(string? make = null, int? yearMin = null, int? yearMax = null, 
        decimal? priceMin = null, decimal? priceMax = null, FuelType? fuelType = null, 
        CarStatus? status = null, CancellationToken cancellationToken = default);
    Task<Car> UpdateAsync(Car car, CancellationToken cancellationToken = default);
    Task<Car> ReserveAsync(int id, CancellationToken cancellationToken = default);
}

public class CarService : ICarService
{
    private readonly AppDbContext _db;

    public CarService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Car> CreateAsync(Car car, CancellationToken cancellationToken = default)
    {
        ValidateVin(car.VIN);
        ValidateYear(car.Year);
        ValidateMileage(car.Mileage);

        var existing = await _db.Cars.FirstOrDefaultAsync(c => c.VIN == car.VIN, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException("VIN must be unique");

        _db.Cars.Add(car);
        await _db.SaveChangesAsync(cancellationToken);
        return car;
    }

    public async Task<Car?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Cars.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Car>> GetAllAsync(string? make = null, int? yearMin = null, int? yearMax = null,
        decimal? priceMin = null, decimal? priceMax = null, FuelType? fuelType = null,
        CarStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Cars.AsQueryable();

        if (!string.IsNullOrEmpty(make))
            query = query.Where(c => c.Make == make);
        if (yearMin.HasValue)
            query = query.Where(c => c.Year >= yearMin);
        if (yearMax.HasValue)
            query = query.Where(c => c.Year <= yearMax);
        if (priceMin.HasValue)
            query = query.Where(c => c.Price >= priceMin);
        if (priceMax.HasValue)
            query = query.Where(c => c.Price <= priceMax);
        if (fuelType.HasValue)
            query = query.Where(c => c.FuelType == fuelType);
        if (status.HasValue)
            query = query.Where(c => c.Status == status);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Car> UpdateAsync(Car car, CancellationToken cancellationToken = default)
    {
        ValidateYear(car.Year);
        ValidateMileage(car.Mileage);

        var existing = await _db.Cars.FirstOrDefaultAsync(c => c.Id == car.Id, cancellationToken);
        if (existing == null)
            throw new KeyNotFoundException("Car not found");

        existing.Make = car.Make;
        existing.Model = car.Model;
        existing.Year = car.Year;
        existing.Color = car.Color;
        existing.Mileage = car.Mileage;
        existing.Price = car.Price;
        existing.FuelType = car.FuelType;

        await _db.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<Car> ReserveAsync(int id, CancellationToken cancellationToken = default)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (car == null)
            throw new KeyNotFoundException("Car not found");

        if (car.Status != CarStatus.Available)
            throw new InvalidOperationException("Only available cars can be reserved");

        car.Status = CarStatus.Reserved;
        await _db.SaveChangesAsync(cancellationToken);
        return car;
    }

    private static void ValidateVin(string vin)
    {
        if (string.IsNullOrEmpty(vin) || vin.Length != 17)
            throw new ArgumentException("VIN must be exactly 17 characters");
    }

    private static void ValidateYear(int year)
    {
        var currentYear = DateTime.UtcNow.Year;
        if (year < 1900 || year > currentYear + 1)
            throw new ArgumentException($"Year must be between 1900 and {currentYear + 1}");
    }

    private static void ValidateMileage(decimal mileage)
    {
        if (mileage < 0)
            throw new ArgumentException("Mileage cannot be negative");
    }
}
