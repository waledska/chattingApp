using chattingApp.Services;
using chattingApp.vModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace chattingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // get userId
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("getUserIdFromToken")]
        public async Task<IActionResult> getUserId()
        {
            return Ok(_authService.getUserId());
        }

        // Register
        [HttpPost("sendOtpToRegister")]
        public async Task<IActionResult> sendOtpToRegister([FromBody] sendOTPForLoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.sendOtpToConfirmPhoneAsync(model);

            //if (result != "")
            //    return BadRequest(result);
            //
            //return Ok("sms send succesfully!");

            ////////////////
            
            /// this is modified part!
            if(result.StartsWith("error, "))
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("registerUser")]
        public async Task<IActionResult> register([FromForm] userDataModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.registerAsync(model);

            if (result.Message != "")
                return BadRequest(result.Message);

            return Ok(result);
        }
        // login
        [HttpPost("sendOtpToLogin")]
        public async Task<IActionResult> sendOtpToLogin([FromBody] sendOTPForLoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.sendOTPToLoginAsync(model);

            //if (result != "")
            //    return BadRequest(result);
            //
            //return Ok("sms send succesfully!");

            ////////////////

            /// this is modified part!
            if (result.StartsWith("error, "))
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("getTokenForUser")]
        public async Task<IActionResult> getToken([FromBody] loginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.getTokenAsync(model);

            if (result.Message != "")
                return BadRequest(result.Message);

            return Ok(result);
        }
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogOutAsync();
            if (result == "Logged out successfully")
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}