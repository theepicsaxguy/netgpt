using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using NetGPT.Infrastructure.Declarative;

namespace NetGPT.API.Controllers
{
    [ApiController]
    [Route("api/declarative/definitions")]
    public sealed class DeclarativeDefinitionsController : ControllerBase
    {
        private readonly IDefinitionRepository repo;
        private readonly IDeclarativeLoader loader;
        private readonly NetGPT.Application.Interfaces.IAgentOrchestrator orchestrator;
        private readonly ILogger<DeclarativeDefinitionsController> logger;

        public DeclarativeDefinitionsController(
            IDefinitionRepository repo,
            IDeclarativeLoader loader,
            NetGPT.Application.Interfaces.IAgentOrchestrator orchestrator,
            ILogger<DeclarativeDefinitionsController> logger)
        {
            this.repo = repo;
            this.loader = loader;
            this.orchestrator = orchestrator;
            this.logger = logger;
        }

        public record CreateDefinitionRequest(string Name, string Kind, string ContentYaml);
        public record DefinitionDto(Guid Id, string Name, string Kind, int Version, string CreatedBy, DateTime CreatedAtUtc);
        public record ExecuteRequest(string? Input);

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateDefinitionRequest request)
        {
            if (request is null) return BadRequest("Request body is required");

            // Basic YAML parse validation to provide early feedback about syntax
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            try
            {
                _ = deserializer.Deserialize<object>(request.ContentYaml);
            }
            catch (YamlException yex)
            {
                // Return parser location and message
                return BadRequest(new { message = yex.Message, start = yex.Start, end = yex.End });
            }

            var def = new DefinitionEntity
            {
                Name = request.Name,
                Kind = request.Kind,
                ContentYaml = request.ContentYaml,
                CreatedBy = User?.Identity?.Name ?? "api",
            };

            try
            {
                var created = await repo.CreateAsync(def);
                var dto = new DefinitionDto(created.Id, created.Name, created.Kind, created.Version, created.CreatedBy, created.CreatedAtUtc);
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
            if (def == null) return NotFound();
            var dto = new DefinitionDto(def.Id, def.Name, def.Kind, def.Version, def.CreatedBy, def.CreatedAtUtc);
            return Ok(dto);
        }

        [HttpPost("{id:guid}/execute")]
        [Authorize]
        public async Task<IActionResult> Execute([FromRoute] Guid id, [FromBody] ExecuteRequest? request)
        {
            var def = await repo.GetByIdAsync(id);
            if (def == null) return NotFound();

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
