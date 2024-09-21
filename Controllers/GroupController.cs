using chattingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace chattingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class GroupController : ControllerBase
    {
        private readonly IUserService _userService;

        public GroupController(IAuthService authService, IUserService userService)
        {
            _userService = userService;
        }

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
