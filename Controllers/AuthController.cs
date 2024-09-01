using chattingApp.Services;
using chattingApp.vModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> sendOtp([FromBody] sendOTPForLoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.phoneNumber = "+2" + model.phoneNumber;

            var result = await _authService.sendOtpToConfirmPhoneAsync(model);

            if (result != "")
                return BadRequest(result);

            return Ok("sms send succesfully!");
        }
        [HttpPost("registerUser")]
        public async Task<IActionResult> register([FromBody] userDataModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.registerAsync(model);

            if (result.Message != "")
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}
