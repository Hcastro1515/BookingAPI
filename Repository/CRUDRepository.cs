using AlestheticApi.Models;
using AlestheticApi.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace AlestheticApi.Repository;

public class CRUDRepository<T> : ICRUDRepository<T> where T : class
{
    private readonly AlestheticDataContext _context;

    public CRUDRepository(AlestheticDataContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public async Task<ICollection<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<Appointment?> GetAllAppointmentsIncludingCustomerAndEmployeeAsync()
    {
        return await _context.Appointments
            .Include(e => e.Employee)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync();
    }

    public async Task<T?> GetByEmail(string email)
    {
        var emailProperty = typeof(T).GetProperty("Email");
        if (emailProperty == null)
        {
            return null;
        }

        var entities = await _context.Set<T>().ToListAsync();
        return entities.FirstOrDefault(x => emailProperty.GetValue(x) != null && emailProperty.GetValue(x).ToString() == email);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T?> GetByName(string name)
    {
        var nameProperty = typeof(T).GetProperty("Name");
        if (nameProperty == null)
        {
            return null;
        }

        var entities = await _context.Set<T>().ToListAsync();
        return entities.FirstOrDefault(x => nameProperty.GetValue(x) != null && nameProperty.GetValue(x).ToString() == name);
    }

    public async Task RemoveAsync(T entity)
    {
        await Task.Run(() => _context.Set<T>().Remove(entity));
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        await Task.Run(() => _context.Set<T>().Update(entity));
    }
}