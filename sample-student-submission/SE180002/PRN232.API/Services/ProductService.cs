using PRN232.API.Models;

namespace PRN232.API.Services;

public interface IProductService
{
    IEnumerable<ProductItem> GetAll();
    ProductItem? GetById(Guid id);
    ProductItem Create(CreateProductDto dto);
    ProductItem? Update(Guid id, UpdateProductDto dto);
}

public class ProductService : IProductService
{
    private readonly Dictionary<Guid, ProductItem> _items = new();

    public ProductService()
    {
        var initial1 = new ProductItem { Id = Guid.NewGuid(), Name = "Gaming Monitor 27-inch", Price = 350.00m };
        var initial2 = new ProductItem { Id = Guid.NewGuid(), Name = "Mechanical Keyboard", Price = 89.99m };
        _items[initial1.Id] = initial1;
        _items[initial2.Id] = initial2;
    }

    public IEnumerable<ProductItem> GetAll() => _items.Values;

    public ProductItem? GetById(Guid id)
    {
        _items.TryGetValue(id, out var item);
        return item;
    }

    public ProductItem Create(CreateProductDto dto)
    {
        var entity = new ProductItem
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Price = dto.Price
        };
        _items[entity.Id] = entity;
        return entity;
    }

    public ProductItem? Update(Guid id, UpdateProductDto dto)
    {
        if (!_items.TryGetValue(id, out var entity))
        {
            return null;
        }

        entity.Name = dto.Name;
        entity.Price = dto.Price;
        return entity;
    }
}
