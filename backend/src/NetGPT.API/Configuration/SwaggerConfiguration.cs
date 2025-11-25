// Copyright (c) 2025 NetGPT. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace NetGPT.API.Configuration
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            _ = services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "NetGPT API",
                    Version = "v1",
                    Description = "ChatGPT clone with Agent Framework",
                });

                // Note: Schemas are auto-generated from DTOs used in controllers.
            });

            return services;
        }
    }
}
