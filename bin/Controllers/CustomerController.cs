using Kolosnycin.Data;
using Kolosnycin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace S.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/customer
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        return await _context.Customers
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    // GET: api/customer/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(long id)  // int -> long
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound(new { error = "Клиент не найден" });
        }

        return customer;
    }

    // POST: api/customer
    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // ВАЖНО: Убедитесь, что ID не передается извне
        customer.Id = 0;  // Принудительно сбрасываем ID, чтобы БД сгенерировала новый

        // Проверяем уникальность email
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == customer.Email);

        if (existingCustomer != null)
        {
            return BadRequest(new { error = "Клиент с таким email уже существует" });
        }

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    // PUT: api/customer/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(long id, Customer customer)  // int -> long
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (id != customer.Id)
        {
            return BadRequest(new { error = "ID не совпадают" });
        }

        // Проверяем существование клиента
        var existingCustomer = await _context.Customers.FindAsync(id);
        if (existingCustomer == null)
        {
            return NotFound(new { error = "Клиент не найден" });
        }

        // Проверяем уникальность email (если он изменился и не пустой)
        if (!string.IsNullOrWhiteSpace(customer.Email) &&
            existingCustomer.Email != customer.Email)
        {
            var emailExists = await _context.Customers
                .AnyAsync(c => c.Email == customer.Email && c.Id != id);

            if (emailExists)
            {
                return BadRequest(new { error = "Клиент с таким email уже существует" });
            }
        }

        // Обновляем поля
        existingCustomer.Name = customer.Name;
        existingCustomer.Email = customer.Email;
        existingCustomer.Address = customer.Address;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CustomerExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/customer/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(long id)  // int -> long
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound(new { error = "Клиент не найден" });
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CustomerExists(long id)
    {
        return _context.Customers.Any(e => e.Id == id);
    }

    private long? GetCurrentUserId()  // int? -> long?
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
        {
            return userId;
        }
        return null;
    }
}