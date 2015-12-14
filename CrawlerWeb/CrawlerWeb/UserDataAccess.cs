using CrawlerWeb;
using CrawlerWeb.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerWeb
{
    public class UserDataAccess
    {
        private UserDataAccess() { }

        public static SiteUser GetUserByUsername(string username)
        {
            SiteUser user = null;
            using (var ctx = new CrawlerDataContext())
            {
                try
                {
                    var result = (from u in ctx.SiteUsers where u.username.Equals(username) select u);
                    if (result.Count() > 0)
                    {
                        user = result.First();
                    }
                }
                catch (Exception)
                {                    
                    return null;
                }
            }

            return user;
        }


        public static void CreateUser(SiteUser user)
        {

            try
            {
                
                using (var ctx = new CrawlerDataContext())
                {
                    ctx.SiteUsers.Add(user);
                    ctx.SaveChanges();
                }
            }
            catch (DbUpdateException dbe)
            {
                Exception ex = dbe.InnerException;
                if (ex != null)
                {
                    SqlException sqe = ex.InnerException as SqlException;
                    if (sqe != null && sqe.Number == 2627)
                    {
                        throw new CrawlerDataError("DuplicateUsernameOrEmailError");
                    }
                }
                throw new CrawlerDataError("DBError");
            }

        }

        public static bool UpdateUser(SiteUser newUser)
        {

            try
            {
                using (var ctx = new CrawlerDataContext())
                {
                    var result = from u in ctx.SiteUsers where u.username.Equals(newUser.username) select u;
                    SiteUser oldUser;
                    if (result.Count() > 0)
                    {
                        oldUser = result.First();
                        if (newUser.password != null && !newUser.password.Contains(" ") && newUser.password.Length > 5)
                        {
                            oldUser.password = newUser.password;
                        }
                        oldUser.email = newUser.email;
                        oldUser.fullname = oldUser.fullname;
                        ctx.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (DbUpdateException dbe)
            {
                Exception ex = dbe.InnerException;
                if (ex != null)
                {
                    SqlException sqe = ex.InnerException as SqlException;
                    if (sqe != null && sqe.Number == 2627)
                    {
                        throw new CrawlerDataError("DuplicateUsernameOrEmailError");
                    }
                }
                throw new CrawlerDataError("DBError");
            }

        }
    }
}
