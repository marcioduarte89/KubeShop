using Order.Api.Models;

namespace Order.Api.Services
{
    public interface IProductService
    {
        Task<Product> GetProduct(Guid productId, CancellationToken cancellationToken);

        Task<IEnumerable<Product>> GetProducts(IEnumerable<Guid> productIds, CancellationToken cancellationToken);
    }
}
