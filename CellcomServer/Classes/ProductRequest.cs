namespace CellcomServer.Classes
{
    public class ProductRequest
    {
        public string ProductID { get; set; }

        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }

        public bool ToUpdate { get; set; }

        public IFormFile? Image { get; set; }
    }
}
