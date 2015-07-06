using Perfectial.Infrastructure.Services.Interfaces;
using Perfectial.Infrastructure.Services.Services;
using Perfectial.Common;
using Perfectial.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Perfectial.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(UserDto userModel)
        {
            await _userService.SaveUserAsync(userModel);
            return View();
        }
    }
}