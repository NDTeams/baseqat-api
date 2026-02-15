using Baseqat.EF.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.CORE.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="ApplicationUser">The identity user.</param>
        /// <returns>JWT token as string.</returns>
        Task<string> GenerateTokenAsync(ApplicationUser user);

        /// <summary>
        /// Validates the given JWT token and returns the user ID if valid, otherwise null.
        /// </summary>
        /// <param name="token">JWT token.</param>
        /// <returns>User ID or null.</returns>
        Task<string> ValidateTokenAsync(string token);

        /// <summary>
        /// Generates a new JWT token using a refresh token.
        /// </summary>
        /// <param name="token">Current JWT token.</param>
        /// <param name="refreshToken">Refresh token.</param>
        /// <returns>New JWT token as string.</returns>
        Task<string> RefreshTokenAsync(string token, string refreshToken);

        /// <summary>
        /// Gets the user ID from the JWT token.
        /// </summary>
        /// <param name="token">JWT token.</param>
        /// <returns>User ID or null.</returns>
        Task<string> GetUserIdByTokenAsync(string token);

        /// <summary>
        /// Generates an email payload token for the specified user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="identityToken">The identity token.</param>
        /// <returns>Email payload token as string.</returns>
        Task<string> GenerateEmailPayloadToken(string userId, string identityToken);

        /// <summary>
        /// Decodes an email payload token to extract user ID and identity token.
        /// </summary>
        /// <param name="token">Email payload token.</param>
        /// <returns>A tuple containing user ID and identity token, or null values if invalid.</returns>
        Task<(string? userId, string? identityToken)> DecodeEmailPayloadTokenAsync(string token);



    }
}
