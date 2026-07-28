using Application.Products;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<ProductResponse>>> GetAll(IProductService svc, CancellationToken ct) =>
        TypedResults.Ok(await svc.GetAllAsync(ct));

    private static async Task<IResult> GetById(int id, IProductService svc, CancellationToken ct)
    {
        var product = await svc.GetByIdAsync(id, ct);
        return product is null ? Results.NotFound() : Results.Ok(product);
    }

    private static async Task<Created<ProductResponse>> Create(CreateProductRequest request, IProductService svc, CancellationToken ct)
    {
        var created = await svc.CreateAsync(request, ct);
        return TypedResults.Created($"/api/products/{created.Id}", created);
    }

    private static async Task<IResult> Update(int id, UpdateProductRequest request, IProductService svc, CancellationToken ct)
    {
        var updated = await svc.UpdateAsync(id, request, ct);
        return updated ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> Delete(int id, IProductService svc, CancellationToken ct)
    {
        var deleted = await svc.DeleteAsync(id, ct);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}
