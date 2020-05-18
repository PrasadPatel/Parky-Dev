using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Model;
using ParkyAPI.Repository;
using ParkyAPI.Repository.IRepository;

namespace ParkyAPI.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/Users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticationModel model)
        {
            var user = _userRepo.Authenticate(model.Username, model.Password);
            if (user == null)
            {
                return BadRequest(new { Message = "Username or password is incorrect"});
            }
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register([FromBody] AuthenticationModel model)
        {
            bool ifUserNameUnique = _userRepo.IsUserUnique(model.Username);
            if (!ifUserNameUnique)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            var user = _userRepo.Register(model.Username, model.Password);
            if (user == null)
            {
                return BadRequest(new { message = "Error while registering"});
            }

            return Ok();
        }
    }
}