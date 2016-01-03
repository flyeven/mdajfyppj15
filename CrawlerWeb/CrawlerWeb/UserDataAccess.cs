using CrawlerWeb;
using CrawlerWeb.Data;
using Newtonsoft.Json.Linq;
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


        public static SiteUser GetUser(int id) {
            using (var ctx = new CrawlerDataContext())
            {
                var result = ctx.SiteUsers.Where(u => u.id == id);
                if (result.Count() > 0) {
                    var user = result.First();
                    var load = user.UserDetail;
                    return user;
                }
            }
            return null;
        }

        public static SiteUser GetUserByUsername(string username)
        {
            SiteUser user = null;
            using (var ctx = new CrawlerDataContext())
            {
                try
                {
                    var result = (from u in ctx.SiteUsers
                                  where u.username.Equals(username) select u);
                    if (result.Count() > 0)
                    {
                        user = result.First();
                        var load = user.UserDetail;
                    }
                }
                catch (Exception e)
                {                    
                    throw e;
                }
            }

            return user;
        }


        public static string Validate(SiteUser user) {
            if (string.IsNullOrWhiteSpace(user.username))
                return "Invalid username";

            //if (string.IsNullOrWhiteSpace(user.password) || user.password.Contains(' ') || user.password.Length < 6)
                //return "Password must not contains space and it must be atleast 6 characters in length";

            if (string.IsNullOrWhiteSpace(user.email))
                return "Invalid email";

            if (string.IsNullOrWhiteSpace(user.fullname))
                return "Invalid fullname";


            return "ok";
        }

        public static int CreateUser(SiteUser user)
        {

           

            try
            {
                
                using (var ctx = new CrawlerDataContext())
                {
                    SiteUser newUser = ctx.SiteUsers.Add(user);
                    ctx.SaveChanges();
                    return newUser.id;
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

        public static JObject UserToObject(SiteUser user)
        {
            JObject o = new JObject();
            o["id"] = user.id;
            o["email"] = user.email;
            o["password"] = user.password;
            o["fullname"] = user.fullname;
            o["username"] = user.username;
            if (user.UserDetail != null)
            {
                JObject od = new JObject();
                od["Id"] = user.UserDetail.Id;
                od["Obtained"] = user.UserDetail.Obtained;
                od["Field"] = user.UserDetail.Field;
                od["Addr"] = user.UserDetail.Addr;
                od["Country"] = user.UserDetail.Country;
                od["City"] = user.UserDetail.City;
                od["Phone"] = user.UserDetail.Phone;
                od["Usertype"] = user.UserDetail.Usertype;
                od["Academiclevel"] = user.UserDetail.Academiclevel;
                o["UserDetail"] = od;
                o["UserDetailsId"] = user.UserDetail.Id;
            }

            return o;

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
                        oldUser.fullname = newUser.fullname;
                        if (oldUser.UserDetail == null) {
                            oldUser.UserDetail = new UserDetail();
                        }
                        oldUser.UserDetail.Academiclevel = newUser.UserDetail.Academiclevel;
                        oldUser.UserDetail.Addr = newUser.UserDetail.Addr;
                        oldUser.UserDetail.City = newUser.UserDetail.City;
                        oldUser.UserDetail.Country = newUser.UserDetail.Country;
                        oldUser.UserDetail.Field = newUser.UserDetail.Field;
                        oldUser.UserDetail.Phone = newUser.UserDetail.Phone;
                        oldUser.UserDetail.Obtained = newUser.UserDetail.Obtained;
                        oldUser.UserDetail.Usertype = newUser.UserDetail.Usertype;
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
