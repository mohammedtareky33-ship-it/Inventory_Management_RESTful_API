
using InventoryBL.Interfaces;
using InventoryBL.Services;
using InventoryDAL.Interfaces;
using InventoryDAL.Repos;
using InventoryManagemetRESTFUL_API.Authoraization;
using InventoryManagemetRESTFUL_API.Authorization;
using InventoryShared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
namespace InventoryManagemetRESTFUL_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRateLimiter(options => {
                options.RejectionStatusCode=StatusCodes.Status429TooManyRequests;
                options.AddPolicy("AuthLimiter", httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString()??"Unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey: ip, factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(2),
                        QueueLimit = 0
                    });


                });
            });
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {

                    Name = "Authorization",



                    Type = SecuritySchemeType.Http,

                    Scheme = "Bearer",


                    In = ParameterLocation.Header,

                    Description = "Enter: Bearer {your JWT token}"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {

                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },



                    new string[] { } } }
                );
            });
            //Configure Connection String
            string connictionString = builder.Configuration.GetConnectionString("InventoryDB")??throw new Exception("Connection String Not Found");
            DataAccessSettings.ConnectionString=connictionString;
           


            //Buisness Layer DI
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IBatchService, BatchService>();
            builder.Services.AddScoped<IBatchServiceForInvoice, BatchService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            // Data Access Layer DI
            builder.Services.AddScoped<IUserDAL, UserDAL>();
            builder.Services.AddScoped<IProductDAL, ProductDAL>();
            builder.Services.AddScoped<IBatchDAL, BatchDAL>();
            builder.Services.AddScoped<IInvoiceItemDAL, InvoiceItemDAL>();
            builder.Services.AddScoped<IInvoiceDAL,InvoiceDAL>();
            builder.Services.AddScoped<IStockMovementDAL, StockMovementDAL>();
            //AddCORS
            builder.Services.AddCors(options=>options.AddPolicy("InventoryAPICORS",policy=>policy.WithOrigins("https://localhost:5215").AllowAnyHeader().AllowAnyMethod()));
            //AddAuthentication
            var secretKeyValue = Environment.GetEnvironmentVariable("JWT__KEY");
            if (string.IsNullOrEmpty(secretKeyValue))
            {
                throw new Exception("JWT Secret Key is missing from environment variables");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
                options =>
                {
                    options.TokenValidationParameters=new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateIssuer=true,
                        ValidateAudience=true,
                        ValidateLifetime=true,
                        ValidateIssuerSigningKey=true,
                        ValidIssuer="InventoryAPI",
                        ValidAudience="InventoryAPIUsers",
                        IssuerSigningKey=new SymmetricSecurityKey( Encoding.UTF8.GetBytes(secretKeyValue))
                    };

                }
                );
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionsHandler>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionsPolicyProvider>();
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionUsersOrUserOwnershipHandler>();
            builder.Services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("PermissionUsersOrUserOwnership", policy => policy.Requirements.Add(new PermissionUsersOrUserOwnershipRequirement()));
                }
                );

      
            var app = builder.Build();
           
          
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseExceptionHandler(error =>
            {
                error.Run(async context =>
                {
                    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    context.Response.ContentType = "application/json";
                    if (ex is NotFoundException)
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = ex.Message
                        });
                        return;
                    }

                    if (ex is ValidationException)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            message = ex.Message
                        });
                        return;

                    }

                  
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = ex.Message
                    });
                });


            });
            app.UseHttpsRedirection();

            app.UseCors("InventoryAPICORS");
            app.UseRateLimiter();
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    await context.Response.WriteAsync("Too many login attempts. Please try again later.");
                }
            });

            app.UseAuthentication();
            app.UseAuthorization();


            app.Use(async (context, next) =>
            {
                await next();

                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var path = context.Request.Path.ToString();

                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {




                    app.Logger.LogWarning(
                        "Forbidden access. UserId={UserId}, Path={Path}, IP={IP}",
                        userId,
                        path,
                        ip
                    );

                }

            });

            app.MapControllers();

            app.Run();
        }
    }
}
