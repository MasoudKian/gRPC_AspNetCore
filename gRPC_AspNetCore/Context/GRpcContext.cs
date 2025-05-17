using gRPC_AspNetCore.Models;
using Microsoft.EntityFrameworkCore;

namespace gRPC_AspNetCore.Context
{
    public class GRpcContext : DbContext
    {
        public GRpcContext(DbContextOptions<GRpcContext> options) : base(options)
        {
            
        }

        public DbSet<Product> Products { get; set; }
    }
}
