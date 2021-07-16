using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Microsoft.Owin.Host.SystemWeb;

//[assembly: PreApplicationStartMethod(typeof(PreApplicationStart), "Initialize")]
[assembly:OwinStartup(typeof(Accounting.Startup))]
namespace Accounting {
	public class Startup {
		public void Configuration(IAppBuilder app) {
			app.UseCookieAuthentication(new CookieAuthenticationOptions {
				AuthenticationType = "ApplicationCookie",
				LoginPath = new PathString("/Home/Login")
			});
		}
	}
}