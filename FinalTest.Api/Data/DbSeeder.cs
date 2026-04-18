using AutoFixture;
using FinalTest.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinalTest.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, int targetCount = 10000, CancellationToken cancellationToken = default)
    {
        var existingCarCount = await db.Cars.CountAsync(cancellationToken);
        if (existingCarCount >= targetCount)
            return;

        var fixture = new Fixture();
        var carCountToAdd = targetCount - existingCarCount;
        var customerCount = Math.Max(1000, carCountToAdd / 10);
        var saleCount = Math.Max(5000, carCountToAdd / 2);

        // Seed customers
        var existingCustomers = await db.Customers.CountAsync(cancellationToken);
        if (existingCustomers == 0)
        {
            var customers = Enumerable.Range(1, customerCount)
                .Select(i =>
                {
                    var suffix = Guid.NewGuid().ToString("N")[..8];
                    return fixture.Build<Customer>()
                        .Without(c => c.Id)
                        .Without(c => c.Sales)
                        .With(c => c.FirstName, $"Customer-{i}")
                        .With(c => c.Email, $"cust-{i}-{suffix}@example.com")
                        .With(c => c.Phone, $"+1-555-{i:00000}")
                        .With(c => c.DriversLicense, $"DL-{i:00000000}")
                        .Create();
                })
                .ToList();

            for (int i = 0; i < customers.Count; i += 1000)
            {
                db.Customers.AddRange(customers.Skip(i).Take(1000));
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        var allCustomers = await db.Customers.ToListAsync(cancellationToken);

        // Seed cars
        var makes = new[] { "Toyota", "Honda", "Ford", "BMW", "Mercedes", "Audi", "Volkswagen", "Mazda", "Nissan", "Hyundai" };
        var models = new[] { "Model-A", "Model-B", "Model-C", "Model-D", "Model-E" };
        var colors = new[] { "Red", "Blue", "Black", "White", "Silver", "Gray" };
        var rand = new Random();

        var cars = Enumerable.Range(existingCarCount + 1, carCountToAdd)
            .Select(i =>
            {
                var suffix = Guid.NewGuid().ToString("N")[..4].ToUpper();
                var year = rand.Next(2000, 2026);
                var basePrice = rand.Next(15000, 80000);

                return fixture.Build<Car>()
                    .Without(c => c.Id)
                    .Without(c => c.Sales)
                    .With(c => c.Make, makes[rand.Next(makes.Length)])
                    .With(c => c.Model, models[rand.Next(models.Length)])
                    .With(c => c.Year, year)
                    .With(c => c.Color, colors[rand.Next(colors.Length)])
                    .With(c => c.VIN, $"VIN-{i:0000000000}-{suffix}")
                    .With(c => c.Price, basePrice)
                    .With(c => c.Mileage, rand.Next(0, 200000))
                    .With(c => c.Status, (CarStatus)rand.Next(0, 3))
                    .With(c => c.FuelType, (FuelType)rand.Next(0, 4))
                    .Create();
            })
            .ToList();

        for (int i = 0; i < cars.Count; i += 1000)
        {
            db.Cars.AddRange(cars.Skip(i).Take(1000));
            await db.SaveChangesAsync(cancellationToken);
        }

        var allCars = await db.Cars.ToListAsync(cancellationToken);

        // Seed sales
        var existingSales = await db.Sales.CountAsync(cancellationToken);
        if (existingSales == 0)
        {
            var sales = Enumerable.Range(1, Math.Min(saleCount, allCars.Count))
                .Select(i =>
                {
                    var car = allCars[rand.Next(allCars.Count)];
                    var customer = allCustomers[rand.Next(allCustomers.Count)];
                    var salePrice = car.Price * ((decimal)rand.Next(95, 106) / 100m);

                    return fixture.Build<Sale>()
                        .Without(s => s.Id)
                        .With(s => s.CarId, car.Id)
                        .With(s => s.CustomerId, customer.Id)
                        .With(s => s.SaleDate, DateTime.UtcNow.AddDays(-rand.Next(0, 365)))
                        .With(s => s.SalePrice, Math.Round(salePrice, 2))
                        .With(s => s.PaymentMethod, (PaymentMethod)rand.Next(0, 3))
                        .Create();
                })
                .ToList();

            for (int i = 0; i < sales.Count; i += 1000)
            {
                db.Sales.AddRange(sales.Skip(i).Take(1000));
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
