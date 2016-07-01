using GUDC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace GUDC.Controllers
{
    public class MyAccountController : Controller
    {
        public static SelectList UserRoles = new SelectList(new string[] { "Administrator", "Crew Teams Manager", "Crew Member" });
        public static SelectList DistrictCategory;
        public List<string> strDistricts;
        GUDCContext _db = new GUDCContext();
        //
        // GET: /MyAccount/

        public ActionResult Login(string ReturnUrl = "")
        {
            
            if (DistrictCategory == null)
            {
                var asd = Request.PhysicalApplicationPath;
                string[] lines = System.IO.File.ReadAllLines(asd + @"/App_Data/sec_name.csv");

                strDistricts = new List<string>();
                foreach (string line in lines)
                {
                    string[] elements = line.Split(',');
                    if (elements[3] == "SEC_NAME") continue;

                    if (!strDistricts.Contains(elements[3]))
                    {
                        strDistricts.Add(elements[3]);
                    }
                }

                DistrictCategory = new SelectList(strDistricts);
            }

            if (_db.Users.ToList().Count == 0)
            {
                return RedirectToAction("Register");
            }
            if (Session["indexLogin"] != null)
            {
                int count = (int)Session["indexLogin"];
                if (count >= 1)
                {
                    return RedirectToAction("ExternalLoginFailure");
                }
            }
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model, string returnUrl)
        {

            if (ModelState.IsValid && Authenticate(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                if (Session["indexLogin"] != null)
                {
                    Session["indexLogin"] = 0;
                }
                if (model.RememberMe)
                {
                    var json = JsonConvert.SerializeObject(model);
                    var userCookie = new HttpCookie("user", json);
                    userCookie.Expires.AddDays(365);
                    HttpContext.Response.Cookies.Add(userCookie);
                }
                else
                {
                    var user = Request.Cookies["user"];
                    if (user != null)
                    {
                        //Response.Cookies.Remove("user");
                        user.Value = null;
                        Response.SetCookie(user);
                        Response.Cookies.Add(user);
                    }
                }
                return RedirectToAction("Index", "Teams");
                //return RedirectToAction("index", "Mobile");

            }
            //indexLogin += 1;
            //if (indexLogin >= 3) return RedirectToLocal(Url.Action("ExternalLoginFailure"));
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            if (Session["indexLogin"] != null)
            {
                int count = (int)Session["indexLogin"];
                if (count >= 1) return RedirectToLocal(Url.Action("ExternalLoginFailure"));
                Session["indexLogin"] = count + 1;
            }
            else
                Session.Add("indexLogin", 0);
            return View(model);
        }
        public ActionResult Logout()
        {
            Session.RemoveAll();
            //var user = Request.Cookies["user"];
            ////var user = new HttpCookie("user")
            ////{
            ////    Expires = DateTime.Now.AddDays(-1),
            ////    Value = null
            ////};
            //user.Value = null;
            ////user.Expires = DateTime.Now.AddDays(-1);
            //Response.SetCookie(user);
            //Response.Cookies.Add(user);
            return RedirectToAction("Index", "Teams");
        }
        //
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }


        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            //var asd = Request.PhysicalApplicationPath;
            ///////////////////////////////////////////////////////////////////
            //string[] lines = System.IO.File.ReadAllLines(asd + @"/App_Data/teams.csv");

            //foreach (string line in lines)
            //{
            //    string[] elements = line.Split(',');
            //    if (elements[0] == "X") continue;
            //    TeamItem team = new TeamItem();
            //    team.TEAMLOCATION = elements[1] + "," + elements[0];
            //    team.TEAMNAME = elements[3];
            //    team.id = int.Parse(elements[2]);
            //    team.TEAMSTATUS = "out of service";
            //    _db.TeamItems.Add(team);
            //    _db.SaveChanges();
            //}
            if (_db.Users.ToList().Count == 0)
            {
                return View();

            }

            //ViewBag.ReturnUrl = ReturnUrl;
            return RedirectToAction("Login");
        }
        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    _db.TeamEntries.Add(new TeamEntry()
                    {
                        USENAME = model.UserName,
                        PHONE = model.PHONE,
                        EMAIL = model.EMAIL,
                        ROLE = model.Role,
                        TEAMCODE = model.TEAMCODE,
                        NAME = model.NAME
                    });
                    _db.SaveChanges();
                    TeamEntry firstUser =_db.TeamEntries.First(m=>m.USENAME == model.UserName);
                    _db.Users.Add(new Users() { UserName = model.UserName, Password = model.Password, Role = "Administrator", SeletUserId = firstUser.id});
                    _db.SaveChanges();
                    Authenticate(model.UserName, model.Password, false);
                    return RedirectToAction("Index", "Teams");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private bool Authenticate(string UserName, string Password, bool persistCookie)
        {
            try
            {
                var userValid = _db.Users.Any(u => u.UserName == UserName && u.Password == Password);

                if (userValid)
                {
                    var user = _db.Users.First(u => u.UserName == UserName && u.Password == Password);
                    Session.Add("IsAuthenticated", true);
                    Session.Add("UserName", UserName);
                    Session.Add("Role", user.Role);
                }
                return userValid;
            }
            catch
            {
                return false;
            }
        }
        [HttpPost]
        public ActionResult LoginMobile(LoginModel model)
        {
            Dictionary<string, string> mobileLoginResult = new Dictionary<string, string>();
            if (ModelState.IsValid && Authenticate(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                if (Session["indexLogin"] != null)
                {
                    Session["indexLogin"] = 0;
                }

                var user = _db.Users.FirstOrDefault(u => u.UserName == model.UserName);
                if (user != null)
                {
                    mobileLoginResult.Add("result", "Succeed");
                    mobileLoginResult.Add("username", user.UserName);

                    var teamMember = _db.TeamMemberEntries.FirstOrDefault(u => u.TEAMMEMBERNAME == user.UserName);
                    if (teamMember != null)
                    {
                        mobileLoginResult.Add("teamname", teamMember.TEAMCODE);
                        var task = _db.TaskEntries.FirstOrDefault(u => u.TEAMNAME == teamMember.TEAMCODE && u.STATUS == "Open");
                        if (task != null)
                        {
                            mobileLoginResult.Add("tasklocation", task.COORDINATE);
                        }
                        else
                        {
                            mobileLoginResult.Add("tasklocation", "");
                        }
                    }
                    else
                    {
                        mobileLoginResult.Add("teamname", "");
                        mobileLoginResult.Add("tasklocation", "");
                    }
                    return Json(mobileLoginResult);
                    
                    //return Json(user);
                    
                }
                else
                {
                    mobileLoginResult.Add("result", "Failed");
                    mobileLoginResult.Add("error", "User name is not exist in users table.");
                    return Json(mobileLoginResult);
                }
                
            }
            else
            {
                mobileLoginResult.Add("result", "Failed");
                mobileLoginResult.Add("error", "This user can not log in.");
                return Json(mobileLoginResult);
            }
            
        }
        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Teams");
            }
        }
        #endregion
    }
}
