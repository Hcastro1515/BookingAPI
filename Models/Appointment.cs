using System.Text.Json.Serialization;

namespace AlestheticApi.Models;

public class Appointment
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }
    public int ServiceId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public Customer Customer { get; set; }
    public Employee Employee { get; set; }
    public Service Service { get; set; }
}