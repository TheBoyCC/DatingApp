using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using AutoMapper.QueryableExtensions;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DataContext _context;
    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper, IUnitOfWork unitOfWork, DataContext context)
    {
      _context = context;
      _unitOfWork = unitOfWork;
      _userManager = userManager;
      _signInManager = signInManager;
      _mapper = mapper;
      _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
      if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

      var user = _mapper.Map<AppUser>(registerDto);

      user.UserName = registerDto.Username.ToLower();

      var results = await _userManager.CreateAsync(user, registerDto.Password);

      if (!results.Succeeded) return BadRequest(results.Errors);

      var roleResult = await _userManager.AddToRoleAsync(user, "Member");

      if (!roleResult.Succeeded) return BadRequest(results.Errors);

      return new UserDto
      {
        Username = user.UserName,
        Token = await _tokenService.CreateToken(user),
        KnownAs = user.KnownAs,
        Gender = user.Gender
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      var user = await _userManager.Users
        .Include(p => p.Photos)
        .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

      if (user == null) return Unauthorized("Invalid username");

      var results = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

      if (!results.Succeeded) return Unauthorized();

      var unreadMessages = await _context.Messages.Where(m => m.DateRead == null && m.RecipientUsername == loginDto.Username)
        .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
        .ToListAsync();

      return new UserDto
      {
        Username = user.UserName,
        Token = await _tokenService.CreateToken(user),
        PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
        KnownAs = user.KnownAs,
        Gender = user.Gender,
        Messages = unreadMessages
      };
    }

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassowrd(ChangePasswordDto changePasswordDto)
    {
      var user = await _userManager.Users.SingleOrDefaultAsync(x => x.UserName == changePasswordDto.Username.ToLower());

      var passwordCompare = IsValidPassword(user, changePasswordDto.OldPassword, user.PasswordHash);

      if (passwordCompare != true) return BadRequest("Incorrect Old password");

      if (changePasswordDto.OldPassword == changePasswordDto.NewPassword) return BadRequest("New password cannot be same as old password");

      var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);

      if (!result.Succeeded) return BadRequest(result.Errors);

      return NoContent();
    }

    private async Task<bool> UserExists(string username)
    {
      return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
    private bool IsValidPassword(AppUser user, string password, string hash)
    {
      PasswordHasher<AppUser> hasher = new PasswordHasher<AppUser>();
      PasswordVerificationResult result = hasher.VerifyHashedPassword(user, hash, password);
      if (result == PasswordVerificationResult.Success) return true;

      return false;
    }
  }
}