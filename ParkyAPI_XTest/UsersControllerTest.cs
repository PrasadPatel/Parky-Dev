using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ParkyAPI;
using ParkyAPI.Controllers;
using ParkyAPI.Model;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace ParkyAPI_XTest
{
    public class UsersControllerTest
    {
        private readonly Mock<IUserRepository> _userMockRepo;
        private readonly UsersController _usersController;
        
        public UsersControllerTest()
        {
            _userMockRepo = new Mock<IUserRepository>();
            _usersController = new UsersController(_userMockRepo.Object);
        }

        [Fact]
        public void Authenticate_Returns_200()
        {
            //Arrange
            _userMockRepo.Setup(repo => repo.Authenticate(GetAuthenticationModel().Username, GetAuthenticationModel().Password)).
                  Returns(MockJwtTokens.GetUserWithJwtToken(GetUser()));
            //Act
            var result =_usersController.Authenticate(GetAuthenticationModel()) as OkObjectResult;
            //Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void Authenticate_Returns_BadRequest()
        {
            //Arrange
            User user = null;
            _userMockRepo.Setup(repo => repo.Authenticate(GetAuthenticationModel().Username, GetAuthenticationModel().Password)).
                Returns(user);
            //Act
            var result = _usersController.Authenticate(GetAuthenticationModel()) as BadRequestObjectResult;
            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Register_Returns_BadRequest_For_Uniqueness()
        {
            //Arrange
            _userMockRepo.Setup(repo => repo.IsUserUnique(GetUser().Username)).Returns(false);
            //Act
            var result = _usersController.Register(GetAuthenticationModel()) as BadRequestObjectResult;
            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public void Register_Returns_BadRequest_For_Save_Error()
        {
            //Arrange
            _userMockRepo.Setup(repo => repo.Register(GetAuthenticationModel().Username, GetAuthenticationModel().Password)).
                Returns(new User());
            //Act
            var result = _usersController.Register(GetAuthenticationModel()) as BadRequestObjectResult;
            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public void Register_Returns_200()
        {
            //Arrange
            _userMockRepo.Setup(repo => repo.IsUserUnique(GetAuthenticationModel().Username)).Returns(true);
            _userMockRepo.Setup(repo => repo.Register(GetAuthenticationModel().Username, GetAuthenticationModel().Password)).
                Returns(GetUser());
            //Act
            var result = _usersController.Register(GetAuthenticationModel()) as OkResult;
            //Assert
            Assert.IsType<OkResult>(result);
        }
        private static AuthenticationModel GetAuthenticationModel()
        {
            return new AuthenticationModel()
            {
                Username = "Patel",
                Password = "Patel"
            };
        }
        public static User GetUser()
        {
            return new User()
            {
                Id = 1,
                Username = "Patel",
                Password = "Patel",
                Role = "Admin",
                Token = ""
            };
        }
    }
    public static class MockJwtTokens
    {
        public static string Issuer { get; } = Guid.NewGuid().ToString();
        public static SecurityKey SecurityKey { get; }
        public static SigningCredentials SigningCredentials { get; }

        private static readonly JwtSecurityTokenHandler s_tokenHandler = new JwtSecurityTokenHandler();
        private static readonly byte[] s_key = Encoding.ASCII.GetBytes("This is secret key for authentication");
        private static readonly SecurityTokenDescriptor tokenDescriptor;

        static MockJwtTokens()
        {
            tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    //Claim type for Id
                    new Claim(ClaimTypes.Name, UsersControllerTest.GetUser().Id.ToString()),
                    //Claim type for Role
                    new Claim(ClaimTypes.Role, UsersControllerTest.GetUser().Role.ToString())
                }),
                Expires = DateTime.Now.AddMinutes(30),
                SigningCredentials = new SigningCredentials
                                         (new SymmetricSecurityKey(s_key), SecurityAlgorithms.HmacSha256Signature)
            };
        }

        public static User GetUserWithJwtToken(User user)
        {
            var token = s_tokenHandler.CreateToken(tokenDescriptor);
            user.Token = s_tokenHandler.WriteToken(token);
            user.Password = "";
            return user;
        }
    }
}
