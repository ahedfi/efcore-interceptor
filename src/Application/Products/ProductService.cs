using Domain.Entities;
using Domain.Repositories;

namespace Application.Products;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, ct);
        return product is null ? null : MapToResponse(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var products = await _unitOfWork.Repository<Product>().GetAllAsync(ct);
        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            CategoryId = request.CategoryId
        };
        await _unitOfWork.Repository<Product>().AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(product);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, ct);
        if (product is null) return false;

        product.Name = request.Name;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;
        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, ct);
        if (product is null) return false;

        _unitOfWork.Repository<Product>().Remove(product);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    private static ProductResponse MapToResponse(Product product) =>
        new(product.Id, product.Name, product.Price, product.CategoryId, product.CreatedDate, product.ModifiedDate);
}
