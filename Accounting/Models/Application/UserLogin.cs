using FluentNHibernate.Mapping;
using Microsoft.AspNet.Identity.EntityFramework;
using Accounting.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Accounting.Models {
	public class UserLogin : IdentityUserLogin, ILongIdentifiable {
		public virtual long Id { get; protected set; }

		public UserLogin() {

			CreateTime = DateTime.UtcNow;
		}

		public virtual DateTime CreateTime { get; set; }

	}
	public class UserLoginMap : ClassMap<UserLogin> {
		public UserLoginMap() {
			Id(x => x.Id);
			Map(x => x.ProviderKey);
			Map(x => x.LoginProvider);
			Map(x => x.CreateTime);

		}
	}
}

