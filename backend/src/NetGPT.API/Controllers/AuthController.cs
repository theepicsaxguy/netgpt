// Copyright (c) 2025 NetGPT. All rights reserved.

using Microsoft.AspNetCore.Mvc;

namespace NetGPT.API.Controllers
{
    /// <summary>
    /// Controller for authentication operations.
    /// Endpoint implementations are split into partial files to keep per-file size small.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed partial class AuthController(
        Application.Services.ITokenService tokenService,
        Infrastructure.Persistence.Repositories.RefreshTokenRepository refreshRepo,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        Microsoft.Extensions.Logging.ILogger<AuthController> logger,
        Application.Interfaces.IUserRepository userRepo,
        Infrastructure.Services.IPasswordHasher hasher) : ControllerBase
    {
        private readonly Application.Services.ITokenService tokenService = tokenService ?? throw new System.ArgumentNullException(nameof(tokenService));
        private readonly Infrastructure.Persistence.Repositories.RefreshTokenRepository refreshRepo = refreshRepo ?? throw new System.ArgumentNullException(nameof(refreshRepo));
        private readonly Microsoft.Extensions.Configuration.IConfiguration configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        private readonly Microsoft.Extensions.Logging.ILogger<AuthController> logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        private readonly Application.Interfaces.IUserRepository userRepo = userRepo ?? throw new System.ArgumentNullException(nameof(userRepo));
        private readonly Infrastructure.Services.IPasswordHasher hasher = hasher ?? throw new System.ArgumentNullException(nameof(hasher));
    }
}
