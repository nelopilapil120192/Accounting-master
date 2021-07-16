using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Accounting.Exceptions;
using Accounting.Models;
using Accounting.Models.UserModels;
using Accounting.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Security.Claims;
using NHibernate.Criterion;

namespace Accounting.NHibernate {
	// Summary:
	//     Implements IUserStore using EntityFramework where TUser is the entity type
	//     of the user being stored
	//
	// Type parameters:
	//   TUser:


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

	public class NHibernateUserStore : IUserLoginStore<UserModel>, IUserClaimStore<UserModel>,
		IUserPasswordStore<UserModel>, IUserSecurityStampStore<UserModel>,
		IUserStore<UserModel>, IDisposable {
		public async Task<IList<UserModel>> GetAllUsers() {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					var users = db.QueryOver<UserModel>().Where(x => x.DeleteTime == null).List();
					return users;

				}
			}
		}
		
		public async Task CreateAsync(UserModel user) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					db.Save(user);
					tx.Commit();
					db.Flush();
				}
			}
		}

		public async Task DeleteAsync(UserModel user) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					user = db.Get<UserModel>(user);
					user.DeleteTime = DateTime.UtcNow;
					db.SaveOrUpdate(user);
					tx.Commit();
					db.Flush();
				}
			}
		}

		public async Task<UserModel> FindByIdAsync(string userId) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					return db.Get<UserModel>(userId);
				}
			}
		}

		public async Task<UserModel> FindUserById(string userId) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					return db.Get<UserModel>(userId);
				}
			}
		}

		public async Task<UserModel> FindByNameAsync(string userName) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					return db.QueryOver<UserModel>().Where(x => x.UserName == userName).SingleOrDefault();
				}
			}
		}

		public async Task<UserModel> FindByUserNamePassAsync(string userName, string password) {
			PasswordHasher ph = new PasswordHasher();

			var usr = await FindByNameAsync(userName);
			if (usr != null) {
				var result = ph.VerifyHashedPassword(usr.PasswordHash, password);
				if (result != PasswordVerificationResult.Success)
					usr = null;
			}
			return usr;
		}

		public async Task<UserModel> FindByStampAsync(string stamp) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					return db.QueryOver<UserModel>().Where(x => x.SecurityStamp == stamp).SingleOrDefault();
				}
			}
		}
		public async Task<CompanyModel> FindCompanyByToken(string Token) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					return db.QueryOver<CompanyModel>().Where(x => x.Token == Token).SingleOrDefault();
				}
			}
		}
		public async Task UpdateAsync(UserModel user) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					db.SaveOrUpdate(user);
					tx.Commit();
					db.Flush();
				}
			}
		}

		public void Dispose() {
			//TODO should this do anything?
		}

		public async Task<string> GetPasswordHashAsync(UserModel user) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					return db.Get<UserModel>(user.Id).PasswordHash;
				}
			}
		}

		public async Task<bool> HasPasswordAsync(UserModel user) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					return db.Get<UserModel>(user.Id).PasswordHash != null;
				}
			}
		}

		public async Task SetPasswordHashAsync(UserModel user, string passwordHash) {
			user.PasswordHash = passwordHash;
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					var foundUser = db.Get<UserModel>(user.Id);
					if (foundUser != null) {
						foundUser.PasswordHash = passwordHash;
						db.Update(foundUser);
						tx.Commit();
						db.Flush();
					}
					//user.PasswordHash = passwordHash;
				}
			}
			/*using (var db = HibernateSession.GetCurrentSession())
            {
                using (var tx = db.BeginTransaction())
                {
                    user = db.Get<UserModel>(user.Id);
                    user.PasswordHash = passwordHash;
                    db.SaveOrUpdate(user);
                    tx.Commit();
                    db.Flush();
                }
            }*/
		}
		public async Task SetPasswordAsync(UserModel user, string password) {
			var usr = await FindByIdAsync(user.Id);
			PasswordHasher ph = new PasswordHasher();
			var passHash = ph.HashPassword(password);
			await SetPasswordHashAsync(usr, passHash);
		}

		public async Task AddLoginAsync(UserModel user, UserLoginInfo login) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					user = db.Get<UserModel>(user.Id);
					user.Logins.Add(new UserLogin() {
						LoginProvider = login.LoginProvider,
						ProviderKey = login.ProviderKey,
						// User = user,
						UserId = user.Id
					});

					db.SaveOrUpdate(user);
					tx.Commit();
					db.Flush();
				}
			}
		}

		public async Task<UserModel> FindAsync(UserLoginInfo login) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					return db.QueryOver<UserModel>().Where(x => x.Logins.Any(y => y.ProviderKey == login.ProviderKey && login.LoginProvider == y.LoginProvider)).SingleOrDefault();
				}
			}
		}

		public async Task<IList<UserLoginInfo>> GetLoginsAsync(UserModel user) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					return  db.Get<UserModel>(user.Id).Logins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey)).ToList();
				}
			}
		}

		public async Task RemoveLoginAsync(UserModel user, UserLoginInfo login) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					var u = db.Get<UserModel>(user.Id);
					var toRemove = u.Logins.FirstOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
					if (toRemove == null)
						throw new PermissionsException("Login does not exist.");
					u.Logins.Remove(toRemove);
					db.SaveOrUpdate(u);
					tx.Commit();
					db.Flush();
				}
			}
		}

		public async Task AddToRoleAsync(UserModel user, UserRoleType role) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					//user = db.Get<UserModel>(user.Id);
					user.Roles.Add(new UserRoleModel() { RoleType = role});
					db.SaveOrUpdate(user);
					tx.Commit();
					db.Flush();
				}
			}
		}

		public async Task<IList<UserRoleType>> GetRolesAsync(UserModel user) {

			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					user = db.Get<UserModel>(user.Id);
					return user.Roles.NotNull(y => y.Where(x => x.DeleteTime==null).Select(x => x.RoleType).ToList());
				}
			}

		}

		public async Task<bool> IsInRoleAsync(UserModel user, UserRoleType role) {
			return user.Roles.NotNull(y => y.Any(x => x.RoleType == role && x.DeleteTime == null));
		}

		public async Task RemoveFromRoleAsync(UserModel user, UserRoleType role) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					user = db.Get<UserModel>(user.Id);
					var found = user.Roles.NotNull(y => y.ToList().FirstOrDefault(x => x.RoleType == role));
					if (found != null) {
						found.DeleteTime = DateTime.UtcNow;
						db.Delete(found);
						user.Roles.Remove(found);
					} else {
						throw new PermissionsException("Role could not be removed because it doesn't exist.");
					}
					//user.Roles.Remove(found);
					//db.Update(user);

					tx.Commit();
					db.Flush();
				}
			}
		}

		public async Task<string> GetSecurityStampAsync(UserModel user) {
			return user.SecurityStamp;
		}

		public async Task SetSecurityStampAsync(UserModel user, string stamp) {
			user.SecurityStamp = stamp;
		}

		public async Task AddClaimAsync(UserModel user, System.Security.Claims.Claim claim) {
			throw new NotImplementedException();
			/*using (var db = HibernateSession.GetCurrentSession())
            {
                using (var tx = db.BeginTransaction())
                {
                    user = db.Get<UserModel>(user.Id);
                    user.Claims.Add(claim);
                    db.SaveOrUpdate(user);
                    tx.Commit();
                    db.Flush();
                }
            }*/

		}

		public async Task<IList<Claim>> GetClaimsAsync(UserModel user) {
			return new List<Claim>();
			throw new NotImplementedException();
			/*using (var db = HibernateSession.GetCurrentSession())
            {
                using (var tx = db.BeginTransaction())
                {
                    user=db.Get<UserModel>(user.Id);
                    return user.Claims.Cast<Claim>().ToList();
                }
            }*/
		}

		public async Task RemoveClaimAsync(UserModel user, System.Security.Claims.Claim claim) {
			throw new NotImplementedException();
			/*using (var db = HibernateSession.GetCurrentSession())
            {
                using (var tx = db.BeginTransaction())
                {
                    user = db.Get<UserModel>(user.Id);
                    user.Claims.FirstOrDefault(x => x.Type == claim.Type && x.Value == claim.Value).NotNull(x => x.Deleted = true);
                    db.SaveOrUpdate(user);
                    tx.Commit();
                    db.Flush();
                }
            }*/
		}
		public async Task<IList<UserModel>> GetUsersAsync(string search) {
			using (var db = HibernateSession.GetCurrentSession()) {
				using (var tx = db.BeginTransaction()) {
					var Users = db.QueryOver<UserModel>().Where((Restrictions.On<UserModel>(x => x.UserName).IsLike("%" + search + "%")
						|| Restrictions.On<UserModel>(x => x.FirstName).IsLike("%" + search + "%")
						|| Restrictions.On<UserModel>(x => x.LastName).IsLike("%" + search + "%"))
						&& Restrictions.On<UserModel>(x => x.DeleteTime).IsNull
						).OrderBy(x => x.CreateTime).Desc();
					return Users.List();
				}

			}
		}


	}
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously