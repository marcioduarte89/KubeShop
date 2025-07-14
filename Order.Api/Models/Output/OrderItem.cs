namespace Order.Api.Models.Ouput
{
    public class OrderItem
    {
        public int? Id { get; set; }

        public Guid ProductUId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}
