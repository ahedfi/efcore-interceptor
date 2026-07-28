using Application.Categories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", GetAll);
        group.MapGet("/all", GetAllIncludingDeleted);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<CategoryResponse>>> GetAll(ICategoryService svc, CancellationToken ct) =>
        TypedResults.Ok(await svc.GetAllAsync(ct));

    private static async Task<Ok<IReadOnlyList<CategoryResponse>>> GetAllIncludingDeleted(ICategoryService svc, CancellationToken ct) =>
        TypedResults.Ok(await svc.GetAllIncludingDeletedAsync(ct));

    private static async Task<IResult> GetById(int id, ICategoryService svc, CancellationToken ct)
    {
        var category = await svc.GetByIdAsync(id, ct);
        return category is null ? Results.NotFound() : Results.Ok(category);
    }

    private static async Task<Created<CategoryResponse>> Create(CreateCategoryRequest request, ICategoryService svc, CancellationToken ct)
    {
        var created = await svc.CreateAsync(request, ct);
        return TypedResults.Created($"/api/categories/{created.Id}", created);
    }

    private static async Task<IResult> Update(int id, UpdateCategoryRequest request, ICategoryService svc, CancellationToken ct)
    {
        var updated = await svc.UpdateAsync(id, request, ct);
        return updated ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> Delete(int id, ICategoryService svc, CancellationToken ct)
    {
        var deleted = await svc.DeleteAsync(id, ct);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}
