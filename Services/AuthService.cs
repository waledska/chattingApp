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
using chattingApp.DataAndContext;
using chattingApp.DataAndContext.ModelsForChattingApp;

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
        private readonly ApplicationDbContext _DbContext;
        private static readonly HashSet<string> _blacklistedTokens = new HashSet<string>();

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JWT> jwtOptions,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            ISMSService smsService,
            ITransferPhotosToPathWithStoreService transferPhotosToPath,
            ApplicationDbContext applicationDbContext
            )
        {
            _userManager = userManager;
            _jwt = jwtOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _smsService = smsService;
            _transferPhotosToPath = transferPhotosToPath;
            _DbContext = applicationDbContext;
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

            return await sendOtpToUserAsync(model.phoneNumber);
        }
        public async Task<registerResult> registerAsync(userDataModel model)
        {
            

            var user = await _userManager.Users.FirstOrDefaultAsync(n => n.UserName == model.name);
            if (user != null)
                return new registerResult{ Message = "error, this user name is already used for another user!" };

            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.phoneNumber);
            if (user != null)
                return new registerResult{ Message = "error, this phone number is already registered for another user!" };

            if (!await IsValidOtpForUserAsync(new VerificationOtp { OTP = model.OTPforPhoneConfirmaiton, phoneNumber = model.phoneNumber }))
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
                UserName = model.name,
                Email = ""
            };


            // storing the new user 
            var result = await _userManager.CreateAsync(newUser);
            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new registerResult { Message = errors };
            }

            
            // select the user with his full data
            var newUserFullData = await _userManager.Users.FirstOrDefaultAsync(p => p.PhoneNumber == model.phoneNumber);


            // creating JWT Token
            var jwtSecurityToken = await CreateJwtTokenAsync(newUserFullData);

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
            // check if this user data for real user in the db

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == model.userName);
            if (user == null)
                return "error, your data is not correct!";
            if (user.PhoneNumber != model.phoneNumber)
                return "error, your data is not correct!";

            return await sendOtpToUserAsync(model.phoneNumber);
        }
        public async Task<registerResult> getTokenAsync(loginModel model)
        {
            
            // checking the user data user name and otp(password)
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return new registerResult { Message = "error, your data is not correct!" };

            var phoneOtp = await _DbContext.PhoneOtps.FirstOrDefaultAsync(x => x.PhoneNumber == user.PhoneNumber);
            if (phoneOtp == null)
                return new registerResult { Message = "error,  your data is not correct!" };
            if(user == null)
                return new registerResult { Message = "error,  your data is not correct!" };
            if (!await IsValidOtpForUserAsync(new VerificationOtp { OTP = model.Otp, phoneNumber = user.PhoneNumber}))
                return new registerResult { Message = "error,  your data is not correct" };

            // generating the token for this user by jwt
            var jwtToken = await CreateJwtTokenAsync(user);

            // creating the result object 
            var result = new registerResult
            {
                name = user.UserName,
                phoneNumber = user.PhoneNumber,
                imgUrl = user.imgURL,
                token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                ExpiresOn = jwtToken.ValidTo,
                Message = ""
            };
            return result;
        }

        // logout
        public async Task<string> LogOutAsync()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            BlacklistToken(token);
            return "Logged out successfully";
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
        // helping for logout by put generated token in black list
        public void BlacklistToken(string token)
        {
            lock (_blacklistedTokens)
            {
                _blacklistedTokens.Add(token);
            }
        }

        public bool IsTokenBlacklisted(string token)
        {
            lock (_blacklistedTokens)
            {
                return _blacklistedTokens.Contains(token);
            }
        }

        private string generateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task<string> sendOtpToUserAsync(string userPhone)
        {
            if (userPhone is null || userPhone == "")
                return "error, phone number for user can't be empty!";
            var otp = generateOtp();
            if (otp is null || otp == "")
                return "error, some thing went wrong please try again!";

            // your logic for storing otps in sql in table phoneOtps
            var newPhoneOtp = new phoneOtp
            {
                otp = otp,
                PhoneNumber = userPhone,
                validTo = DateTime.Now.AddMinutes(5),
            };

            var oldOtp = await _DbContext.PhoneOtps.FirstOrDefaultAsync(x => x.PhoneNumber == userPhone);
            if (oldOtp == null)
            {
                // Asynchronously add new OTP entry if no existing one is found
                await _DbContext.PhoneOtps.AddAsync(newPhoneOtp);
            }
            else
            {
                // Update only the necessary fields
                oldOtp.otp = newPhoneOtp.otp;
                oldOtp.validTo = newPhoneOtp.validTo;
            }

            // Asynchronously save changes to the database
            await _DbContext.SaveChangesAsync();

            // send OTP to user in sms message
            //var responseFromTwilio = _smsService.Send( "+2" + userPhone, $"Your OTP for chatting app is: {otp}");
            //if(!string.IsNullOrEmpty(responseFromTwilio.ErrorMessage))
            //    return "error, from sending twilio.... " + responseFromTwilio.ErrorMessage;

            //return "";
            return otp; /////////////////// this modification only and above!!!
        }
        
        private async Task<bool> IsValidOtpForUserAsync(VerificationOtp request)
        {
            var otpForThisPhone = await _DbContext.PhoneOtps.FirstOrDefaultAsync(x => x.PhoneNumber == request.phoneNumber);

            if ( (otpForThisPhone is not null) && (otpForThisPhone.validTo > DateTime.Now) && (otpForThisPhone.otp == request.OTP))
            {
                otpForThisPhone.validTo = DateTime.Now;
                await _DbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

    }
}
