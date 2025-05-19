using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RMSProjectAPI.Database;
using RMSProjectAPI.Database.Entity;
using RMSProjectAPI.Model;
using RMSProjectAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace RMSProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public UserController(
            IConfiguration configuration,
            AppDbContext appDbContext,
            UserManager<User> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            SignInManager<User> signInManager)
        {
            _configuration = configuration;
            _appDbContext = appDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // ✅ General Register API
        [HttpPost("Register")]
        public async Task<ActionResult> Register(UserDto userDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUser == null)
            {
                var user = new User
                {
                    Email = userDto.Email,
                    UserName = userDto.Email,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    BirthDate = DateOnly.Parse(userDto.BirthDate),
                    Gender = !string.IsNullOrEmpty(userDto.Gender) ? userDto.Gender[0] : null,
                    Country = userDto.Country,
                    City = userDto.City,
                    Street = userDto.Street,
                    Status = userDto.Status,
                    Role = userDto.Role,
                    Description = userDto.Description,
                    CompanyName = userDto.CompanyName,
                    Companywebsite = userDto.Companywebsite,
                    PhoneNumber = userDto.PhoneNumber,
                    ProfilePicturePath = userDto.ProfilePicturePath,
                    UserIDPath = userDto.UserIDPath,
                    AcceptTerms = userDto.AcceptTerms,
                    UserType = (RMSProjectAPI.Database.Entity.UserType)userDto.UserType
                };
                var result = await _userManager.CreateAsync(user, userDto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
                
                // Assign role based on user type
                string role = userDto.UserType == (int)UserType.Marketer ? "marketer" : "customer";
                await _userManager.AddToRoleAsync(user, role);
            }
            else
            {
                return BadRequest("User already exists");
            }

            return Ok(userDto);
        }
        
        // ✅ Register Customer API
        [HttpPost("RegisterCustomer")]
        public async Task<ActionResult> RegisterCustomer(UserDto userDto)
        {
            // Set user type to Customer
            userDto.UserType = (int)UserType.Customer;
            
            // Ensure BirthDate is set
            if (userDto.BirthDate == default)
            {
                userDto.BirthDate = DateTime.Today.ToString("yyyy-MM-dd");
            }

            var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUser == null)
            {
                var user = new User
                {
                    Email = userDto.Email,
                    UserName = userDto.Email,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    BirthDate = DateOnly.Parse(userDto.BirthDate),
                    Gender = !string.IsNullOrEmpty(userDto.Gender) ? userDto.Gender[0] : null,
                    Country = userDto.Country,
                    City = userDto.City,
                    Street = userDto.Street,
                    Status = userDto.Status,
                    Role = userDto.Role,
                    Description = userDto.Description,
                    PhoneNumber = userDto.PhoneNumber,
                    ProfilePicturePath = userDto.ProfilePicturePath,
                    UserIDPath = userDto.UserIDPath,
                    AcceptTerms = userDto.AcceptTerms,
                    UserType = (RMSProjectAPI.Database.Entity.UserType)UserType.Customer,
                    CompanyName = string.Empty,
                    Companywebsite = null
                };
                var result = await _userManager.CreateAsync(user, userDto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
                
                // Assign customer role
                await _userManager.AddToRoleAsync(user, "customer");
            }
            else
            {
                return BadRequest("User already exists");
            }

            return Ok(userDto);
        }
        
        // ✅ Register Marketer API
        [HttpPost("RegisterMarketer")]
        public async Task<ActionResult> RegisterMarketer(UserDto userDto)
        {
            // Set user type to Marketer
            userDto.UserType = (int)UserType.Marketer;
            
            // Ensure BirthDate is set
            if (userDto.BirthDate == default)
            {
                userDto.BirthDate = DateTime.Today.ToString("yyyy-MM-dd");
            }

            var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUser == null)
            {
                var user = new User
                {
                    Email = userDto.Email,
                    UserName = userDto.Email,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    BirthDate = DateOnly.Parse(userDto.BirthDate),
                    Gender = !string.IsNullOrEmpty(userDto.Gender) ? userDto.Gender[0] : null,
                    Country = userDto.Country,
                    City = userDto.City,
                    Street = userDto.Street,
                    Status = userDto.Status,
                    Role = userDto.Role,
                    Description = userDto.Description,
                    CompanyName = userDto.CompanyName,
                    Companywebsite = userDto.Companywebsite,
                    PhoneNumber = userDto.PhoneNumber,
                    ProfilePicturePath = userDto.ProfilePicturePath,
                    UserIDPath = userDto.UserIDPath,
                    AcceptTerms = userDto.AcceptTerms,
                    UserType = (RMSProjectAPI.Database.Entity.UserType)UserType.Marketer
                };
                var result = await _userManager.CreateAsync(user, userDto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
                
                // Assign marketer role
                await _userManager.AddToRoleAsync(user, "marketer");
            }
            else
            {
                return BadRequest("User already exists");
            }

            return Ok(userDto);
        }

        // ✅ Login API
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = await GenerateJwtToken(user);
            return Ok(new { token });
        }

        // ✅ Add Role API
        [HttpPost("AddRole")]
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> AddRole([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Role name cannot be empty.");
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return BadRequest("Role already exists.");
            }

            var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            if (result.Succeeded)
            {
                return Ok($"Role '{roleName}' created successfully.");
            }

            return BadRequest("Failed to create role.");
        }

        // ✅ Get All Users
        [HttpGet("GetUsers")]
        //[Authorize]
        public ActionResult GetAllUsers()
        {
            return Ok(_appDbContext.Users);
        }

        // ✅ JWT Token Generator
        private async Task<string> GenerateJwtToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims = claims.Append(new Claim(ClaimTypes.Role, role)).ToArray();
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ Get User by ID
        [HttpGet("GetUser/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }

        // ✅ Update User Information
        [HttpPut("UpdateUser/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto updatedUser)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.BirthDate = DateOnly.Parse(updatedUser.BirthDate); // Fixed conversion
            user.Gender = !string.IsNullOrEmpty(updatedUser.Gender) ? updatedUser.Gender[0] : null;
            user.Country = updatedUser.Country;
            user.City = updatedUser.City;
            user.Street = updatedUser.Street;
            user.Status = updatedUser.Status;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User updated successfully.");
        }

        // ✅ Delete User
        [HttpDelete("DeleteUser/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User deleted successfully.");
        }

        // ✅ Change Password
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Password changed successfully.");
        }

        // New
        // ✅ Change Password
        [HttpPost("forgot-password")]
        //public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        public async Task<IActionResult> ForgotPassword()
        {
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            //var user = await _userManager.FindByEmailAsync(model.Email);
            //if (user == null)
            //    return Ok(new { message = "If the email exists, a reset link was sent." });

            //var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            //var encodedToken = WebUtility.UrlEncode(token);

            //var resetUrl = $"{_config["FrontendBaseUrl"]}/reset-password?email={user.Email}&token={encodedToken}";

            //await _emailSender.SendEmailAsync(user.Email, "Password Reset",
            //    $"Reset your password by clicking <a href='{resetUrl}'>here</a>.");

            //MailService.SendEmail();

            return Ok(new { message = "If the email exists, a reset link was sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { message = "Invalid email." });

            var decodedToken = WebUtility.UrlDecode(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password has been reset successfully." });
        }

        // New

        // ✅ Assign role to user
        [HttpPost("AssignRole")]
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.Role);
            if (!roleExists)
            {
                return BadRequest("Role does not exist.");
            }

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok($"Role '{model.Role}' assigned to {user.Email}.");
        }

        // ✅ Refresh token
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenDto tokenDto)
        {
            var user = await _userManager.FindByEmailAsync(tokenDto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var newToken = await GenerateJwtToken(user);
            return Ok(new { token = newToken });
        }
    }
}