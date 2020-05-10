using Moq;
using ParkyAPI.Controllers;
using ParkyAPI.Model;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
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
        public void Index_ReturnsView()
        {
            _userMockRepo.Setup(repo => repo.Authenticate(GetAuthenticationModel().Username, GetAuthenticationModel().Password)).
                  Returns();
        }

        private static AuthenticationModel GetAuthenticationModel()
        {
            return new AuthenticationModel()
            {
                Username = "Patel",
                Password = "Patel"
            };
        }
        private static User GetUser()
        {
            return new User()
            {
                Id = 1,
                Username = "Patel",
                Password = "",
                Role = "Admin",
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlBhdGVsIiwiaWF0IjoxNTE2MjM5MDIyfQ.cbnfzG00mRfMNY47e9-OPOpFJ1DsP3m-pTSseSJXeLw"
            };
        }
    }
}
