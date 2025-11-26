// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetGPT.Application.Services;

namespace NetGPT.Infrastructure.Services
{
    public sealed class TokenService : ITokenService
    {
        private readonly IConfiguration configuration;
        private readonly JwtSecurityTokenHandler tokenHandler = new();
        private readonly SigningCredentials signingCredentials;
        private readonly TokenValidationParameters validationParameters;

        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;

            string issuer = configuration["Jwt:Issuer"] ?? "netgpt";
            string audience = configuration["Jwt:Audience"] ?? "netgpt_clients";

            // Prefer RSA key if provided, otherwise use symmetric secret
            string? rsaPrivatePem = configuration["Jwt:RSAPrivatePem"];
            if (!string.IsNullOrWhiteSpace(rsaPrivatePem))
            {
                using RSA rsa = RSA.Create();
                rsa.ImportFromPem(rsaPrivatePem.ToCharArray());
                RsaSecurityKey key = new(rsa);
                signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
                validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            }
            else
            {
                string secret = configuration["Jwt:Secret"] ?? "netgpt_dev_secret_change_me";
                SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(secret));
                signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            }
        }

        public string CreateAccessToken(object user, TimeSpan? lifetime = null)
        {
            // user is intentionally typed as object per interface; expect a dynamic with Id and Name or a Guid
            string sub = user switch
            {
                Guid g => g.ToString(),
                string s when Guid.TryParse(s, out Guid _) => s,
                var u => u?.GetType().GetProperty("Id")?.GetValue(u)?.ToString() ?? string.Empty,
            };

            DateTime now = DateTime.UtcNow;
            TimeSpan tokenLifetime = lifetime ?? TimeSpan.FromMinutes(15);

            List<Claim> claimsList = new()
            {
                new Claim(JwtRegisteredClaimNames.Sub, sub ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)now).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
            };

            // Optionally include name and roles if provided on the user object
            if (user != null)
            {
                Type type = user.GetType();
                PropertyInfo? nameProp = type.GetProperty("Name");
                if (nameProp != null)
                {
                    string? nameVal = nameProp.GetValue(user)?.ToString();
                    if (!string.IsNullOrEmpty(nameVal))
                    {
                        claimsList.Add(new Claim(ClaimTypes.Name, nameVal!));
                    }
                }

                PropertyInfo? rolesProp = type.GetProperty("Roles");
                if (rolesProp != null)
                {
                    if (rolesProp.GetValue(user) is IEnumerable rolesVal)
                    {
                        foreach (object? r in rolesVal)
                        {
                            if (r != null)
                            {
                                claimsList.Add(new Claim(ClaimTypes.Role, r.ToString()!));
                            }
                        }
                    }
                }
            }

            JwtSecurityToken jwt = new(
                issuer: validationParameters.ValidIssuer,
                audience: validationParameters.ValidAudience,
                claims: claimsList,
                notBefore: now,
                expires: now.Add(tokenLifetime),
                signingCredentials: signingCredentials);

            return tokenHandler.WriteToken(jwt);
        }

        public (string Token, DateTime ExpiresAt) CreateRefreshToken()
        {
            byte[] data = new byte[64];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(data);
            string token = Base64UrlEncoder.Encode(data);
            DateTime expiresAt = DateTime.UtcNow.AddDays(30);
            return (token, expiresAt);
        }

        public ClaimsPrincipal? ValidateAccessToken(string token, bool validateLifetime = true)
        {
            try
            {
                TokenValidationParameters parameters = validationParameters.Clone();
                parameters.ValidateLifetime = validateLifetime;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public string HashRefreshToken(string token)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(token);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
