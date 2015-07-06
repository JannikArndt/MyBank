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

        [Authorize]
        public RedirectToRouteResult Create()
        {
            // Falls es schon Konten gibt, finde die höchste Kontonummer von allen und addiere 1, sonst nimm 1:
            var accountNumber = _bank.Accounts.Any() ? _bank.Accounts.Max(account => account.Number) + 1 : 1;

            // Erstelle ein neues Konto mit der aktuellen UserId als Besitzer und der eben herausgefundenen Kontonummer:
            var newAccount = new BankAccount(User.Identity.GetUserId(), accountNumber);

            // Füge das Konto der Liste der Konten der Bank hinzu:
            _bank.Accounts.Add(newAccount);

            // Speichere die Änderungen in die Datenbank:
            _bank.SaveChanges();

            // Zeige die Übersichtsseite (Index) an:
            return RedirectToAction("Index");
        }
    }
}