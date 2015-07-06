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

        /// <summary>
        /// Listet alle Konten des aktuellen Benutzers auf.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Index(string message = "", bool error = false, bool success = false)
        {
            var userId = _userManager.FindById(User.Identity.GetUserId()).Id;

            var model = new AccountOverviewViewModel
            {
                Accounts = _bank.Accounts.Where(account => account.OwnerId == userId).ToList(),
                Message = message,
                Error = error,
                Success = success
            };

            return View(model);
        }

        /// <summary>
        /// Erstellt ein neues Konto für den aktuell angemeldeten Benutzer.
        /// </summary>
        /// <returns></returns>
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
            return RedirectToAction("Index", new { message = "Das Konto Nummer " + accountNumber + "wurde erfolgreich angelegt.", success = true });
        }

        /// <summary>
        /// Zeigt den View zur Einzahlung an.
        /// </summary>
        /// <param name="account">Kontonummer</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult PayIn(int account)
        {
            return View(account);
        }

        /// <summary>
        /// Methode um die Einzahlung durchzuführen. Muss via POST aufgerufen werden!
        /// </summary>
        /// <param name="account">Kontonummer</param>
        /// <param name="amount">Betrag</param>
        /// <returns>Führt zur Index zurück</returns>
        [HttpPost]
        [Authorize]
        public RedirectToRouteResult PayIn(int account, double amount)
        {
            var myAccount = _bank.Accounts.FirstOrDefault(acc => acc.Number == account);
            if (myAccount == null)
                return RedirectToAction("Index", new { message = "Konto nicht gefunden", error = true });

            myAccount.PayIn(amount);
            _bank.SaveChanges();

            return RedirectToAction("Index", new { message = "Betrag erfolgreich eingezahlt.", success = true });
        }

        /// <summary>
        /// Zeigt den View zur Abhebung an.
        /// </summary>
        /// <param name="account">Kontonummer</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult Withdraw(int account)
        {
            return View(account);
        }

        /// <summary>
        /// Methode um die Abhebung durchzuführen. Muss via POST aufgerufen werden!
        /// </summary>
        /// <param name="account">Kontonummer</param>
        /// <param name="amount">Betrag</param>
        /// <returns>Führt zur Index zurück</returns>
        [HttpPost]
        [Authorize]
        public RedirectToRouteResult Withdraw(int account, double amount)
        {
            var myAccount = _bank.Accounts.FirstOrDefault(acc => acc.Number == account);
            if (myAccount != null)
            {
                if (!myAccount.Withdraw(amount))
                    return RedirectToAction("Index", new { message = "Die Deckung des Kontos reicht nicht aus.", error = true });
            }
            else
                return RedirectToAction("Index", new { message = "Konto nicht gefunden.", error = true });

            _bank.SaveChanges();
            return RedirectToAction("Index", new { message = "Betrag erfolgreich abgehoben.", success = true });
        }

        /// <summary>
        /// Zeigt den View zur Überweisung an.
        /// </summary>
        /// <param name="account">Kontonummer</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult Transfer(int account)
        {
            return View(account);
        }

        /// <summary>
        /// Methode um die Überweisung durchzuführen. Muss via POST aufgerufen werden!
        /// </summary>
        /// <param name="from">Zahlungspflichtiger</param>
        /// <param name="to">Zahlungsempfänger</param>
        /// <param name="amount">Betrag</param>
        /// <returns>Führt zur Index zurück</returns>
        [HttpPost]
        [Authorize]
        public RedirectToRouteResult Transfer(int from, int to, double amount)
        {
            var sender = _bank.Accounts.FirstOrDefault(acc => acc.Number == from);
            var receiver = _bank.Accounts.FirstOrDefault(acc => acc.Number == to);
            if (sender != null && receiver != null)
            {
                if (!sender.Transfer(amount, receiver))
                    return RedirectToAction("Index", new { message = "Die Deckung des Kontos reicht nicht aus.", error = true });
            }
            else
                return RedirectToAction("Index", new { message = "Konto nicht gefunden.", error = true });

            _bank.SaveChanges();
            return RedirectToAction("Index", new { message = "Betrag erfolgreich überwiesen.", success = true });
        }
    }
}