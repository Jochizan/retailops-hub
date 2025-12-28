using Microsoft.Extensions.DependencyInjection;
using RetailOps.Application.Interfaces;
using RetailOps.Application.Services;

namespace RetailOps.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IInventoryService, InventoryService>();
            return services;
        }
    }
}
