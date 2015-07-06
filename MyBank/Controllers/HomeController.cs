using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MyBank.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyBank.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _bank { get; set; }

        private ApplicationUserManager _userManager { get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); } }

        public HomeController()
        {
            _bank = new ApplicationDbContext();
        }

        [Authorize]
        public ActionResult Index()
        {
            var userId = _userManager.FindById(User.Identity.GetUserId()).Id;
            var accounts = _bank.Accounts.Where(account => account.OwnerId == userId).ToList();
            return View(accounts);
        }
    }
}