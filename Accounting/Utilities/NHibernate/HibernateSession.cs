using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Accounting.Models.Enums;
using NHibernate.Cfg;
using Accounting.Utilities.Productivity;
using NHibernate.SqlCommand;
using log4net;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using System.IO;
using NHibernate.Tool.hbm2ddl;
using System.Linq.Expressions;
using Mapping = NHibernate.Mapping;
using Accounting.Utilities.NHibernate;
using Accounting.Models;
//using Booking.App_Start;
//using Booking.NHibernate;
using Accounting.Models.UserModels;

namespace Accounting.Utilities {
	public static class NHSQL {
		public static string NHibernateSQL { get; set; }
		public static bool SaveCommands { get; set; }
	}
	public class NHSQLInterceptor : EmptyInterceptor, IInterceptor {
		protected static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		SqlString IInterceptor.OnPrepareStatement(SqlString sql) {
			NHSQL.NHibernateSQL = sql.ToString();
			if (NHSQL.SaveCommands) {
				//log.Info(NHSQL.NHibernateSQL);
			}

			return sql;
		}
	}
	public class HibernateSession {
		private static Env? CurrentEnv;
		private static object lck = new object();
		private static Dictionary<Env, ISessionFactory> factories;
		private static String DbFile = null;
		public static RuntimeNames Names { get; private set; }
		public static ISessionFactory GetDatabaseSessionFactory(Env? environmentOverride_testOnly = null) {
			Configuration c;
			var env = environmentOverride_testOnly ?? CurrentEnv ?? Config.GetEnv();
			CurrentEnv = env;
			if (factories == null)
				factories = new Dictionary<Env, ISessionFactory>();
			if (!factories.ContainsKey(env)) {
				lock (lck) {
					ChromeExtensionComms.SendCommand("dbStart");
					var config = System.Configuration.ConfigurationManager.AppSettings;
					var connectionStrings =  System.Configuration.ConfigurationManager.ConnectionStrings;

					switch (environmentOverride_testOnly ?? Config.GetEnv()) {
						case Env.local_mysql: {

								var connectionString = connectionStrings["DefaultConnectionMsSql"].ConnectionString;
								var file = connectionString.Split(new String[] { "Data Source=" }, StringSplitOptions.RemoveEmptyEntries)[0].Split(';')[0];
								DbFile = file;
								try {
									c = new Configuration();
									c.SetInterceptor(new NHSQLInterceptor());
								
									factories[env] = Fluently.Configure(c).Database(MsSqlConfiguration.MsSql2012.ConnectionString(connectionString))
									.Mappings(m => {
										m.FluentMappings.AddFromAssemblyOf<UserModel>();
									})
								   .CurrentSessionContext("web")
								   .ExposeConfiguration(x => BuildMsSqlSchema(x))
								   .BuildSessionFactory();
								} catch (Exception e) {
									throw e;
								}
								break;
							}
						case Env.test_server: {
								try {
									c = new Configuration();
									c.SetInterceptor(new NHSQLInterceptor());

									factories[env] = Fluently.Configure(c).Database(MsSqlConfiguration.MsSql2012.ConnectionString(connectionStrings["Test_Server"].ConnectionString)/*.ShowSql()*/)
									   .Mappings(m => {
										   m.FluentMappings.AddFromAssemblyOf<UserModel>();
									   }) 
									   .ExposeConfiguration(x => BuildMsSqlSchema(x))
									   .BuildSessionFactory();
								} catch (Exception e) {
									var mbox = e.Message;
									if (e.InnerException != null && e.InnerException.Message != null)
										mbox = e.InnerException.Message;

									ChromeExtensionComms.SendCommand("dbError", mbox);
									throw e;
								}
								break;
							}
						case Env.DefaultConnectionMsSql: {
								try {
									c = new Configuration();
									c.SetInterceptor(new NHSQLInterceptor());

									factories[env] = Fluently.Configure(c).Database(MsSqlConfiguration.MsSql2012.ConnectionString(connectionStrings["DefaultConnectionMsSql"].ConnectionString)/*.ShowSql()*/)
									   .Mappings(m => {
										   m.FluentMappings.AddFromAssemblyOf<UserModel>();
									   })
									   .ExposeConfiguration(x => BuildMsSqlSchema(x))
									   .BuildSessionFactory();
								} catch (Exception e) {
									var mbox = e.Message;
									if (e.InnerException != null && e.InnerException.Message != null)
										mbox = e.InnerException.Message;

									ChromeExtensionComms.SendCommand("dbError", mbox);
									throw e;
								}
								break;
							}

						default:
							throw new Exception("No database type");
					}
					Names = new RuntimeNames(c);
					ChromeExtensionComms.SendCommand("dbComplete");
				}
			}
			return factories[env];
		}
		private static void BuildMsSqlSchema(Configuration config, bool forceCreate = false) {
			// delete the existing db on each run
			var env = Config.GetEnv();
			var dir = Path.Combine(Path.GetTempPath(), "Booking");
			var DbFile = Path.Combine(dir, "dbversion" + env + ".txt");
			if (Config.ShouldUpdateDB() || forceCreate) {
				if (!File.Exists(DbFile) || forceCreate) {
					new SchemaExport(config).Create(false, true);
				} else {
					new SchemaUpdate(config).Execute(false, true);
				}
				Config.DbUpdateSuccessful();
			} 
		}
		public static bool CloseCurrentSession() {
			var session = (SingleRequestSession)HttpContext.Current.NotNull(x => x.Items["NHibernateSession"]);
			if (session != null) {
				if (session.IsOpen) {
					session.Close();

					}
				if (session.WasDisposed) {
					session.GetBackingSession().Dispose();
				}
				HttpContext.Current.Items.Remove("NHibernateSession");
				return true;
			}
			return false;
			
		}
		public static ISession GetCurrentSession(bool singleSession = true, Env? environmentOverride_TestOnly = null) {

			if (singleSession && !(HttpContext.Current == null || HttpContext.Current.Items == null) && HttpContext.Current.Items["IsTest"] == null) {
				try {
					var session = GetExistingSingleRequestSession();
					if (session == null) {
						session = new SingleRequestSession(GetDatabaseSessionFactory(environmentOverride_TestOnly).OpenSession()); // Create session, like SessionFactory.createSession()...
						HttpContext.Current.Items.Add("NHibernateSession", session);
					} else {
						session.AddContext();
					}
					return session;
				} catch (Exception e) {
					//Something went wrong.. revert
					var a = e;
				}
			}
			if (!(HttpContext.Current == null || HttpContext.Current.Items == null) && HttpContext.Current.Items["IsTest"] != null)
				return GetDatabaseSessionFactory(environmentOverride_TestOnly).OpenSession();
			if (singleSession == false)
				return GetDatabaseSessionFactory(environmentOverride_TestOnly).OpenSession();

			return new SingleRequestSession(GetDatabaseSessionFactory(environmentOverride_TestOnly).OpenSession(), true);
			
		}
		private static SingleRequestSession GetExistingSingleRequestSession() {
			if (!(HttpContext.Current == null || HttpContext.Current.Items == null) && HttpContext.Current.Items["IsTest"] == null) {
				try {
					var session = (SingleRequestSession)HttpContext.Current.Items["NHibernateSession"];
					return session;
				} catch (Exception) {
					//Something went wrong.. revert
					//var a = "Error";
				}
			}
			return null;
		}
		//public static async void SignInUser(UserModel user, bool remeberMe) {

			
		//	//CurrentUserSession.userSession = user.Id;

