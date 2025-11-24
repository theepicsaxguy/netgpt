// <copyright file="HealthController.cs" company="PlaceholderCompany">
// \
// </copyright>

namespace NetGPT.API.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public sealed class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
