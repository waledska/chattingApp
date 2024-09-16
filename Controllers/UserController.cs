using chattingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace chattingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public UserController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        // get userId
        [HttpGet("getContacts")]
        public async Task<IActionResult> getContacts()
        {
            var result = await _userService.getContactsForUserAsync();
            if (result == null)
                return BadRequest("some thing went wrong");

            return Ok(result);
        }

    }
}
