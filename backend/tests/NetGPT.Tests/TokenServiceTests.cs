using System;
using Microsoft.Extensions.Configuration;
using NetGPT.Infrastructure.Services;
using Xunit;

namespace NetGPT.Tests
{
    public class TokenServiceTests
    {
        [Fact]
        public void CreateAndValidateAccessToken()
        {
            var inMemory = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var svc = new TokenService(inMemory);

            var user = new { Id = Guid.NewGuid(), Name = "Tester", Roles = new string[] { "User" } };
            string token = svc.CreateAccessToken(user);
            Assert.False(string.IsNullOrEmpty(token));

            var principal = svc.ValidateAccessToken(token);
            Assert.NotNull(principal);
            Assert.Equal(user.Id.ToString(), principal?.FindFirst("sub")?.Value);
        }
    }
}
