using gRPC_AspNetCore.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Add DbContext

builder.Services.AddDbContext<GRpcContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("gRPCConnectionString"));
});

#endregion

#region Reflection gRPC

builder.Services.AddGrpcReflection();

#endregion

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();


// Configure the HTTP request pipeline.
//
//app.MapGrpcService<ProductService>();

app.MapGrpcService<gRPC_AspNetCore.Services.v1.ProductService>();
app.MapGrpcService<gRPC_AspNetCore.Services.v2.ProductService>();

#region Continue for Reflection

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

#endregion

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
