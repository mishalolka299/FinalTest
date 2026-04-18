using FinalTest.Api.Data;
using FinalTest.Api.Domain;
using FinalTest.Api.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace FinalTest.Tests.Unit;

public class CarServiceTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ValidCar_CreatesSuccessfully()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var service = new CarService(db);
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

        // Act
        var result = await service.CreateAsync(car);

        // Assert
        result.Id.ShouldBeGreaterThan(0);
        result.VIN.ShouldBe("12345678901234567");
    }

    [Fact]
    public async Task CreateAsync_VinTooShort_ThrowsArgumentException()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);
        var car = new Car
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Color = "Blue",
            Mileage = 1000,
            Price = 25000,
            VIN = "123",
            FuelType = FuelType.Petrol
        };

        await Should.ThrowAsync<ArgumentException>(() => service.CreateAsync(car));
    }

    [Fact]
    public async Task CreateAsync_VinTooLong_ThrowsArgumentException()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);
        var car = new Car
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Color = "Blue",
            Mileage = 1000,
            Price = 25000,
            VIN = "123456789012345678",
            FuelType = FuelType.Petrol
        };

        await Should.ThrowAsync<ArgumentException>(() => service.CreateAsync(car));
    }

    [Fact]
    public async Task CreateAsync_DuplicateVin_ThrowsInvalidOperationException()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);
        var vin = "12345678901234567";
        
        var car1 = new Car
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Color = "Blue",
            Mileage = 1000,
            Price = 25000,
            VIN = vin,
            FuelType = FuelType.Petrol
        };

        var car2 = new Car
        {
            Make = "Honda",
            Model = "Civic",
            Year = 2022,
            Color = "Red",
            Mileage = 2000,
            Price = 22000,
            VIN = vin,
            FuelType = FuelType.Diesel
        };

        await service.CreateAsync(car1);
        await Should.ThrowAsync<InvalidOperationException>(() => service.CreateAsync(car2));
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(2030)]
    public async Task CreateAsync_InvalidYear_ThrowsArgumentException(int year)
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);
        var car = new Car
        {
            Make = "Toyota",
            Model = "Camry",
            Year = year,
            Color = "Blue",
            Mileage = 1000,
            Price = 25000,
            VIN = "12345678901234567",
            FuelType = FuelType.Petrol
        };

        await Should.ThrowAsync<ArgumentException>(() => service.CreateAsync(car));
    }

    [Fact]
    public async Task CreateAsync_NegativeMileage_ThrowsArgumentException()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);
        var car = new Car
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Color = "Blue",
            Mileage = -100,
            Price = 25000,
            VIN = "12345678901234567",
            FuelType = FuelType.Petrol
        };

        await Should.ThrowAsync<ArgumentException>(() => service.CreateAsync(car));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCar_ReturnsCar()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);
        var car = new Car
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Color = "Blue",
            Mileage = 1000,
            Price = 25000,
            VIN = "12345678901234567",
            FuelType = FuelType.Petrol
        };

        var created = await service.CreateAsync(car);
        var result = await service.GetByIdAsync(created.Id);

        result.ShouldNotBeNull();
        result.Make.ShouldBe("Toyota");
    }

    [Fact]
    public async Task GetByIdAsync_NonexistentCar_ReturnsNull()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);

        var result = await service.GetByIdAsync(999);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task ReserveAsync_AvailableCar_ReservesSuccessfully()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);
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

        var created = await service.CreateAsync(car);
        var result = await service.ReserveAsync(created.Id);

        result.Status.ShouldBe(CarStatus.Reserved);
    }

    [Fact]
    public async Task ReserveAsync_ReservedCar_ThrowsInvalidOperationException()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);
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
            Status = CarStatus.Reserved
        };

        var created = await service.CreateAsync(car);

        await Should.ThrowAsync<InvalidOperationException>(() => service.ReserveAsync(created.Id));
    }

    [Fact]
    public async Task ReserveAsync_SoldCar_ThrowsInvalidOperationException()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);
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
            Status = CarStatus.Sold
        };

        var created = await service.CreateAsync(car);

        await Should.ThrowAsync<InvalidOperationException>(() => service.ReserveAsync(created.Id));
    }

    [Fact]
    public async Task GetAllAsync_WithFilters_ReturnsFilteredCars()
    {
        var db = CreateInMemoryDb();
        var service = new CarService(db);

        var car1 = new Car { Make = "Toyota", Model = "Camry", Year = 2023, Color = "Blue", Mileage = 1000, Price = 25000, VIN = "11111111111111111", FuelType = FuelType.Petrol };
        var car2 = new Car { Make = "Honda", Model = "Civic", Year = 2022, Color = "Red", Mileage = 2000, Price = 22000, VIN = "22222222222222222", FuelType = FuelType.Diesel };

        await service.CreateAsync(car1);
        await service.CreateAsync(car2);

        var result = await service.GetAllAsync(make: "Toyota");

        result.Count().ShouldBe(1);
        result.First().Make.ShouldBe("Toyota");
    }
}
