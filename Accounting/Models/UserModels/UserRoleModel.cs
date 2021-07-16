using FluentNHibernate.Mapping;
using Accounting.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Accounting.Models.UserModels {


	public enum UserRoleType {
		Admin=0,
		Standard=1
	}
	public class UserRoleModel : ILongIdentifiable {
		public virtual long Id { get; set; }
		public virtual UserRoleType RoleType { get; set; }
		public virtual DateTime CreateTime { get; set; }
		public virtual DateTime? DeleteTime { get; set; }
		

		public UserRoleModel() {
			CreateTime = DateTime.UtcNow;
		}

		public class Map : ClassMap<UserRoleModel> {
			public Map() {
				Id(x => x.Id);
				Map(x => x.CreateTime);
				Map(x => x.DeleteTime);
				Map(x => x.RoleType).CustomType<UserRoleType>();
			}
		}
	}
}
