using FinalTest.Api.Data;
using FinalTest.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalTest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ISaleService _saleService;

    public CustomersController(AppDbContext db, ISaleService saleService)
    {
        _db = db;
        _saleService = saleService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer == null)
            return NotFound();
        return Ok(customer);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var customers = await _db.Customers.ToListAsync(cancellationToken);
        return Ok(customers);
    }

    [HttpGet("{id}/purchases")]
    public async Task<IActionResult> GetPurchases(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer == null)
            return NotFound();

        var purchases = await _saleService.GetByCustomerAsync(id, cancellationToken);
        return Ok(purchases);
    }
}
