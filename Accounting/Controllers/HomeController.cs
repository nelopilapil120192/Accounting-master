using Accounting.Accessors.Users;
using Accounting.Models;
using Accounting.NHibernate;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Accounting.Controllers
{
    public class HomeController : BaseController
    {
		// GET: Home
		public ActionResult Login() {
			//login Controller
			return View();
		}

		public ActionResult ChartOfAccnt() {
			//login Controller
			return View();
		}


		public ActionResult ChartOfAccnt2() {
			//login Controller
			return View();
		}

		public ActionResult ChartOfAccnt3() {
			//login Controller
			return View();
		}



		// GET: Home
		public ActionResult Index() {
			//login Controller
			return View();
		}
		public ActionResult Signup() {

			return View();
		}
		public async Task<ActionResult> SigningUp(UserModel signing) {
			NHibernateUserStore nhu = new NHibernateUserStore();
			PasswordHasher ph = new PasswordHasher();
			var passHash = ph.HashPassword(signing.Password);
			var newUser = new UserModel {
				UserName = signing.Email,
				FirstName = signing.FirstName,
				LastName = signing.LastName,
				Phone = signing.Phone,
				PasswordHash = passHash
			};
			await nhu.CreateAsync(newUser);
			return Redirect("Login");
		}
		[HttpPost]
		public async Task<ActionResult> Authorise(UserModel usr) {
			NHibernateUserStore nhu = new NHibernateUserStore();
			var user = await nhu.FindByUserNamePassAsync(usr.UserName, usr.Password);
			if (user != null && user.DeleteTime == null && user.IsActive) {
				var session = HttpContext.Session;
				var owinAuthentication = new OwinAuthenticationService(HttpContext);
				owinAuthentication.SignIn(user);
				return RedirectToAction("Index");
			} else {
				usr.ErrorMessage = "Invalid User Name or Password";
				return View("Login", usr);
			}
		}
	}
}