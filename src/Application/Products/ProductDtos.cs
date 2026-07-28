namespace Application.Products;

public record CreateProductRequest(string Name, decimal Price, int CategoryId);
public record UpdateProductRequest(string Name, decimal Price, int CategoryId);
public record ProductResponse(int Id, string Name, decimal Price, int CategoryId, DateTime CreatedDate, DateTime ModifiedDate);
