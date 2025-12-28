namespace RetailOps.Domain.Entities;

public class AttributeType
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // FABRICANTE | MARCA | CONTENIDO
    public string Name { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty; // PRODUCT | SKUJSON
}
