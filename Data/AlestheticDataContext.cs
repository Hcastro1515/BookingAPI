using AlestheticApi.Models;
using Microsoft.EntityFrameworkCore;

public class AlestheticDataContext : DbContext
{
    public AlestheticDataContext(DbContextOptions<AlestheticDataContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<User> Users { get; set; }    


}