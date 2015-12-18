using System;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using CrawlerWeb;
using System.Web.Http.Cors;
using CrawlerWeb.Data;

namespace CrawlerWeb.Controllers
{


    

    public class UserController : ApiController
    {

        //hides password before sending it in response
        bool hidePassword = true;

        [Route("api/login")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject login([FromUri(Name = "username", SuppressPrefixCheck = false)] string username = "",
                           [FromUri(Name = "password", SuppressPrefixCheck = false)] string password = "")
        {

            

            JObject response = new JObject();
            SiteUser user = null;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                
                response["authenticated"] = false;
                response["message"] = "Username and Password can not be empty";
                response["user"] = null;
                return response;
            }
            try
            {
                user = UserDataAccess.GetUserByUsername(username); 
                if (user != null)
                {

                    if (user.password.Equals(password))
                    {
                        response["authenticated"] = true;
                        response["message"] = string.Format("User {0} was authenticated successfully", user.username);
                        if (hidePassword)
                        {
                            user.password = string.Concat("".PadLeft(user.password.Length, '*'), user.password.Substring(user.password.Length));
                        }
                        response["user"] = JObject.FromObject(user);
                    }
                    else
                    {
                        response["authenticated"] = false;
                        response["message"] = string.Format("Could not authenticate user with username {0}", user.username);
                        response["user"] = null;
                    }
                }
                else
                {
                    response["authenticated"] = false;
                    response["message"] = string.Format("No user registered with username {0}", username);
                    response["user"] = null;
                }
            }
            catch (Exception e)
            {
                
                response["authenticated"] = false;
                response["message"] = "Error occured while authenticating user";
                response["errorDetails"] = JObject.FromObject(e);
                response["user"] = null;
                Console.WriteLine(e.StackTrace);
            }

            return response;
        }
        /*
        {
         "cmd" : "create",
         "user" : {
            "usenrame" : "mudasir"

            }
        }
            
            */


         
        [Route("api/userop")]
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject userop(JObject jparams)
        {
            JObject response = new JObject();
            JObject user = (JObject)jparams.GetValue("user");
            string cmd = (string) jparams.GetValue("cmd");
            SiteUser newUser = user.ToObject<SiteUser>();

            string status = UserDataAccess.Validate(newUser);
            if (!status.Equals("ok"))
            {
                response["status"] = false;
                response["message"] = status;
                response["user"] = user;
                return response;
            }

            try
            {

                if (cmd.ToLower().Equals("create"))
                {
                    UserDataAccess.CreateUser(newUser);                        
                    response["status"] = true;
                    response["message"] = "Created";                        
                    response["user"] = user;
                }
                else
                {                    
                    if (UserDataAccess.UpdateUser(newUser))
                    {
                        response["status"] = true;
                        response["message"] = "Updated";
                        response["user"] = user;
                    }
                    else
                    {
                        response["status"] = false;
                        response["message"] = "NoSuchUserError";
                        response["user"] = user;
                    }
                }
                
            }
            catch (CrawlerDataError e)
            {
                response["status"] = false;
                response["message"] = e.Message;
                response["user"] = user;
            }           
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = "GenericError";
                response["user"] = user;
                response["exception"] = JObject.FromObject(e);
            }
            if (hidePassword && user != null)
            {
                string p = user.GetValue("password").ToString();
                user["password"] = string.Concat("".PadLeft(p.Length, '*'), p.Substring(p.Length));
            }
            return response;
        }
    }
}
