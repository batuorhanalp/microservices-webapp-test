using Microsoft.AspNetCore.Mvc;
using WebApp.Common.Entities;
using WebApp.Common.Interfaces;

namespace WebApp.UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService, 
        IUserRepository userRepository,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("by-username/{username}")]
    public async Task<ActionResult<User>> GetUserByUsername(string username)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by username {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<User>> GetUserByEmail(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(
                request.Email, 
                request.Username, 
                request.DisplayName);
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating user");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}/profile")]
    public async Task<ActionResult<User>> UpdateUserProfile(Guid id, [FromBody] UpdateProfileRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserProfileAsync(id, request.DisplayName, request.Bio);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating user profile");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{followerId}/follow/{followeeId}")]
    public async Task<IActionResult> FollowUser(Guid followerId, Guid followeeId)
    {
        try
        {
            await _userService.FollowUserAsync(followerId, followeeId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for following user");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating follow relationship: {FollowerId} -> {FolloweeId}", followerId, followeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{followerId}/unfollow/{followeeId}")]
    public async Task<IActionResult> UnfollowUser(Guid followerId, Guid followeeId)
    {
        try
        {
            await _userService.UnfollowUserAsync(followerId, followeeId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for unfollowing user");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing follow relationship: {FollowerId} -> {FolloweeId}", followerId, followeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{userId}/followers")]
    public async Task<ActionResult<IEnumerable<User>>> GetFollowers(Guid userId)
    {
        try
        {
            var followers = await _userRepository.GetFollowersAsync(userId);
            return Ok(followers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving followers for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{userId}/following")]
    public async Task<ActionResult<IEnumerable<User>>> GetFollowing(Guid userId)
    {
        try
        {
            var following = await _userRepository.GetFollowingAsync(userId);
            return Ok(following);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving following for user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<User>>> SearchUsers(
        [FromQuery] string searchTerm,
        [FromQuery] int limit = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term is required");

            var users = await _userService.SearchUsersAsync(searchTerm, limit);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with term {SearchTerm}", searchTerm);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("check-email/{email}")]
    public async Task<ActionResult<bool>> CheckEmailAvailability(string email)
    {
        try
        {
            var isAvailable = !await _userRepository.IsEmailTakenAsync(email);
            return Ok(new { available = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("check-username/{username}")]
    public async Task<ActionResult<bool>> CheckUsernameAvailability(string username)
    {
        try
        {
            var isAvailable = !await _userRepository.IsUsernameTakenAsync(username);
            return Ok(new { available = isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability {Username}", username);
            return StatusCode(500, "Internal server error");
        }
    }
}

// DTOs for requests
public record CreateUserRequest(string Email, string Username, string DisplayName);
public record UpdateProfileRequest(string DisplayName, string? Bio);
