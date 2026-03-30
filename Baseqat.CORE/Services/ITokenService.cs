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

        /// <summary>
        /// Revokes a JWT token by adding it to the blacklist.
        /// </summary>
        /// <param name="token">JWT token to revoke.</param>
        /// <param name="userId">User ID associated with the token.</param>
        /// <param name="revokedFrom">IP address or device identifier.</param>
        /// <returns>True if revoked successfully, false otherwise.</returns>
        Task<bool> RevokeTokenAsync(string token, string userId, string? revokedFrom = null);

        /// <summary>
        /// Checks if a JWT token has been revoked.
        /// </summary>
        /// <param name="token">JWT token to check.</param>
        /// <returns>True if token is revoked, false otherwise.</returns>
        Task<bool> IsTokenRevokedAsync(string token);

        /// <summary>
        /// Cleans up expired revoked tokens from the database.
        /// </summary>
        /// <returns>Number of tokens removed.</returns>
        Task<int> CleanupExpiredTokensAsync();

    }
}
