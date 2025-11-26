// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Entities;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NetGPT.API.Controllers
{
    [ApiController]
    [Route("api/declarative/definitions")]
    public sealed class DeclarativeDefinitionsController(
        IDefinitionRepository repo,
        IDeclarativeLoader loader,
        IAgentOrchestrator orchestrator,
        ILogger<DeclarativeDefinitionsController> logger) : ControllerBase
    {
        private readonly IDefinitionRepository repo = repo;
        private readonly IDeclarativeLoader loader = loader;
        private readonly IAgentOrchestrator orchestrator = orchestrator;
        private readonly ILogger<DeclarativeDefinitionsController> logger = logger;

        public record CreateDefinitionRequest(string Name, string Kind, string ContentYaml);
        public record DefinitionDto(Guid Id, string Name, string Kind, int Version, string CreatedBy, DateTime CreatedAtUtc);
        public record ExecuteRequest(string? Input);

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateDefinitionRequest request)
        {
            if (request is null)
            {
                return BadRequest("Request body is required");
            }

            // Basic YAML parse validation to provide early feedback about syntax
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            try
            {
                _ = deserializer.Deserialize<object>(request.ContentYaml);
            }
            catch (YamlException yex)
            {
                // Return parser location and message
                return BadRequest(new { message = yex.Message, line = yex.Start.Line + 1, column = yex.Start.Column + 1 });
            }

            DefinitionEntity def = new()
            {
                Name = request.Name,
                Kind = request.Kind,
                ContentYaml = request.ContentYaml,
                CreatedBy = User?.Identity?.Name ?? "api",
            };

            try
            {
                await loader.LoadAsync(def);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }

            try
            {
                var created = await repo.CreateAsync(def);
                DefinitionDto dto = new(created.Id, created.Name, created.Kind, created.Version, created.CreatedBy, created.CreatedAtUtc);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, dto);
            }
            catch (InvalidOperationException inv)
            {
                // Repository-level validation (e.g., raw secrets) -> 422
                return UnprocessableEntity(new { error = inv.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create declarative definition {Name}", request.Name);
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var items = await repo.ListLatestAsync(page, pageSize);
            var dtos = items.Select(d => new DefinitionDto(d.Id, d.Name, d.Kind, d.Version, d.CreatedBy, d.CreatedAtUtc));
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var def = await repo.GetByIdAsync(id);
            if (def == null)
            {
                return NotFound();
            }

            DefinitionDto dto = new(def.Id, def.Name, def.Kind, def.Version, def.CreatedBy, def.CreatedAtUtc);
            return Ok(dto);
        }

        [HttpPost("{id:guid}/execute")]
        [Authorize]
        public async Task<IActionResult> Execute([FromRoute] Guid id, [FromBody] ExecuteRequest? request)
        {
            var def = await repo.GetByIdAsync(id);
            if (def == null)
            {
                return NotFound();
            }

            Guid executionId = Guid.NewGuid();
            DateTime startTime = DateTime.UtcNow;
            try
            {
                var exec = await loader.LoadAsync(def);

                var result = await orchestrator.ExecuteDefinitionAsync(def, exec, request?.Input ?? string.Empty);

                DateTime endTime = DateTime.UtcNow;

                if (result.IsSuccess)
                {
                    logger.LogInformation("Declarative execution completed: definitionId={DefinitionId} version={Version} executionId={ExecutionId} start={Start} end={End} outcome={Outcome}", def.Id, def.Version, executionId, startTime, endTime, "success");
                    return Ok(new { definitionId = def.Id, version = def.Version, executionId, startTime, endTime, result = result.Value });
                }
                else
                {
                    logger.LogWarning("Declarative execution failed: definitionId={DefinitionId} version={Version} executionId={ExecutionId} start={Start} end={End} outcome=failure", def.Id, def.Version, executionId, startTime, endTime);
                    return UnprocessableEntity(new { definitionId = def.Id, executionId, error = result.Errors.Select(e => e.Message) });
                }
            }
            catch (InvalidOperationException inv)
            {
                DateTime endTime = DateTime.UtcNow;
                logger.LogWarning(inv, "Declarative execution failed for {DefinitionId} v{Version} executionId={ExecutionId}", def.Id, def.Version, executionId);
                return UnprocessableEntity(new { definitionId = def.Id, executionId, error = inv.Message });
            }
            catch (Exception ex)
            {
                DateTime endTime = DateTime.UtcNow;
                logger.LogError(ex, "Declarative execution error for {DefinitionId} v{Version} executionId={ExecutionId}", def.Id, def.Version, executionId);
                return StatusCode(500, new { definitionId = def.Id, executionId, error = ex.Message });
            }
        }
    }
}
