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

            Console.WriteLine($"HOST {_httpClient.BaseAddress.AbsoluteUri} products{query}");

            var response = await _httpClient.GetAsync($"products{query}");
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response: {content}");
            var result = response.EnsureSuccessStatusCode(); // This will throw for 404, 500 etc.

            Console.WriteLine($"Http Response: {result.StatusCode}");

            var product = await _httpClient.GetFromJsonAsync<IEnumerable<Product>>($"products{query}", new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            Console.WriteLine($"Managed to deserialize it");

            return product;
        }
    }
}
