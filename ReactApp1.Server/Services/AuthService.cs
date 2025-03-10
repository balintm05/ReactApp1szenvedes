﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using ReactApp1.Server.Data;
using ReactApp1.Server.Entities;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using ReactApp1.Server.Models.JWT;
using ReactApp1.Server.Models.User;
using ReactApp1.Server.Models.User.EditUser;
using NuGet.Common;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

//https://www.youtube.com/watch?v=6EEltKS8AwA

namespace ReactApp1.Server.Services
{
    public class AuthService(DataBaseContext context, IConfiguration configuration):IAuthService
    {
        public async Task<TokenResponseDto?> LoginAsync(AuthUserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.email == request.email);
            if (user == null || new PasswordHasher<User>().VerifyHashedPassword(user, user.passwordHash, request.password) == PasswordVerificationResult.Failed)
            {
                return null;
            };
            return await CreateTokenResponse(user);

        }       

        public async Task<TokenResponseDto?> RegisterAsync(AuthUserDto request)
        {            
            if (await context.Users.AnyAsync(u => u.email == request.email)|| !new EmailAddressAttribute().IsValid(request.email)|| !new Regex("^[a-zA-Z0-9_.-]*$").IsMatch(request.password)|| !(request.password.Length >= 6 && request.password.Length <= 30))
            {
                return null;
            }
            var user = new User();
            var hashedPw = new PasswordHasher<User>().HashPassword(user, request.password);
            user.passwordHash = hashedPw;
            user.email = request.email;
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return await CreateTokenResponse(user);
        }
        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.userID,request.RefreshToken);
            return user != null ? await CreateTokenResponse(user) : null;
        }
        public async Task<List<User>?> GetAllUsersAsync()
        {
            return await context.Users.ToListAsync();
        }
        //add token response later
        public async Task<User?> EditUserAsync(EditUserDto request, int pid)
        {
            if(!new EmailAddressAttribute().IsValid(request.email))
            {
                return null;
            }
            var user = await context.Users.FindAsync(pid);
            var patchDoc = new JsonPatchDocument<User>{ };
            patchDoc.Replace(user => user.email, request.email);
            patchDoc.ApplyTo(user);
            await context.SaveChangesAsync();
            return user;
        }
        public async Task<User?> EditUserAdminAsync(EditUserAdminDto request, int pid)
        {
            try
            {
                var user = await context.Users.FindAsync(pid);
                if (user == null || !new EmailAddressAttribute().IsValid(request.email) || !configuration["roles"].Split(',').ToList().Contains(request.role) || request.account_status.ToString().Length != 1)
                {
                    return null;
                }
                var patchDoc = new JsonPatchDocument<User> { };
                patchDoc.Replace(user => user.email, request.email);
                patchDoc.Replace(user => user.role, request.role);
                patchDoc.Replace(user => user.account_status, request.account_status);
                patchDoc.Replace(user => user.Megjegyzes, request.Megjegyzes);
                patchDoc.ApplyTo(user);
                await context.SaveChangesAsync();
                return user;
            }
            catch (NullReferenceException ex) 
            { 
                return null; 
            }
        }
        public async Task<User?> EditPasswordAsync(EditPasswordDto request, int pid)
        {
            var user = await context.Users.FindAsync(pid);
            if(!(request.password.Length>=6 && request.password.Length <= 30))
            {
                return null;
            }
            var hashedPw = new PasswordHasher<User>().HashPassword(user, request.password);
            var patchDoc = new JsonPatchDocument<User> { };
            patchDoc.Replace(user => user.passwordHash, hashedPw);
            patchDoc.ApplyTo(user);
            await context.SaveChangesAsync();
            return user;

        }
        private async Task<TokenResponseDto> CreateTokenResponse(User? user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }
        private async Task<User?> ValidateRefreshTokenAsync(int uid, string refToken)
        {
            var user = await context.Users.FindAsync(uid);
            if (user == null || user.refreshToken != refToken || user.refreshTokenExpiry <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        private string GenerateRefreshToken()
        {
            var ranNum = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(ranNum);
            return Convert.ToBase64String(ranNum);
        }
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var rToken = GenerateRefreshToken();
            user.refreshToken = rToken;
            user.refreshTokenExpiry = DateTime.UtcNow.AddHours(168);
            await context.SaveChangesAsync();
            return rToken;
        }
        private string CreateToken(User user)
        {
            var Claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.userID.ToString()),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Role, user.role.ToString())
            };
            var Key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["AppSettings:Token"]));
            var credentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["AppSettings:Issuer"],
                audience: configuration["AppSettings:Audience"],
                claims: Claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        //I have wasted a day on this shit and it never worked lmao
        /*public async Task<HttpResponseMessage?> OkResponseSetTokenCookie(TokenResponseDto request)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { };
            var jwtcookie = new CookieHeaderValue("JWTToken", request.AccessToken) { Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/", HttpOnly = true , Secure = true, Domain = "https://localhost:60769/" };
            var refcookie = new CookieHeaderValue("refreshToken", request.RefreshToken) { Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/refresh", HttpOnly = true, Domain = "https://localhost:60769/"};
            List<CookieHeaderValue> cookies = new List<CookieHeaderValue>{ jwtcookie, refcookie };
            response.Headers.AddCookies(cookies);
            return response;
        }*/
    }
}
