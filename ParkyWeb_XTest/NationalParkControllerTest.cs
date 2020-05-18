using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using Moq;
using ParkyWeb;
using ParkyWeb.Controllers;
using ParkyWeb.Models;
using ParkyWeb.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ParkyWeb_XTest
{
    public class NationalParkControllerTest
    {
        private readonly Mock<INationalParkRepository> _mockNPRepo;
        private readonly NationalParkController _nationalParkController;

        public NationalParkControllerTest()
        {
            _mockNPRepo = new Mock<INationalParkRepository>();
            _nationalParkController = new NationalParkController(_mockNPRepo.Object);
        }
        [Fact]
        public void Index_Returns_View()
        {
            var result = _nationalParkController.Index() as ViewResult;
            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task GetAllNationalPark_Returns_NationalPark_List()
        {
            //Arrange
            var httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
            httpContext.Session.SetString("JWToken", _Token);
            _nationalParkController.ControllerContext = new ControllerContext() { HttpContext = httpContext };
            _mockNPRepo.Setup(_ =>_.GetAllAsync(SD.NationalParkAPIPath, _Token)).ReturnsAsync(GetExpectedNationalParks());
            //Act
            var result = await _nationalParkController.GetAllNationalPark() as JsonResult;
            //Assert
            Assert.IsType<JsonResult>(result);
        }
        [Fact]
        public async Task Upsert_Returns_View_With_0_NationalPark_When_ID_Null()
        {
            //Arrange
            Nullable<int> mockId = null;
            //Act
            var result = await _nationalParkController.Upsert(mockId) as ViewResult;
            //Assert
            Assert.IsType<ViewResult>(result);
            Assert.IsType<NationalPark>(result.Model);
        }
        [Fact]
        public async Task Upsert_Returns_NationalPark_For_Id()
        {
            //Arrange
            int mockId = 1;
            var httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
            httpContext.Session.SetString("JWToken", _Token);
            _nationalParkController.ControllerContext = new ControllerContext() { HttpContext = httpContext };
            _mockNPRepo.Setup(_ => _.GetAsync(SD.NationalParkAPIPath, mockId, _Token)).Returns(Task.FromResult<NationalPark>(GetSampleNationalParkObject()));
            //Act
            var result = await _nationalParkController.Upsert(mockId) as ViewResult;
            //Assert
            Assert.IsType<ViewResult>(result);
            Assert.IsType<NationalPark>(result.Model);
        }
        [Fact]
        public async Task Upsert_Returns_NotFound()
        {
            //Arrange
            int mockId = 1;
            var httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
            httpContext.Session.SetString("JWToken", _Token);
            _nationalParkController.ControllerContext = new ControllerContext() { HttpContext = httpContext };
            _mockNPRepo.Setup(_ => _.GetAsync(SD.NationalParkAPIPath, mockId, _Token)).Returns(Task.FromResult<NationalPark>(null));
            //Act
            var result = await _nationalParkController.Upsert(mockId) as NotFoundResult;
            //Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Upsert_Returns_View_For_Invalid_Model()
        {
            //Arrange
            _nationalParkController.ModelState.AddModelError("","Data not correct");
            //Act
            var result = await _nationalParkController.Upsert(GetSampleNationalParkObject()) as ViewResult;
            //Assert
            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task Upsert_For_Create_Returns_Redirect_Index()
        {
            //Arrange
            NationalPark nationalPark = GetSampleNationalParkObject();
            nationalPark.Id = 0; 
            var httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
            httpContext.Session.SetString("JWToken", _Token);
            httpContext.Request.Headers.Add("Content-Type", "multipart/form-data");
            var file = new FormFile(new MemoryStream(Resource.Bangalore_Mysore_Wayanad_Route_Map), 0, 0, "Data", "dummy.png");
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection { file });
            _nationalParkController.ControllerContext = new ControllerContext() { HttpContext = httpContext };
            _mockNPRepo.Setup(_=>_.CreateAsync(SD.NationalParkAPIPath, nationalPark, _Token)).Returns(Task.FromResult<bool>(true));
            //Act
            var result = await _nationalParkController.Upsert(nationalPark) as RedirectToActionResult;
            //Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", result.ActionName);
        }
        [Fact]
        public async Task Upsert_For_Update_Returns_Redirect_Index()
        {
            //Arrange
            NationalPark nationalPark = GetSampleNationalParkObjWithoutImage();
            var httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
            httpContext.Session.SetString("JWToken", _Token);
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection { });
            _mockNPRepo.Setup(_=>_.GetAsync(SD.NationalParkAPIPath, nationalPark.Id, _Token)).Returns(Task.FromResult<NationalPark>(GetSampleNationalParkObject()));
            _mockNPRepo.Setup(_ => _.UpdateAsync(SD.NationalParkAPIPath + nationalPark.Id, nationalPark, _Token)).Returns(Task.FromResult<bool>(true));
            _nationalParkController.ControllerContext = new ControllerContext() { HttpContext = httpContext };
            //Act
            var result = await _nationalParkController.Upsert(nationalPark) as RedirectToActionResult;
            //Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", result.ActionName);
        }
        [Fact]
        public async Task Delete_Returns_Unsuccessful()
        {
            //Arrange
            int mockId = 1;
            var httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
            httpContext.Session.SetString("JWToken", _Token);
            _nationalParkController.ControllerContext = new ControllerContext() { HttpContext = httpContext };
            _mockNPRepo.Setup(_=>_.DeleteAsync(SD.NationalParkAPIPath, mockId, _Token)).Returns(Task.FromResult<bool>(false));
            //Act
            var result = await _nationalParkController.Delete(mockId) as JsonResult;
            //Assert
            Assert.IsType<JsonResult>(result);
        }
        [Fact]
        public async Task Delete_Returns_Successful()
        {
            //Arrange
            int mockId = 1;
            var httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
            //httpContext.Session.SetString("JWToken", _Token);
            httpContext.Session.SetString("JWToken", "");
            _nationalParkController.ControllerContext = new ControllerContext() { HttpContext = httpContext };
            _mockNPRepo.Setup(_ => _.DeleteAsync(SD.NationalParkAPIPath, mockId, _Token)).Returns(Task.FromResult<bool>(true));
            //Act
            var result = await _nationalParkController.Delete(mockId) as JsonResult;
            //Assert
            Assert.True(AuthorizationTest.IsAuthorized(_nationalParkController, "Delete", new Type[] { typeof(int) }));
            Assert.IsType<JsonResult>(result);
        }

        private string _Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlBhdGVsIiwiaWF0IjoxNTE2MjM5MDIyfQ.dxtJy4MronN9pH9omVIhZIBdq-55Kn8MCOoPX4gDha8";

        private static List<NationalPark> GetExpectedNationalParks()
        {
            return new List<NationalPark>()
                {
                    new NationalPark(){ Id=1,Created= DateTime.Now,Established=DateTime.Now,Name="TestNP1",Picture=null,State="TestNP1State"},
                    new NationalPark(){ Id=2,Created= DateTime.Now,Established=DateTime.Now,Name="TestNP2",Picture=null,State="TestNP2State"},
                };
        }
        private static NationalPark GetSampleNationalParkObject()
        {
            return new NationalPark()
            {
                Id = 1,
                Created = DateTime.Now,
                Established = DateTime.Now,
                Name = "TestNP1",
                Picture = Resource.Bangalore_Mysore_Wayanad_Route_Map,
                State = "TestNP1State",
            };
        }
        private static NationalPark GetSampleNationalParkObjWithoutImage()
        {
            return new NationalPark()
            {
                Id = 1,
                Created = DateTime.Now,
                Established = DateTime.Now,
                Name = "TestNP2",
                Picture = null,
                State = "TestNP2State",
            };
        }
        static Mock<HttpContext> HttpContextForVersioning()
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
            return httpContext;
        }
        //Add the file in the underlying request object.
        private ControllerContext RequestWithFile()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Content-Type", "multipart/form-data");
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(Resource.Bangalore_Mysore_Wayanad_Route_Map , typeof(byte[]));

            var file = new FormFile(new MemoryStream(xByte), 0, 0, "Data", "dummy.png");
            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection { file });
            var actx = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
            return new ControllerContext(actx);
        }
    }
}
