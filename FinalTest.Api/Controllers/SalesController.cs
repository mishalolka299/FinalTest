using FinalTest.Api.Domain;
using FinalTest.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinalTest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await _saleService.CreateAsync(request.CarId, request.CustomerId, request.SalePrice, request.PaymentMethod, cancellationToken);
            return CreatedAtAction(nameof(GetByDateRange), new { }, sale);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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

    [HttpGet]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var sales = await _saleService.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return Ok(sales);
    }
}

public record CreateSaleRequest(int CarId, int CustomerId, decimal SalePrice, PaymentMethod PaymentMethod);
