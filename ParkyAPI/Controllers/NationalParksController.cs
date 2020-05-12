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
using Microsoft.AspNetCore.Authorization;

namespace ParkyAPI.Controllers
{
    //Will dynamically load version this belongs to
    [Route("api/v{version:apiversion}/nationalparks")]
    //If version is not mentioned then will default to 1.0
    //[Route("api/[controller]")]
    [ApiController]
    //Commented for proper API versioning
    //[ApiExplorerSettings(GroupName = "ParkyOpenAPISpecNP")] //For bundling API documents
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class NationalParksController : ControllerBase
    {
        private INationalParkRepository _npRepo;
        private readonly IMapper _mapper;

        public NationalParksController(INationalParkRepository nationalParkRepository, IMapper mapper)
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
            var objList = _npRepo.GetNationalParks();

            var objDto = new List<NationalParkDto>();
            foreach (var obj in objList)
            {
                objDto.Add(_mapper.Map<NationalParkDto>(obj));
            }

            return Ok(objDto);
        }

        /// <summary>
        /// Get individual National Park
        /// </summary>
        /// <param name="nationalParkId">Id of National Park</param>
        /// <returns></returns>
        [HttpGet("{nationalParkId:int}", Name= "GetNationalPark")]
        [ProducesResponseType(200, Type = typeof(List<NationalParkDto>))]
        [ProducesResponseType(400)]
        [Authorize]
        [ProducesDefaultResponseType()]
        public IActionResult GetNationalPark(int nationalParkId)
        {
            var obj = _npRepo.GetNationalPark(nationalParkId);
            if (obj == null)
            {
                return NotFound();
            }

            var objDto = _mapper.Map<NationalParkDto>(obj);
            return Ok(objDto);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(NationalParkDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateNationalPark([FromBody] NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            if (_npRepo.NationalParkExists(nationalParkDto.Name))
            {
                ModelState.AddModelError("", "National Park Already Exists..!");
                return StatusCode(404, ModelState);
            }

            var nationalPark = _mapper.Map<NationalPark>(nationalParkDto);
            if (!_npRepo.CreateNationalPark(nationalPark))
            {
                ModelState.AddModelError("",$"Something went wrong while adding National Park {nationalPark.Name}");
                return StatusCode(500, ModelState);
            }

            //return Ok();
            //return CreatedAtRoute("GetNationalPark", new { nationalParkId = nationalPark.Id}, nationalPark);
            //Versioning fix
            return CreatedAtRoute("GetNationalPark", new { version = HttpContext.GetRequestedApiVersion().ToString(),
                nationalParkId = nationalPark.Id}, nationalPark);
        }

        [HttpPatch("{nationalParkId:int}", Name = "UpdateNationalPark")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateNationalPark(int nationalParkId, [FromBody] NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null || nationalParkId != nationalParkDto.Id)
            {
                return BadRequest();
            }

            var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);
            if (!_npRepo.UpdateNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong while updating National Park {nationalParkObj.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{nationalParkId:int}", Name = "DeleteNationalPark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteNationalPark(int nationalParkId)
        {
            if (!_npRepo.NationalParkExists(nationalParkId))
            {
                return NotFound();
            }

            var nationalParkObj = _npRepo.GetNationalPark(nationalParkId);
            if (!_npRepo.DeleteNationalPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong while updating National Park {nationalParkObj.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}