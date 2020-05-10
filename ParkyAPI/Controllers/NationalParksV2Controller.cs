using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Repository.IRepository;
using AutoMapper;
using ParkyAPI.Model.Dtos;
using ParkyAPI.Model;

namespace ParkyAPI.Controllers
{
    //Will dynamically load version this belongs to
    [Route("api/v{version:apiversion}/nationalparks")]
    [ApiVersion("2.0")]
    //[Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class NationalParksV2Controller : ControllerBase
    {
        private INationalParkRepository _npRepo;
        private readonly IMapper _mapper;

        public NationalParksV2Controller(INationalParkRepository nationalParkRepository, IMapper mapper)
        {
            _npRepo = nationalParkRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of National Parks
        /// </summary>
        /// <returns>List of National Parks</returns>
        [HttpGet]
        public IActionResult GetNationalParks()
        {
            var obj = _npRepo.GetNationalParks().FirstOrDefault();
            return Ok(_mapper.Map<NationalParkDto>(obj));
        }
    }
}