using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyGolfAPI.DTOs.Users;
using MyGolfAPI.Services.Auth;


namespace MyGolfAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        public UsersController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Get the current user's profile (auto-creates on first login).
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<UserReadDto>> GetMe(CancellationToken ct)
        {
            try
            {
                var user = await _currentUserService.GetMeAsync(User, ct);
                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Create or complete current user info.
        /// </summary>
        [HttpPost("me")]
        public async Task<ActionResult<UserReadDto>> CreateOrCompleteMe([FromBody] UserCreateDto dto, CancellationToken ct)
        {
            try
            {
                var user = await _currentUserService.CreateOrCompleteMeAsync(User, dto, ct);
                return Ok(user);

            }
            catch (ArgumentException ex)
            {
               return BadRequest(new { message = ex.Message });   
            }
        }

        /// <summary>
        /// Update current user info.
        /// </summary>
        [HttpPatch("me")]
        public async Task<ActionResult<UserReadDto>> UpdateMe([FromBody] UserUpdateDto dto, CancellationToken ct)
        {
            try
            {
                var user = await _currentUserService.UpdateMeAsync(User, dto, ct); 
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User profile not found." });
            }
        }

        /// <summary>
        /// Delete current user (Local DB row only).
        /// </summary>
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe(CancellationToken ct)
        {
            try
            {
                await _currentUserService.DeleteMeAsync(User, ct);
                return NoContent();

            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
}
