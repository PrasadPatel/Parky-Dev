using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Moq;
using ParkyAPI.Controllers;
using ParkyAPI.Model;
using ParkyAPI.Model.Dtos;
using ParkyAPI.ParkyMapper;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;

namespace ParkyAPI_XTest
{
    public class TrailsControllerTest
    {
        private readonly Mock<ITrailRepository> _mockTrailRepo;
        private readonly IMapper _mapper;
        private readonly TrailsController _trailsController;

        public TrailsControllerTest()
        {
            _mockTrailRepo = new Mock<ITrailRepository>();
            _mapper = new MapperConfiguration(c => c.AddProfile<ParkyMappings>()).CreateMapper();
            _trailsController = new TrailsController(_mockTrailRepo.Object, _mapper);
        }
        [Fact]
        public void GetTrails_Returns_All_Trails()
        {
            _mockTrailRepo.Setup(repo => repo.GetTrails()).Returns(GetTrails());
            var result = _trailsController.GetTrails() as OkObjectResult;
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetTrail_By_Id_Returns_Trail()
        {
            _mockTrailRepo.Setup(repo => repo.GetTrail(1)).Returns(GetTrail());
            var result = _trailsController.GetTrail(1) as OkObjectResult;
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetTrail_By_Id_Returns_NotFound()
        {
            Trail trail = null;
            _mockTrailRepo.Setup(repo => repo.GetTrail(2)).Returns(trail);
            var result = _trailsController.GetTrail(2) as NotFoundResult;
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetTrailInNationalPark_Returns_TrailList()
        {
            _mockTrailRepo.Setup(repo => repo.GetTrailsInNationalPark(2)).Returns(GetTrails_For_NP_2());
            var result = _trailsController.GetTrailInNationalPark(2) as OkObjectResult;
            Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public void GetTrailInNationalPark_Returns_NotFound()
        {
            List<Trail> trails = null;
            _mockTrailRepo.Setup(repo => repo.GetTrailsInNationalPark(3)).Returns(trails);
            var result = _trailsController.GetTrailInNationalPark(3) as NotFoundResult;
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void CreateTrail_Returns_BadRequest()
        {
            _mockTrailRepo.Setup(repo => repo.CreateTrail(null)).Returns(false);
            var result = _trailsController.CreateTrail(null) as BadRequestResult;
            Assert.IsType<BadRequestResult>(result);
        }
        [Fact]
        public void CreateTrail_Returns_404()
        {
            _mockTrailRepo.Setup(repo => repo.TrailExists(GetTrail().Name)).Returns(true);
            _mockTrailRepo.Setup(repo => repo.CreateTrail(GetTrail())).Returns(false);
            var result = _trailsController.CreateTrail(_mapper.Map<TrailCreateDto>(GetTrail())) as ObjectResult;
            Assert.Equal(404, result.StatusCode);
        }
        [Fact]
        public void CreateTrail_Returns_500()
        {
            _mockTrailRepo.Setup(repo => repo.TrailExists(GetTrail().Name)).Returns(false);
            _mockTrailRepo.Setup(repo => repo.CreateTrail(GetTrail())).Returns(false);
            var result = _trailsController.CreateTrail(_mapper.Map<TrailCreateDto>(GetTrail())) as ObjectResult;
            Assert.Equal(500, result.StatusCode);
        }
        [Fact]
        public void CreateTrail_Returns_CreatedAtRoute()
        {
            _mockTrailRepo.Setup(repo => repo.TrailExists(GetTrail().Name)).Returns(false);
            _mockTrailRepo.Setup(repo => repo.CreateTrail(It.IsAny<Trail>())).Returns(true);
            _trailsController.ControllerContext.HttpContext = HttpContextForVersioning();
            var result = _trailsController.CreateTrail(_mapper.Map<TrailCreateDto>(GetTrail())) as CreatedAtRouteResult;
            Assert.IsType<CreatedAtRouteResult>(result);
        }
        [Fact]
        public void UpdateTrail_Returns_400()
        {
            TrailUpdateDto trailUpdateDto = null;
            var result = _trailsController.UpdateTrail(1,trailUpdateDto) as BadRequestResult;
            Assert.IsType<BadRequestResult>(result);
        }
        [Fact]
        public void UpdateTrail_Returns_500()
        {
            _mockTrailRepo.Setup(repo => repo.UpdateTrail(GetTrail())).Returns(false);
            var result = _trailsController.UpdateTrail(1, _mapper.Map<TrailUpdateDto>(GetTrail())) as ObjectResult;
            Assert.Equal(500, result.StatusCode);
        }
        [Fact]
        public void UpdateTrail_Returns_NoContent()
        {
            TrailUpdateDto trailUpdateDto = _mapper.Map<TrailUpdateDto>(GetTrail());
            trailUpdateDto.Difficulty = Trail.DifficultyType.Difficult;
            _mockTrailRepo.Setup(repo => repo.UpdateTrail(It.IsAny<Trail>())).Returns(true);
            var result = _trailsController.UpdateTrail(1, trailUpdateDto) as NoContentResult;
            Assert.IsType<NoContentResult>(result);
        }
        [Fact]
        public void DeleteTrail_Returns_404()
        {
            _mockTrailRepo.Setup(repo => repo.TrailExists(1)).Returns(false);
            var result = _trailsController.DeleteTrail(1) as NotFoundResult;
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void DeleteTrail_Returns_500()
        {
            _mockTrailRepo.Setup(repo => repo.TrailExists(1)).Returns(true);
            _mockTrailRepo.Setup(repo => repo.GetTrail(1)).Returns(GetTrail());
            _mockTrailRepo.Setup(repo => repo.DeleteTrail(GetTrail())).Returns(false);
            var result = _trailsController.DeleteTrail(1) as ObjectResult;
            Assert.Equal(500, result.StatusCode);
        }
        [Fact]
        public void DeleteTrail_Returns_NoContent()
        {
            _mockTrailRepo.Setup(repo => repo.TrailExists(1)).Returns(true);
            _mockTrailRepo.Setup(repo => repo.GetTrail(1)).Returns(GetTrail());
            _mockTrailRepo.Setup(repo => repo.DeleteTrail(It.IsAny<Trail>())).Returns(true);
            var result = _trailsController.DeleteTrail(1) as NoContentResult;
            Assert.IsType<NoContentResult>(result);
        }
        static List<Trail> GetTrails()
        {
            return new List<Trail>()
            {
                new Trail()
                {
                    Id=1, DateCreated=DateTime.Now, Difficulty= Trail.DifficultyType.Moderate, Distance=10.5f,
                    Elevation=10, Name="TestTrail_1" ,NationalParkId=1,
                    NationalPark = new NationalPark()
                    {
                        Id=1,Created= DateTime.Now,Established=DateTime.Now,Name="TestNP1",Picture=null,State="TestNP1State"
                    }
                },
                new Trail()
                {
                    Id=2, DateCreated=DateTime.Now, Difficulty= Trail.DifficultyType.Expert, Distance=10.5f,
                    Elevation=10, Name="TestTrail_2" ,NationalParkId=1,
                    NationalPark = new NationalPark()
                    {
                        Id=2,Created= DateTime.Now,Established=DateTime.Now,Name="TestNP2",Picture=null,State="TestNP2State"
                    }
                }
            };
        }
        static List<Trail> GetTrails_For_NP_2()
        {
            return new List<Trail>()
            {
                new Trail()
                {
                    Id=1, DateCreated=DateTime.Now, Difficulty= Trail.DifficultyType.Moderate, Distance=10.5f,
                    Elevation=10, Name="TestTrail_1" ,NationalParkId=1,
                    NationalPark = new NationalPark()
                    {
                        Id=1,Created= DateTime.Now,Established=DateTime.Now,Name="TestNP1",Picture=null,State="TestNP1State"
                    }
                },
                new Trail()
                {
                    Id=2, DateCreated=DateTime.Now, Difficulty= Trail.DifficultyType.Expert, Distance=10.5f,
                    Elevation=10, Name="TestTrail_2" ,NationalParkId=1,
                    NationalPark = new NationalPark()
                    {
                        Id=2,Created= DateTime.Now,Established=DateTime.Now,Name="TestNP2",Picture=null,State="TestNP2State"
                    }
                }
            };
        }
        static Trail GetTrail() 
        {
            return new Trail()
            {
                Id = 1,
                DateCreated = DateTime.Now,
                Difficulty = Trail.DifficultyType.Moderate,
                Distance = 10.5f,
                Elevation = 10,
                Name = "TestTrail_1",
                NationalParkId = 1,
                NationalPark = new NationalPark()
                {
                    Id = 1,
                    Created = DateTime.Now,
                    Established = DateTime.Now,
                    Name = "TestNP1",
                    Picture = null,
                    State = "TestNP1State"
                }
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
