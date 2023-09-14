namespace AlestheticApi.Models.DTOs;

public class AppointmentDTO
{
    public int EmployeeId { get; set; }
    public int ServiceId { get; set; }
    public int CustomerId{ get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}