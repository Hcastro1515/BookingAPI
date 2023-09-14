namespace AlestheticApi.Models.DTOs; 

public class ServiceDTO
{
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Duration { get; set; }
    public decimal Price { get; set; }
}