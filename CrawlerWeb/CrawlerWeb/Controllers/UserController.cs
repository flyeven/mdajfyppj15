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


        [Route("api/user")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject getUser([FromUri(Name = "uid", SuppressPrefixCheck = false)] int id = -1)
        {
            JObject o = new JObject();
            try
            {
                o["status"] = true;                
                o["user"] = UserDataAccess.UserToObject(UserDataAccess.GetUser(id));
            }
            catch (Exception e)
            {
                o["status"] = false;
                o["message"] = e.Message;
                o["user"] = null;
               
            }
            return o;
        }


        [Route("api/verify")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject VerifyEmail([FromUri(Name = "code", SuppressPrefixCheck = false)] string code = "",
            [FromUri(Name = "username", SuppressPrefixCheck = false)] string username = "")
        {
            JObject response = new JObject();
            ;
            try
            {
                bool isVerified = UserDataAccess.Verify(code, username);
                response["verified"] = isVerified;
                response["message"] = isVerified ? "User email verified" : "User email could not be verified";
            }
            catch (Exception e)
            {
                response["verified"] = false;
                response["message"] = e.Message;
            }

            return response;
        }

        [Route("api/verificationemail")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JObject SendVerificationEmail([FromUri(Name = "username", SuppressPrefixCheck = false)] string username = "")
        {
            JObject response = new JObject();
            ;
            try
            {
                UserDataAccess.SendVerificationLink(username);
                response["status"] = true;
                //response["message"] = isVerified ? "User email verified" : "User email could not be verified";
            }
            catch (Exception e)
            {
                response["status"] = false;
                response["message"] = e.Message;
            }

            return response;
        }


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
                    if (user.Verification != null && user.Verification.StartsWith("KEY"))
                    {
                        response["authenticated"] = false;
                        response["message"] = "The user was authenticated successfully, but has not been verified throguh the email, kindly verify your account. Click <a href=\"Javascript:Ajax('"+UserDataAccess.Host()+"api/verificationemail?username="+user.username+"');\">here</a> to send verification link again";
                        response["user"] = null;
                    }
                    else
                    {
                        if (user.password.Equals(password))
                        {
                            response["authenticated"] = true;
                            response["message"] = string.Format("User {0} was authenticated successfully", user.username);
                            if (hidePassword)
                            {
                                user.password = string.Concat("".PadLeft(user.password.Length, '*'), user.password.Substring(user.password.Length));
                            }
                            response["user"] = UserDataAccess.UserToObject(user);
                        }
                        else
                        {
                            response["authenticated"] = false;
                            response["message"] = string.Format("Could not authenticate user with username {0}", user.username);
                            response["user"] = null;
                        }
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
                    user["id"] = UserDataAccess.CreateUser(newUser);                        
                    response["status"] = true;
                    response["message"] = "Created";                        
                    response["user"] = user;
                }
                else
                {
                    newUser.password = null;
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
                response["message"] = e.Message;
                response["user"] = user;
            }
            if (hidePassword && user != null)
            {
                if (user.GetValue("password") != null)
                {
                    string p = user.GetValue("password").ToString();
                    user["password"] = string.Concat("".PadLeft(p.Length, '*'), p.Substring(p.Length));
                }
            }
            return response;
        }
    }
}