		//	if (remeberMe) {

		//		if (user.SecurityStamp == null) {
		//			user.SecurityStamp = Guid.NewGuid().ToString();
		//			NHibernateUserStore hs = new NHibernateUserStore();
		//			await hs.UpdateAsync(user);
		//		}
		//		CurrentUserSession.userSecurityStampCookie = user.SecurityStamp;
		//	} else {
		//		CurrentUserSession.removeSecurityStampCookie();
		//	}
		//}
		public class RuntimeNames {
			private Configuration cfg;

			public RuntimeNames(Configuration cfg) {
				this.cfg = cfg;
			}
	
			public string ColumnName<T>(Expression<Func<T, object>> property)
				where T : class, new() {
				var accessor = FluentNHibernate.Utils.Reflection
					.ReflectionHelper.GetAccessor(property);

				var names = accessor.Name.Split('.');
						
				var classMapping = cfg.GetClassMapping(typeof(T));

				return WalkPropertyChain(classMapping.GetProperty(names.First()), 0, names);
			}

			private string WalkPropertyChain(Mapping.Property property, int index, string[] names) {
				if (property.IsComposite)
					return WalkPropertyChain(((Mapping.Component)property.Value).GetProperty(names[++index]), index, names);

				return property.ColumnIterator.First().Text;
			}

			public string TableName<T>() where T : class, new() {
				return cfg.GetClassMapping(typeof(T)).Table.Name;
			}
		}
	}
}