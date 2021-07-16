using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounting.Models.Enums
{
	public enum Env
	{
		invalid,
		local_mysql,
		production,
		test_server,
		DefaultConnectionMsSql
	}

	
}