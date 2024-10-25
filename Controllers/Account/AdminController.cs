using BrainStormEra.Models;
using Microsoft.AspNetCore.Mvc;

namespace BrainStormEra.Controllers.Account
{
    public class AdminController : Controller
    {
        private readonly SwpMainFpContext _context;
        public AdminController(SwpMainFpContext context)
        {
            _context = context;
        }
        public IActionResult ManageCourses()
        {

            return View();
        }

        public IActionResult ManageUsers()
        {
            var users = _context.Accounts.ToList();
            return View(users);
        }

    }
}
