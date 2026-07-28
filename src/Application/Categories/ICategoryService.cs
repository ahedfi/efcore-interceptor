namespace Application.Categories;

public interface ICategoryService
{
    Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CategoryResponse>> GetAllIncludingDeletedAsync(CancellationToken ct = default);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
