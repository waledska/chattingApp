using chattingApp.DataAndContext.Models;
using chattingApp.Helpers;
using chattingApp.vModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace chattingApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWT _jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthService(
            UserManager<ApplicationUser> userManager,
            JWT jwt,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _jwt = jwt;
            _httpContextAccessor = httpContextAccessor;
        }

        // 2 function to get user Id GetUserIdFromToken(helper), getUserId(main)
        public string getUserId()
        {
            // logic
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HTTP context is not available.");
            }// Extract the Authorization header
            var authorizationHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new UnauthorizedAccessException("Authorization header is missing or invalid.");
            }
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            // Extract the user ID from the token
            return GetUserIdFromToken(token); // Reuse the previously defined methodreturn userId;
        }
        // helping function
        private string GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required.", nameof(token));

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "uid");

            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }
            else
            {
                throw new InvalidOperationException("User ID claim ('uid') not found in token.");
            }
        }
        // register
        public Task<string> sendOtpToConfirmPhoneAsync(sendOTPForLoginModel model)
        {
            throw new NotImplementedException();
        }
        public Task<registerResult> registerAsync(userDataModel model)
        {
            throw new NotImplementedException();
        }
        // login
        public Task<string> sendOTPToLoginAsync(sendOTPForLoginModel model)
        {
            throw new NotImplementedException();
        }
        public Task<registerResult> getTRokenAsync(loginModel model)
        {
            throw new NotImplementedException();
        }

        // logout
        public Task<string> logOutAsync()
        {
            throw new NotImplementedException();
        }
    }
}
