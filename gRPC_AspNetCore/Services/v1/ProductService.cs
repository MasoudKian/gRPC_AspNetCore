﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using gRPC_AspNetCore.Context;
using gRPC_AspNetCore.Models;
using gRPC_AspNetCore.Protos.v1;
using Microsoft.EntityFrameworkCore;
using static gRPC_AspNetCore.Protos.v1.ProductService;

namespace gRPC_AspNetCore.Services.v1
{
    public class ProductService
        (GRpcContext dbContext) : ProductServiceBase
    {

        //grpcui -plaintext localhost:5286

        /// <summary>
        /// Bi-Directional Streaming RPC //// Created Product
        /// </summary>
        /// <param name="requestStream">server</param>
        /// <param name="responseStream">client</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task CreateProduct(IAsyncStreamReader<CreateProductRequest> requestStream
            , IServerStreamWriter<CreateProductReplay> responseStream, ServerCallContext context)
        {

            // Create a new product and save it to the database
            int createdProducCount = 0;

            while (await requestStream.MoveNext())
            {
                dbContext.Products.Add(new Product()
                {
                    CreatedDate = DateTime.Now,
                    Description = requestStream.Current.Description,
                    NameProduct = requestStream.Current.Nameproduct,
                    Price = requestStream.Current.Price,

                });
                createdProducCount++;
            }
            await dbContext.SaveChangesAsync();

            await responseStream.WriteAsync(new CreateProductReplay()
            {
                CreatedItemCount = createdProducCount,
                Message = "Created Successfully",
                Status = 200
            });
        }


        /// <summary>
        /// Unary RPC // Get Product By Id
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<GetProductByIdReplay> GetProductById(GetProductByIdRequest request
            , ServerCallContext context)
        {
            // Directly return null if the product is not found, avoiding unnecessary assignment.
            Product? product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == request.Id);

            if (product == null)
            {
                return null;
            }

            #region Send Response Headers 

            ///////// زمان ارسال قبل از ارسال پیام اصلی از سمت کلاینت یا سروز
            ///در ابتدای ارتباط ارسال میشن

            Metadata headers = new()
            {
                { "fName", "Masoud"},
                { "lName", "Kian" },
                { "age", "36" }
            };

            await context.WriteResponseHeadersAsync(headers);

            #endregion

            #region Send Response Trailers

            ////////بعد از ارسال همه پیام ها توسط سرور
            /// پیام های ( unary , stream )
            ///           

            context.ResponseTrailers.Add("FName", "Masoud");
            context.ResponseTrailers.Add("LName", "Kian");
            context.ResponseTrailers.Add("Done", "GetProductById");
            var trailers = new Metadata
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            //foreach (var entry in trailers)
            //{
            //    context.ResponseTrailers.Add(entry);
            //}

            context.ResponseTrailers.Clear();

            #endregion

            return new GetProductByIdReplay()
            {
                Id = product.Id,
                Nameproduct = product.NameProduct,
                Description = product.Description,
                Price = product.Price,
                CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(product.CreatedDate, DateTimeKind.Utc))
            };
        }


        /// <summary>
        /// server streaming RPC // Get All Products
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GetAllProduct(GetAllProductRequest request
            , IServerStreamWriter<GetAllProductReplay> responseStream, ServerCallContext context)
        {
            int skip = (request.Page - 1) * request.Take;
            List<Product> products = await dbContext.Products
                .Skip(skip)
                .Take(request.Take)
                .ToListAsync();

            foreach (var product in products)
            {
                await responseStream.WriteAsync(new GetAllProductReplay()
                {
                    Id = product.Id,
                    Nameproduct = product.NameProduct,
                    Description = product.Description,
                    Price = product.Price,
                    CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(product.CreatedDate, DateTimeKind.Utc))
                });
            }

        }


        /// <summary>
        /// unary RPC // Update Product
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>

        public override async Task<UpdateProductReplay> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {

            Product? product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == request.Id);
            if (product == null)
            {
                return new UpdateProductReplay()
                {
                    Message = "Product Not Found",
                    Status = 404,
                    UpdatedItemCount = 0,
                };
            }
            product.NameProduct = request.Nameproduct;
            product.Description = request.Description;
            product.Price = request.Price;

            dbContext.Products.Update(product);
            await dbContext.SaveChangesAsync();

            return new UpdateProductReplay()
            {
                Message = "Updated Successfully",
                Status = 200,
                UpdatedItemCount = 1,
            };

        }

        /// <summary>
        /// Client streaming RPC // Remove Product By Id
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<RemoveProductReply> RemoveProductById(IAsyncStreamReader<RemoveProductRequest> requestStream
            , ServerCallContext context)
        {
            int removedItemProduct = 0;
            while (await requestStream.MoveNext())
            {
                var product = dbContext.Products.FirstOrDefault(p => p.Id == requestStream.Current.Id);
                if (product == null)
                    continue;

                else if (product != null)
                {
                    dbContext.Products.Remove(product);
                    removedItemProduct++;
                }
            }
            await dbContext.SaveChangesAsync();
            return new RemoveProductReply()
            {
                Message = "Deleted Successfully",
                Status = 200,
                RemovedItemProduct = removedItemProduct
            };
        }
    }
}
