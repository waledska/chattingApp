using chattingApp.DataAndContext.Models;
using chattingApp.Helpers;
using chattingApp.vModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Twilio.Http;
using Microsoft.Extensions.Caching.Memory;
using Twilio.Types;
using Microsoft.EntityFrameworkCore;

namespace chattingApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWT _jwt;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly ISMSService _smsService;
        private readonly ITransferPhotosToPathWithStoreService _transferPhotosToPath;
        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JWT> jwtOptions,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            ISMSService smsService,
            ITransferPhotosToPathWithStoreService transferPhotosToPath
            )
        {
            _userManager = userManager;
            _jwt = jwtOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _smsService = smsService;
            _transferPhotosToPath = transferPhotosToPath;
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
        public async Task<string> sendOtpToConfirmPhoneAsync(sendOTPForLoginModel model)
        {

            var user = await _userManager.Users.FirstOrDefaultAsync(n => n.UserName == model.userName);
            if (user != null)
                return "error, this user name is already used for another user!";

            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.phoneNumber);
            if (user != null)
                return "error, this phone number is already assigned to another user!";

            return sendOtpToUserAsync(model.phoneNumber);
        }
        public async Task<registerResult> registerAsync(userDataModel model)
        {
            

            var user = await _userManager.Users.FirstOrDefaultAsync(n => n.UserName == model.name);
            if (user != null)
                return new registerResult{ Message = "error, this user name is already used for another user!" };

            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.phoneNumber);
            if (user != null)
                return new registerResult{ Message = "error, this phone number is already registered for another user!" };

            if (!IsValidOtpForUser(new VerificationOtp { OTP = model.OTPforPhoneConfirmaiton, phoneNumber = model.phoneNumber }))
                return new registerResult { Message = "error, OTP is not correct" };

            var storingImgResult = _transferPhotosToPath.GetPhotoPath(model.img);

            if(storingImgResult.StartsWith("error, "))
                return new registerResult { Message = storingImgResult };

            ApplicationUser newUser = new ApplicationUser
            {
                imgURL = storingImgResult,
                lastOnlineTime = DateTime.Now,
                PhoneNumber = model.phoneNumber,
                PhoneNumberConfirmed = true,
                UserName = model.name

            };

            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new registerResult { Message = errors };
            }

            var jwtSecurityToken = await CreateJwtTokenAsync(newUser);

            registerResult resultModel = new registerResult
            {
                name = model.name,
                phoneNumber = model.phoneNumber,
                imgUrl = storingImgResult,
                Message = "",
                token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpiresOn = jwtSecurityToken.ValidTo
            };

            return resultModel;
        }
        // login
        public async Task<string> sendOTPToLoginAsync(sendOTPForLoginModel model)
        {
            throw new NotImplementedException();
        }
        public async Task<registerResult> getTRokenAsync(loginModel model)
        {
            throw new NotImplementedException();
        }

        // logout
        public Task<string> logOutAsync()
        {
            throw new NotImplementedException();
        }
        // more helping functions

        // for creating Token by JWT
        private async Task<JwtSecurityToken> CreateJwtTokenAsync(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        private string generateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string sendOtpToUserAsync(string userPhone)
        {
            if (userPhone is null || userPhone == "")
                return "error, phone number for user can't be empty!";
            var otp = generateOtp();
            if (otp is null || otp == "")
                return "error, some thing went wrong please try again!";


            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)); // OTP expires in 5 minutes

            // store the OTP in the cash memory
            _memoryCache.Set(userPhone, otp, cacheEntryOptions);

            // send OTP to user in sms message
            var responseFromTwilio = _smsService.Send(userPhone, $"Your OTP for chatting app is: {otp}");
            if(!string.IsNullOrEmpty(responseFromTwilio.ErrorMessage))
                return "error, from sending twilio.... " + responseFromTwilio.ErrorMessage;

            return "";
        }
        
        private bool IsValidOtpForUser(VerificationOtp request)
        {

            if (_memoryCache.TryGetValue(request.phoneNumber, out string cachedOtp) && cachedOtp == request.OTP)
            {
                // otp is right for this phone number
                _memoryCache.Remove(request.phoneNumber); // Remove OTP from cache after successful verification
                return true;
            }
            return false;
        }
    }
}
