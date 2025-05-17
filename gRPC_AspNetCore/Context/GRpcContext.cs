using Microsoft.EntityFrameworkCore;

namespace gRPC_AspNetCore.Context
{
    public class GRpcContext : DbContext
    {
        public GRpcContext(DbContextOptions<GRpcContext> options) : base(options)
        {
            
        }
    }
}
