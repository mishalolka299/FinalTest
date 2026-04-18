using FinalTest.Api.Data;
using FinalTest.Api.Domain;
using FinalTest.Api.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace FinalTest.Tests.Unit;

public class SaleServiceTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private async Task<(Car car, Customer customer)> SetupTestData(AppDbContext db)
    {
        var car = new Car
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Color = "Blue",
            Mileage = 1000,
            Price = 25000,
            VIN = "12345678901234567",
            FuelType = FuelType.Petrol,
            Status = CarStatus.Available
        };

        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "123-456-7890",
            DriversLicense = "DL12345"
        };

        db.Cars.Add(car);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        return (car, customer);
    }

    [Fact]
    public async Task CreateAsync_ValidSale_CreatesSuccessfully()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, customer) = await SetupTestData(db);

        var result = await service.CreateAsync(car.Id, customer.Id, 24000, PaymentMethod.Cash);

        result.Id.ShouldBeGreaterThan(0);
        result.SalePrice.ShouldBe(24000);
        result.CarId.ShouldBe(car.Id);
        result.CustomerId.ShouldBe(customer.Id);

        var updatedCar = await db.Cars.FirstOrDefaultAsync(c => c.Id == car.Id);
        updatedCar!.Status.ShouldBe(CarStatus.Sold);
    }

    [Fact]
    public async Task CreateAsync_PriceExceeds105Percent_ThrowsArgumentException()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, customer) = await SetupTestData(db);

        var maxPrice = car.Price * 1.05m;
        var exceedingPrice = maxPrice + 1000;

        await Should.ThrowAsync<ArgumentException>(() => service.CreateAsync(car.Id, customer.Id, exceedingPrice, PaymentMethod.Cash));
    }

    [Fact]
    public async Task CreateAsync_PriceWithin105Percent_CreatesSuccessfully()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, customer) = await SetupTestData(db);

        var maxPrice = car.Price * 1.05m;
        var result = await service.CreateAsync(car.Id, customer.Id, maxPrice, PaymentMethod.Finance);

        result.SalePrice.ShouldBe(maxPrice);
    }

    [Fact]
    public async Task CreateAsync_NonexistentCar_ThrowsKeyNotFoundException()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (_, customer) = await SetupTestData(db);

        await Should.ThrowAsync<KeyNotFoundException>(() => service.CreateAsync(999, customer.Id, 20000, PaymentMethod.Cash));
    }

    [Fact]
    public async Task CreateAsync_NonexistentCustomer_ThrowsKeyNotFoundException()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, _) = await SetupTestData(db);

        await Should.ThrowAsync<KeyNotFoundException>(() => service.CreateAsync(car.Id, 999, 20000, PaymentMethod.Cash));
    }

    [Fact]
    public async Task CreateAsync_SoldCar_ThrowsInvalidOperationException()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, customer) = await SetupTestData(db);

        car.Status = CarStatus.Sold;
        db.Cars.Update(car);
        await db.SaveChangesAsync();

        await Should.ThrowAsync<InvalidOperationException>(() => service.CreateAsync(car.Id, customer.Id, 20000, PaymentMethod.Cash));
    }

    [Fact]
    public async Task CreateAsync_ReservedCar_CreatesSuccessfully()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, customer) = await SetupTestData(db);

        car.Status = CarStatus.Reserved;
        db.Cars.Update(car);
        await db.SaveChangesAsync();

        var result = await service.CreateAsync(car.Id, customer.Id, 24000, PaymentMethod.Lease);

        result.Id.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Finance)]
    [InlineData(PaymentMethod.Lease)]
    public async Task CreateAsync_VariousPaymentMethods_CreatesSuccessfully(PaymentMethod method)
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, customer) = await SetupTestData(db);

        var result = await service.CreateAsync(car.Id, customer.Id, 24000, method);

        result.PaymentMethod.ShouldBe(method);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithValidRange_ReturnsSales()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, customer) = await SetupTestData(db);

        await service.CreateAsync(car.Id, customer.Id, 24000, PaymentMethod.Cash);

        var today = DateTime.UtcNow;
        var startDate = today.AddDays(-1);
        var endDate = today.AddDays(1);

        var result = await service.GetByDateRangeAsync(startDate, endDate);

        result.Count().ShouldBe(1);
    }

    [Fact]
    public async Task GetByDateRangeAsync_NoSalesInRange_ReturnsEmpty()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, customer) = await SetupTestData(db);

        await service.CreateAsync(car.Id, customer.Id, 24000, PaymentMethod.Cash);

        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(20);

        var result = await service.GetByDateRangeAsync(startDate, endDate);

        result.Count().ShouldBe(0);
    }

    [Fact]
    public async Task GetByCustomerAsync_ExistingSales_ReturnsSales()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        var (car, customer) = await SetupTestData(db);

        await service.CreateAsync(car.Id, customer.Id, 24000, PaymentMethod.Cash);

        var result = await service.GetByCustomerAsync(customer.Id);

        result.Count().ShouldBe(1);
        result.First().CustomerId.ShouldBe(customer.Id);
    }

    [Fact]
    public async Task GetByCustomerAsync_NoSales_ReturnsEmpty()
    {
        var db = CreateInMemoryDb();
        var service = new SaleService(db);
        await SetupTestData(db);

        var result = await service.GetByCustomerAsync(999);

        result.Count().ShouldBe(0);
    }
}
