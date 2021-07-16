using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Web.ApplicationServices;
using Accounting.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Threading;

namespace Accounting.Accessors.Users {
	public class OwinAuthenticationService  {
		private readonly HttpContextBase _context;
		private const string AuthenticationType = "ApplicationCookie";
		public OwinAuthenticationService(HttpContextBase context) {
			_context = context;
		}
		public void SignIn(UserModel user) {
			IList<Claim> claims = new List<Claim> {
				new Claim(ClaimTypes.Sid,user.Id),
				new Claim(ClaimTypes.Name,user.UserName),
				new Claim(ClaimTypes.GivenName,user.FirstName),
				new Claim(ClaimTypes.Surname, user.LastName)
			};
			
			if (user.Roles.Count > 0  ) {
				
				foreach (var role in user.Roles.Where(x=>x.DeleteTime==null)) {
					if (role != null) {
						claims.Add(new Claim(ClaimTypes.Role, role.RoleType.ToString("g")));						
					}
				}
			
				
			}

			ClaimsIdentity identity = new ClaimsIdentity(claims, AuthenticationType);

			IOwinContext context = _context.Request.GetOwinContext();
			IAuthenticationManager authenticationManager = context.Authentication;
			var claimsPrincipal = new ClaimsPrincipal(identity);
			Thread.CurrentPrincipal = claimsPrincipal;
			authenticationManager.SignIn(identity);
		}
		public void SignOut() {
			IOwinContext context = _context.Request.GetOwinContext();
			IAuthenticationManager authenticationManager = context.Authentication;
			authenticationManager.SignOut(AuthenticationType);
		}

	}
}