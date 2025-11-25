// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NetGPT.Application.DTOs;

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

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                });

                // Note: Property-level schema declarations for DTOs to improve Swagger UI discovery.
                options.MapType<ToolInvocationDto>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["ToolName"] = new OpenApiSchema { Type = "string" },
                        ["Arguments"] = new OpenApiSchema { Type = "string" },
                        ["Result"] = new OpenApiSchema { Type = "string", Nullable = true },
                        ["InvokedAt"] = new OpenApiSchema { Type = "string", Format = "date-time" },
                        ["DurationMs"] = new OpenApiSchema { Type = "number", Format = "double" },
                    },
                    Required = new HashSet<string> { "ToolName", "Arguments", "InvokedAt", "DurationMs" },
                });
                options.MapType<AgentDefinitionDto>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["Name"] = new OpenApiSchema { Type = "string" },
                        ["Instructions"] = new OpenApiSchema { Type = "string" },
                        ["ModelName"] = new OpenApiSchema { Type = "string", Nullable = true },
                        ["Temperature"] = new OpenApiSchema { Type = "number", Format = "float", Nullable = true },
                        ["MaxTokens"] = new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true },
                    },
                    Required = new HashSet<string> { "Name", "Instructions" },
                });
                options.MapType<AgentConfigurationDto>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["ModelName"] = new OpenApiSchema { Type = "string", Nullable = true },
                        ["Temperature"] = new OpenApiSchema { Type = "number", Format = "float", Nullable = true },
                        ["MaxTokens"] = new OpenApiSchema { Type = "integer", Format = "int32", Nullable = true },
                        ["CustomProperties"] = new OpenApiSchema { Type = "object", AdditionalProperties = new OpenApiSchema { Type = "object" }, Nullable = true },
                        ["Agents"] = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "AgentDefinitionDto" } }, Nullable = true },
                    },
                });
                options.MapType<UpdateConversationRequest>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["title"] = new OpenApiSchema { Type = "string", Nullable = true },
                        ["configuration"] = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "AgentConfigurationDto" }, Nullable = true },
                    },
                });
                options.MapType<RegenerateResponseRequest>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["conversationId"] = new OpenApiSchema { Type = "string", Format = "uuid" },
                        ["messageId"] = new OpenApiSchema { Type = "string", Format = "uuid" },
                    },
                    Required = new HashSet<string> { "conversationId", "messageId" },
                });
                options.MapType<FileAttachmentDto>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["url"] = new OpenApiSchema { Type = "string", Format = "uri" },
                        ["name"] = new OpenApiSchema { Type = "string" },
                        ["size"] = new OpenApiSchema { Type = "integer" },
                        ["contentType"] = new OpenApiSchema { Type = "string" },
                    },
                    Required = new HashSet<string> { "url", "name", "size", "contentType" },
                });
                options.MapType<StreamingChunkDto>(() => new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["chunkId"] = new OpenApiSchema { Type = "string", Format = "uuid" },
                        ["text"] = new OpenApiSchema { Type = "string" },
                        ["isFinal"] = new OpenApiSchema { Type = "boolean" },
                        ["createdAt"] = new OpenApiSchema { Type = "string", Format = "date-time" },
                    },
                    Required = new HashSet<string> { "chunkId", "text", "isFinal", "createdAt" },
                });
            });

            return services;
        }
    }
}
