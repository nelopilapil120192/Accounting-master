using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounting.Models {
	public class CompanyModel {
		public virtual long Id { get; set; }
		public virtual long TruckerId { get; set; }
		public virtual string Name { get; set; }
		public virtual string Code { get; set; }
		public virtual string Token { get; set; }
		public virtual DateTime CreateTime { get; set; }
		public virtual DateTime? DeleteTime { get; set; }
		public CompanyModel() {
			CreateTime = DateTime.UtcNow;
		}
		public class CompanyModelMap : ClassMap<CompanyModel> {
			public CompanyModelMap() {
				Id(x => x.Id);
				Map(x => x.TruckerId).Not.LazyLoad();
				Map(x => x.Name).Not.LazyLoad();
				Map(x => x.Code).Length(3).Not.LazyLoad();
				Map(x => x.Token).Not.LazyLoad();
				Map(x => x.CreateTime);
				Map(x => x.DeleteTime);
			}
		}

	}
}