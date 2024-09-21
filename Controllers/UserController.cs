using chattingApp.Services;
using chattingApp.vModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace chattingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IAuthService authService, IUserService userService)
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
        [HttpDelete("deleteChat")]
        public async Task<IActionResult> DeleteChat(string deleteContactId)
        {
            if (string.IsNullOrEmpty(deleteContactId))
            {
                // Return 400 Bad Request if the contact ID is missing or invalid
                return BadRequest("Contact ID is required.");
            }

            // Call the delete chat service function
            var result = await _userService.deleteChatAsync(deleteContactId);

            if (result.isDeletedSuccessfully)
            {
                // Return 200 OK with success message if the deletion was successful
                return Ok("chat deleted successfully");
            }
            
            // Return 400 Bad Request if the contact does not exist
            return BadRequest(result.message);
        }
        [HttpPut("UpdateUserData")]
        public async Task<IActionResult> UpdateUserData([FromForm] userUpdateDataModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var result = await _userService.updateUserDataAsync(model);

            if (result != "")
                return BadRequest(result);

            return Ok("user data updated successfully");
        }
        [HttpGet("getUserData")]
        public async Task<IActionResult> getUserData(string userId)
        {
            var result = await _userService.getUserDataAsync(userId);
            if (result.message != "")
                return BadRequest(result.message);

            return Ok(result);
        }
        [HttpGet("getUsersByNameOrPhone")]
        public async Task<IActionResult> getUsersByNameOrPhone(string searchText)
        {
            var result = await _userService.getUsersByNameOrPhoneAsync(searchText);

            return Ok(result);
        }
    }
}
