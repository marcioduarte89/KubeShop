namespace Order.Api.Extensions
{
    public static class OrderExtensions
    {
        public static Models.Ouput.Order ToOutputOrder(this Models.Order order)
        {
            return new Models.Ouput.Order()
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Items = order.Items.Select(x => new Models.Ouput.OrderItem()
                {
                    Id = x.Id,
                    ProductUId = x.ProductUId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                }).ToList()
            };
        }

        public static Models.Order ToModelOrder(this Models.Input.Order order)
        {
            return new Models.Order()
            {
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Items = order.Items.Select(x => new Models.OrderItem()
                {
                    Id = x.Id,
                    ProductUId = x.ProductUId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                }).ToList()
            };
        }
    }
}
