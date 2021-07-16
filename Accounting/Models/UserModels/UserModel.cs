
using System.Diagnostics;
using FluentNHibernate.Mapping;
using System;
using Microsoft.AspNet.Identity.EntityFramework;
using Accounting.Models.UserModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Accounting.Models {
	[DebuggerDisplay("{FirstName} {LastName}")]
	public class UserModel : IdentityUser {
		public virtual string FirstName { get; set; }
		public virtual string LastName { get; set; }
		public virtual string Password { get; set; }
		public virtual string Email { get { return UserName; } }
		public virtual bool RememberMe { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual String Name() {
			return ((FirstName ?? "").Trim() + " " + (LastName ?? "").Trim()).Trim();
		}
		public UserModel() {
			
			CreateTime = DateTime.UtcNow;
			IsActive = false;
		}
		public virtual string ErrorMessage { get; set; }
		public virtual DateTime CreateTime { get; set; }
		public virtual DateTime? DeleteTime { get; set; }
		public virtual string CreatedBy { get; set; }
		public virtual string Phone { get; set; }
		public virtual ICollection<UserRoleModel> Roles { get; set; }
		public virtual ICollection<UserLogin> Logins { get; set; }
		public virtual UserModel Supervisor { get; set; }
		public virtual CompanyModel Company { get; set; }




		public class UserModelMap : ClassMap<UserModel> {
			public UserModelMap() {
				Id(x => x.Id).CustomType(typeof(string)).GeneratedBy.Custom(typeof(GuidStringGenerator)).Length(36);
				Map(x => x.UserName).Index("UserName_IDX").Length(100).UniqueKey("uniq");
				Map(x => x.FirstName).Length(30).Not.LazyLoad();
				Map(x => x.LastName).Length(30).Not.LazyLoad();
				Map(x => x.PasswordHash);
				Map(x => x.DeleteTime);
				Map(x => x.CreateTime);
				Map(x => x.IsActive);
				Map(x => x.Phone).Length(15);
				Map(x => x.SecurityStamp).Index("SecurityStamp_IDX").Length(400).UniqueKey("uniq");
				HasMany(x => x.Roles).Not.LazyLoad().Cascade.SaveUpdate();
				HasMany(x => x.Logins).Cascade.SaveUpdate();
				References(x => x.Supervisor).Not.LazyLoad();
				References(x => x.Company).Not.LazyLoad();
			}
		}



		
	}

}
