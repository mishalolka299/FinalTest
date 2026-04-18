using FinalTest.Api.Domain;
using FinalTest.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinalTest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly ICarService _carService;

    public CarsController(ICarService carService)
    {
        _carService = carService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? make = null,
        [FromQuery] int? yearMin = null,
        [FromQuery] int? yearMax = null,
        [FromQuery] decimal? priceMin = null,
        [FromQuery] decimal? priceMax = null,
        [FromQuery] FuelType? fuelType = null,
        [FromQuery] CarStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var cars = await _carService.GetAllAsync(make, yearMin, yearMax, priceMin, priceMax, fuelType, status, cancellationToken);
        return Ok(cars);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var car = await _carService.GetByIdAsync(id, cancellationToken);
        if (car == null)
            return NotFound();
        return Ok(car);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCarRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var car = new Car
            {
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                Color = request.Color,
                Mileage = request.Mileage,
                Price = request.Price,
                VIN = request.VIN,
                FuelType = request.FuelType,
                Status = CarStatus.Available
            };

            var created = await _carService.CreateAsync(car, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCarRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _carService.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return NotFound();

            existing.Make = request.Make;
            existing.Model = request.Model;
            existing.Year = request.Year;
            existing.Color = request.Color;
            existing.Mileage = request.Mileage;
            existing.Price = request.Price;
            existing.FuelType = request.FuelType;

            var updated = await _carService.UpdateAsync(existing, cancellationToken);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id}/reserve")]
    public async Task<IActionResult> Reserve(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var car = await _carService.ReserveAsync(id, cancellationToken);
            return Ok(car);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public record CreateCarRequest(string Make, string Model, int Year, string Color, decimal Mileage, decimal Price, string VIN, FuelType FuelType);
public record UpdateCarRequest(string Make, string Model, int Year, string Color, decimal Mileage, decimal Price, FuelType FuelType);
