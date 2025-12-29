namespace RetailOps.Domain.Entities;

public class IdempotencyKey
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public string? ResponseJson { get; set; }
    public int? StatusCode { get; set; }
    public DateTime CreatedAt { get; set; }
}
