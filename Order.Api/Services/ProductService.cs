using Order.Api.Models;
using System.Text.Json;

namespace Order.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Product> GetProduct(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _httpClient.GetFromJsonAsync<Product>($"products/{productId}");
            return product;
        }

        public async Task<IEnumerable<Product>> GetProducts(IEnumerable<Guid> productUIds, CancellationToken cancellationToken)
        {
            var query = "";
            for (int i = 0; i < productUIds.Count(); i++)
            {
                if (i == 0)
                {
                    query += $"?productUIds={productUIds.ElementAt(i)}";
                }
                else
                {
                    query += $"&productUIds={productUIds.ElementAt(i)}";
                }
            }

            var product = await _httpClient.GetFromJsonAsync<IEnumerable<Product>>($"products{query}", new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            return product;
        }
    }
}
