using Microsoft.AspNet.Identity;
using Accounting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Accounting.NHibernate
{
    public class NHibernateUserManager : UserManager<UserModel>, IDisposable
    {
        protected NHibernateUserStore NHibernateStore = null;

        public NHibernateUserManager(NHibernateUserStore store)
            : base(store)
        {
            NHibernateStore = store;
            this.UserValidator = new UserValidator<UserModel>(this)
            {
                AllowOnlyAlphanumericUserNames = false
            };
        }

        /*
        public async override Task<IdentityResult> CreateAsync(UserModel user, string password)
        {
            await this.Store.CreateAsync(user);
            await this.NHibernateStore.SetPasswordHashAsync(user, PasswordHasher.HashPassword(password));


            return new IdentityResult();
        }*/
    }
}