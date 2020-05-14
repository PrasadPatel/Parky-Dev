using Castle.Core.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using ParkyWeb;
using ParkyWeb.Controllers;
using ParkyWeb.Models;
using ParkyWeb.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ParkyWeb_XTest
{
    public class HomeControllerTest
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<INationalParkRepository> _mockNPRepo;
        private readonly Mock<ITrailRepository> _mockTrailRepo;
        private readonly Mock<IAccountRepository> _mockAccRepo;
        private readonly HomeController _homeController;

        public HomeControllerTest()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockNPRepo = new Mock<INationalParkRepository>();
            _mockTrailRepo = new Mock<ITrailRepository>();
            _mockAccRepo = new Mock<IAccountRepository>();

            _homeController = new HomeController(_mockLogger.Object, _mockNPRepo.Object, _mockTrailRepo.Object, _mockAccRepo.Object);
        }

        [Fact]
        public async Task Login_Returns_View()
        {
            User user = null;
            _mockAccRepo.Setup(repo => repo.LoginAsync(SD.AccountAPIPath + "authenticate/", GetUser())).
                Returns(Task.FromResult<User>(user));

            var result = await _homeController.Login(GetUser()) as ViewResult;
            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task Login_Redirects_To_Index()
        {
            //Arrange
            User user = GetUser();
            user.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlBhdGVsIiwiaWF0IjoxNTE2MjM5MDIyfQ.dxtJy4MronN9pH9omVIhZIBdq-55Kn8MCOoPX4gDha8";
            var httpContext = new DefaultHttpContext
            {
                // How mock RequestServices?
                RequestServices = MockServiceProvider().Object,
                Session = new MockHttpSession(),
            };
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _homeController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _homeController.TempData = tempData;
            _homeController.Url = MockUrlHelper("index").Object;
            _mockAccRepo.Setup(repo => repo.LoginAsync(SD.AccountAPIPath + "authenticate/", user)).
                Returns(Task.FromResult<User>(user));
            
            //Act
            var result = await _homeController.Login(user) as RedirectToActionResult;
            //Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", result.ActionName);
        }
        
        static User GetUser()
        {
            return new User() 
            { 
                Username = "Patel",
                Password = "Patel",
                Role = "Admin",
                Token = ""
            };
        }
        static ClaimsPrincipal GetPrincipal()
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, GetUser().Username));
            identity.AddClaim(new Claim(ClaimTypes.Role, GetUser().Role));
            var principle = new ClaimsPrincipal(identity);
            return principle;
        }
        private static Mock<IServiceProvider> MockServiceProvider()
        {
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.FromResult((object)null));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            return serviceProviderMock;
        }

        private static Mock<IUrlHelper> MockUrlHelper(string action)
        {
            var mockUrlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);
            Expression<Func<IUrlHelper, string>> urlSetup
                = url => url.Action(It.Is<UrlActionContext>(uac => uac.Action == action));
            mockUrlHelper.Setup(urlSetup).Returns("/"+ action).Verifiable();

            return mockUrlHelper;
        }
        static HttpContext HttpContextForVersioning()
        {
            var featureCollection = new Mock<IFeatureCollection>();
            var serviceProvider = new Mock<IServiceProvider>();
            var headers = new HeaderDictionary() { ["apiversion"] = "1.0" };
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();

            featureCollection.Setup(f => f.Get<IApiVersioningFeature>()).Returns(() => new ApiVersioningFeature(httpContext.Object));
            serviceProvider.Setup(sp => sp.GetService(typeof(IApiVersionReader))).Returns(new HeaderApiVersionReader("apiversion"));
            request.SetupGet(r => r.Headers).Returns(headers);
            httpContext.SetupGet(c => c.Features).Returns(featureCollection.Object);
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupProperty(c => c.RequestServices, serviceProvider.Object);
            return httpContext.Object;
        }
    }
}
