using AlestheticApi.Models;

namespace AlestheticApi.Repository.IRepository; 

public interface ICRUDRepository<T> where T : class
{
    Task<ICollection<T>> GetAllAsync(); 
    Task<Appointment?> GetAllAppointmentsIncludingCustomerAndEmployeeAsync(); 
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByName(string name); 
    Task<T?> GetByEmail(string email);
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task RemoveAsync(T entity);
    Task SaveAsync();
}