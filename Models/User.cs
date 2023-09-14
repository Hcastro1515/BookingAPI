using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AlestheticApi.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

}