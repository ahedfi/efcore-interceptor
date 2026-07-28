using Domain.Common;

namespace Domain.Entities;

public class Category : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public List<Product> Products { get; set; } = new();
}
