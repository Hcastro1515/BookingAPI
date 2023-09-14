using System.Net;

namespace AlestheticApi.Models;

public class APIResponse
{
    public APIResponse()
    {
        ErrorMessages = new List<string>();
    }

    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Result { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public List<string>? ErrorMessages { get; set; }
}