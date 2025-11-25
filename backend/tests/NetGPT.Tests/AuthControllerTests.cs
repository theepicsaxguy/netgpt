using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NetGPT.API.Controllers;
using NetGPT.Application.DTOs.Auth;
using NetGPT.Infrastructure.Persistence;
using NetGPT.Infrastructure.Services;
using Xunit;

namespace NetGPT.Tests
{
    public class AuthControllerTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task RegisterThenLogin()
        {
            var ctx = CreateContext();
            var userRepo = new NetGPT.Infrastructure.Persistence.Repositories.UserRepository(ctx);
            var refreshRepo = new NetGPT.Infrastructure.Persistence.Repositories.RefreshTokenRepository(ctx);
            var tokenSvc = new TokenService(new ConfigurationBuilder().AddInMemoryCollection().Build());
            var hasher = new PasswordHasher();

            var controller = new AuthController(tokenSvc, refreshRepo, new ConfigurationBuilder().Build(), NullLogger<AuthController>.Instance, userRepo, hasher);

            var register = new RegisterRequestDto { Username = "alice", Password = "pass", Name = "Alice" };
            var regRes = await controller.Register(register);
            Assert.IsType<OkResult>(regRes);

            var loginReq = new LoginRequestDto { Username = "alice", Password = "pass" };
            var loginRes = await controller.Login(loginReq);
            Assert.IsType<OkObjectResult>(loginRes);
        }
    }
}
