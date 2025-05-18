namespace gRPC_AspNetCore.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string NameProduct { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
