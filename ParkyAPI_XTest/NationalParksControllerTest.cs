using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using Moq;
using ParkyAPI.Controllers;
using ParkyAPI.Data;
using ParkyAPI.Model;
using ParkyAPI.Model.Dtos;
using ParkyAPI.ParkyMapper;
using ParkyAPI.Repository;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using Xunit;

namespace ParkyAPI_XTest
{
    public class NationalParksControllerTest
    {
        private readonly Mock<INationalParkRepository> _mockNpRepo;
        private readonly IMapper _mapper;
        private readonly NationalParksController _nationalParksController;
        
        public NationalParksControllerTest()
        {
            _mockNpRepo = new Mock<INationalParkRepository>();
            _mapper  = new MapperConfiguration(c => c.AddProfile<ParkyMappings>()).CreateMapper();
            _nationalParksController = new NationalParksController(_mockNpRepo.Object, _mapper);
        }

        [Fact]
        public void GetNationalParks_Test()
        {
            //Arrange
            var objList = GetExpectedNationalParks();
            _mockNpRepo.Setup(repo => repo.GetNationalParks()).Returns(objList);
            //Act
            var result = _nationalParksController.GetNationalParks() as OkObjectResult;
            //Assert
            var nationalPark = Assert.IsType<List<NationalParkDto>>(result.Value);
            var objDto = new List<NationalParkDto>();
            foreach (var obj in nationalPark)
                objDto.Add(_mapper.Map<NationalParkDto>(obj));
            Assert.Equal(2, objDto.Count);
        }
        [Fact]
        public void GetNationalParkById_OkResult_Test()
        {
            //Arrange
            _mockNpRepo.Setup(repo => repo.GetNationalPark(1)).Returns(GetSampleNationalParkObject());
            //Act
            var okObjectResult = _nationalParksController.GetNationalPark(1) as OkObjectResult;
            // Assert
            var nationalPark = Assert.IsType<NationalParkDto>(okObjectResult.Value);
            Assert.IsType<OkObjectResult>(okObjectResult);
            Assert.IsType<NationalParkDto>(okObjectResult.Value);
        }
        [Fact]
        public void GetNationalParkById_NotFoundResult_Test()
        {
            //Arrange
            _mockNpRepo.Setup(repo => repo.GetNationalPark(2)).Returns(new NationalPark());
            //Act
            var okObjectResult = _nationalParksController.GetNationalPark(2) as OkObjectResult;
            //Assert
            var nationalPark = Assert.IsType<NationalParkDto>(okObjectResult.Value);
            if (nationalPark == null)
                Assert.IsType<NotFoundResult>(okObjectResult);
        }
        [Fact]
        public void CreateNationalPark_ReturnsBadRequest()
        {
            //Arrange
            NationalPark dtoNationalPark = null;
            _mockNpRepo.Setup(repo => repo.CreateNationalPark(dtoNationalPark)).Returns(false);
            //Act
            var badRequestResult = _nationalParksController.CreateNationalPark(
                _mapper.Map<NationalParkDto>(dtoNationalPark)) as BadRequestResult;
            //Assert
            Assert.IsType<BadRequestResult>(badRequestResult);
        }
        [Fact]
        public void CreateNationalPark_Returns201Created()
        {
            // Arrange
            NationalPark np = GetSampleNationalParkObject();
            _mockNpRepo.Setup(repo => repo.CreateNationalPark(It.IsAny<NationalPark>())).Returns(true);
            _nationalParksController.ControllerContext.HttpContext = HttpContextForVersioning();
            // Act
            var result = _nationalParksController.CreateNationalPark(_mapper.Map<NationalParkDto>(np)) as CreatedAtRouteResult; 
            // Assert
            _mockNpRepo.Verify(repo => repo.CreateNationalPark(It.IsAny<NationalPark>()), Times.Once);
            Assert.IsType<CreatedAtRouteResult>(result);
        }
        [Fact]
        public void CreateNationalPark_Returns404NotFound()
        {
            // Arrange
            NationalPark np = GetSampleNationalParkObject();
            _mockNpRepo.Setup(repo => repo.NationalParkExists(np.Name)).Returns(true);
            _nationalParksController.ControllerContext.HttpContext = HttpContextForVersioning();
            // Act
            var result = _nationalParksController.CreateNationalPark(_mapper.Map<NationalParkDto>(np));
            //Assert
            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            _mockNpRepo.Verify(x => x.CreateNationalPark(It.IsAny<NationalPark>()), Times.Never);
            Assert.Equal(404, objectResult.StatusCode);
        }
        [Fact]
        public void CreateNationalPark_Returns500InternalServerError()
        {
            // Arrange
            NationalPark np = GetSampleNationalParkObject();
            _mockNpRepo.Setup(repo => repo.CreateNationalPark(np)).Returns(false);
            _nationalParksController.ControllerContext.HttpContext = HttpContextForVersioning();
            //Act
            var result = _nationalParksController.CreateNationalPark(_mapper.Map<NationalParkDto>(GetSampleNationalParkObject()));
            //Assert
            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            _mockNpRepo.Verify(x => x.CreateNationalPark(It.IsAny<NationalPark>()));
            Assert.Equal(500, objectResult.StatusCode);
        }
        [Fact]
        public void UpdateNationalPark_ReturnsNoContent()
        {
            //Arrange
            _mockNpRepo.Setup(r => r.UpdateNationalPark(It.IsAny<NationalPark>())).Returns(true);
            NationalPark updatedNP = GetSampleNationalParkObjectWithKey();
            //Act
            var result = _nationalParksController.UpdateNationalPark(1, _mapper.Map<NationalParkDto>(updatedNP));
            //Assert
            _mockNpRepo.Verify(x => x.UpdateNationalPark(It.IsAny<NationalPark>()));
            Assert.IsType<NoContentResult>(result);
        }
        [Fact]
        public void DeleteNationalPark_ReturnNoContent()
        {
            //Arrange
            _mockNpRepo.Setup(repo => repo.NationalParkExists(1)).Returns(true);
            _mockNpRepo.Setup(repo => repo.GetNationalPark(1)).Returns(GetSampleNationalParkObjectWithKey());
            _mockNpRepo.Setup(repo => repo.DeleteNationalPark(It.IsAny<NationalPark>())).Returns(true);
            //Act
            var result = _nationalParksController.DeleteNationalPark(1);
            //Assert
            _mockNpRepo.Verify(x => x.DeleteNationalPark(It.IsAny<NationalPark>()));
            Assert.IsType<NoContentResult>(result);
        }
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
                Created = DateTime.Now,
                Established = DateTime.Now,
                Name = "TestNP1",
                Picture = null,
                State = "TestNP1State",
            };
        }
        private static NationalPark GetSampleNationalParkObjectWithKey()
        {
            return new NationalPark()
            {
                Id = 1,
                Created = DateTime.Now,
                Established = DateTime.Now,
                Name = "TestNP1",
                Picture = null,
                State = "TestNP1State",
            };
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
