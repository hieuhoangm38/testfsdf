using Microsoft.EntityFrameworkCore;
using System.Net;
using WebApi.Helpers;
using WebApplication1.Authorization;
using WebApplication1.Entities;
using WebApplication1.Helpers;
using WebApplication1.Models.User;


namespace WebApplication1.Services
{

    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest authenticateRequest, string ipAddress);
        User GetById(int id);
    }
    public class UserService : IUserService
    {

        private UserDbContext _context;
        private IJwtUtils _jwtUtils;
        private readonly AppSettings _appSettings;

        public UserService(UserDbContext context, IJwtUtils jwtUtils, AppSettings appSettings)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _appSettings = appSettings;
        }

        public User GetById(int id)
        {
            var user = _context.Find<User>(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
            
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest authenticateRequest, string ipAddress)
        {
            User? user = _context.Users.SingleOrDefault(x => x.UserName == authenticateRequest.UserName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(authenticateRequest.Password, user.PasswordHash))
                throw new AppException("Username or password is incorrect");

            string jwtToken = _jwtUtils.GenerateJwtToken(user);
            RefreshToken refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(refreshToken);
            removeOldRefreshTokens(user);

            // save changes to db
            _context.Update(user);
            _context.SaveChanges();

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        private void removeOldRefreshTokens(User user)
        {
            // remove old inactive refresh tokens from user based on TTL in app settings
            user.RefreshTokens.RemoveAll(x =>
                !x.IsActive &&
                x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
        }
    }
}
