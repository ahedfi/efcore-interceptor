using Domain.Entities;
using Domain.Repositories;

namespace Application.Categories;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, ct);
        return category is null ? null : MapToResponse(category);
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync(ct);
        return categories.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllIncludingDeletedAsync(CancellationToken ct = default)
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllIgnoringFiltersAsync(ct);
        return categories.Select(MapToResponse).ToList();
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var category = new Category { Name = request.Name };
        await _unitOfWork.Repository<Category>().AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(category);
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, ct);
        if (category is null) return false;

        category.Name = request.Name;
        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id, ct);
        if (category is null) return false;

        _unitOfWork.Repository<Category>().Remove(category);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    private static CategoryResponse MapToResponse(Category category) =>
        new(category.Id, category.Name, category.IsDeleted);
}
