using System.Text.Json.Serialization;

namespace AlestheticApi.Models;

public class Service
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Duration { get; set; }
    public decimal Price { get; set; }
    [JsonIgnore]
    public List<Appointment>? Appointments { get; set; }
    
}