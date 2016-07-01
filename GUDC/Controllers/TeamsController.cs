using GUDC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using WebMatrix.WebData;
using System.Web.Security;
//using MvcCheckBoxList.Model;

namespace GUDC.Controllers
{
    public class TeamsController : Controller
    {
        GUDCContext _db = new GUDCContext();
        
        //public string SelectedTeamCode;
        //public static SelectList TeamCodes;
        public static string [] strRoles = new string[] { "Administrator", "Crew Teams Manager", "Crew Member" }; 
        public static SelectList TeamStatuses = new SelectList(new string[] { "free", "on mission", "out of service" });
        public static SelectList TeamMemberRole = new SelectList(new string[] {"team member", "driver", "team leader"});
        public bool editTeamFlag = false;
        //
        // GET: /Teams/

        public ActionResult Index()
        {

            if (Session["UserName"] != null)
            {
                if ((string)Session["Role"] == "Administrator")
                {
                    @ViewBag.ActivePage = "USERS";
                    
                    ViewBag.SessionRole = "Administrator";
                    ViewBag.TeamEntries = _db.TeamEntries.ToList();
                    return View();
                }
                else
                {
                    ViewBag.SessionRole = (string)Session["Role"];
                    return RedirectToAction("TeamIndex");

                }

            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        public ActionResult Add()
        {
            if (Session["UserName"] != null)
            {
                if ((string)Session["Role"] == "Administrator")
                {
                    ViewBag.UserRoles = MyAccountController.UserRoles;
                    var teams = _db.TeamItems.ToList();
                    List<string> strTeams = new List<string>();
                    foreach (TeamItem team in teams)
                    {
                        strTeams.Add(team.TEAMNAME);                        
                    }

                    ViewBag.Teams = new SelectList(strTeams);
                     
                    @ViewBag.ActivePage = "USERS";
                   
                    ViewBag.Title = "Add User";
                    return View();
                }
                else
                {
                    return RedirectToAction("AddEditTeam");
                }
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }
        }

        [HttpPost]
        public ActionResult Add(RegisterModel model)
        {
            if (Session["UserName"] != null)
            {
                try
                {
                    TeamEntry userTeam = new TeamEntry()
                    {
                        USENAME = model.UserName,
                        PHONE = model.PHONE,
                        EMAIL = model.EMAIL,
                        ROLE = model.Role,
                        NAME = model.NAME
                    };
                    if (model.Team != "Administrator") userTeam.TEAMCODE = model.Team;
                    _db.TeamEntries.Add(userTeam);
                    _db.SaveChanges();
                    //TeamEntry firstUser = _db.TeamEntries.First(m => m.USENAME == model.UserName && m.NAME == model.NAME);
                    TeamEntry firstUser = _db.TeamEntries.First(m => m.USENAME == model.UserName );
                    _db.Users.Add(new Users() { UserName = model.UserName, Password = model.Password, Role = model.Role, SeletUserId = firstUser.id });
                    _db.SaveChanges();

                    if (model.Role == "Crew Teams Manager")
                    {
                        _db.TeamMemberEntries.Add(new TeamMemberEntry() { TEAMCODE = model.Team, TEAMMEMBERNAME = model.UserName, TEAMMEMBERNO = model.PHONE, TEAMMEMBERROLE = "team leader" });
                        TeamItem team = _db.TeamItems.First(f => f.TEAMNAME == model.Team);
                        int count = team.MEMBERCOUNT;
                        team.MEMBERCOUNT = count + 1;
                        if (team.TEAMSTATUS == "out of service")
                        {
                            team.TEAMSTATUS = "free";
                        }
                        _db.Entry(team).State = EntityState.Modified;
                        _db.SaveChanges();
                    }
                    else if (model.Role == "Crew Member")
                    {
                        _db.TeamMemberEntries.Add(new TeamMemberEntry() { TEAMCODE = model.Team, TEAMMEMBERNAME = model.UserName, TEAMMEMBERNO = model.PHONE, TEAMMEMBERROLE = "team member" });
                        TeamItem team = _db.TeamItems.First(f => f.TEAMNAME == model.Team);
                        int count = team.MEMBERCOUNT;
                        team.MEMBERCOUNT = count + 1;
                        if (team.TEAMSTATUS == "out of service")
                        {
                            team.TEAMSTATUS = "free";
                        }
                        _db.Entry(team).State = EntityState.Modified;
                        _db.SaveChanges();
                    }
                    return RedirectToAction("Index", "Teams");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", MyAccountController.ErrorCodeToString(e.StatusCode));
                }
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        public ActionResult Edit(int id = 0)
        {
            if (Session["UserName"] != null)
            {
                if ((string)Session["Role"] == "Administrator")
                {
                    ViewBag.UserRoles = MyAccountController.UserRoles;
                    @ViewBag.ActivePage = "USERS";
                    
                    ViewBag.Title = "Edit User";
                    TeamEntry teamEntry = _db.TeamEntries.Find(id);
                    //var user = _db.Users.First(u => u.UserName == teamEntry.USENAME);
                    Users user = _db.Users.Find(id);

                    RegisterModel editModel = new RegisterModel()
                    {
                        UserName = teamEntry.USENAME,
                        PHONE = teamEntry.PHONE,
                        EMAIL = teamEntry.EMAIL,
                        
                        SelectUserId = teamEntry.id,
                        NAME = teamEntry.NAME,
                        Password = user.Password,
                        ConfirmPassword = user.Password,
                        Role = user.Role
                        
                    };
                    if (user.Role != "Administrator")
                    {
                        TeamMemberEntry member = _db.TeamMemberEntries.FirstOrDefault(m => m.TEAMMEMBERNAME == teamEntry.NAME && m.TEAMCODE == teamEntry.TEAMCODE);
                        if (member != null)
                        {
                            editModel.Team = member.TEAMCODE;
                        }
                    }

                    ViewBag.EditModel = editModel;
                    var teams = _db.TeamItems.ToList();
                    List<string> strTeams = new List<string>();
                    foreach (TeamItem team in teams)
                    {
                        strTeams.Add(team.TEAMNAME);
                    }

                    ViewBag.Teams = new SelectList(strTeams);
                    return View("Add", editModel);

                }
                else
                {
                    return RedirectToAction("TeamMemberEdit");
                }

            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }
        [HttpPost]
        public ActionResult Edit(RegisterModel registerModel)
        {
            if (Session["UserName"] != null)
            {
                TeamEntry teamEntry = _db.TeamEntries.Find(registerModel.SelectUserId);
                string userName = teamEntry.USENAME;
                string userRole = teamEntry.ROLE;
                string userTeam = teamEntry.TEAMCODE;
                if (userRole != registerModel.Role)
                {
                    if (userRole == strRoles[0])
                    {
                        string tememberRole = "team member";
                        if (registerModel.Role == "Crew Teams Manager")
                        {
                            tememberRole = "team leader";
                        }
                        _db.TeamMemberEntries.Add(new TeamMemberEntry() { TEAMCODE = registerModel.Team, TEAMMEMBERNAME = registerModel.UserName, TEAMMEMBERNO = registerModel.PHONE, TEAMMEMBERROLE = tememberRole });
                        TeamItem team = _db.TeamItems.First(f => f.TEAMNAME == registerModel.Team);
                        int count = team.MEMBERCOUNT;
                        team.MEMBERCOUNT = count + 1;
                        if (team.TEAMSTATUS == "out of service")
                        {
                            team.TEAMSTATUS = "free";
                        }
                        _db.Entry(team).State = EntityState.Modified;
                        
                        _db.SaveChanges();
                     }
                    else if (registerModel.Role == strRoles[0])
                    {
                        TeamMemberEntry mem = _db.TeamMemberEntries.FirstOrDefault(m => m.TEAMCODE == userTeam && m.TEAMMEMBERNAME == userName);
                        _db.Entry(mem).State = EntityState.Deleted;
                        TeamItem team = _db.TeamItems.First(f => f.TEAMNAME == userTeam);
                        int count = team.MEMBERCOUNT;
                        team.MEMBERCOUNT = count - 1;
                        if (team.MEMBERCOUNT < 1)
                        {
                            team.TEAMSTATUS = "out of service";
                        }
                        _db.Entry(team).State = EntityState.Modified;

                        _db.SaveChanges();
                    }
                    else
                    {
                        string tememberRole = "team member";
                        if (registerModel.Role == "Crew Teams Manager")
                        {
                            tememberRole = "team leader";
                        }
                        _db.TeamMemberEntries.Add(new TeamMemberEntry() { TEAMCODE = registerModel.Team, TEAMMEMBERNAME = registerModel.UserName, TEAMMEMBERNO = registerModel.PHONE, TEAMMEMBERROLE = tememberRole });
                        _db.SaveChanges();
                    }

                 }
                else if (userTeam != registerModel.Team)
                {
                    TeamMemberEntry mem = _db.TeamMemberEntries.FirstOrDefault(m => m.TEAMCODE == userTeam && m.TEAMMEMBERNAME == userName);
                    _db.Entry(mem).State = EntityState.Deleted;
                    TeamItem team = _db.TeamItems.First(f => f.TEAMNAME == userTeam);
                    int count = team.MEMBERCOUNT;
                    team.MEMBERCOUNT = count - 1;
                    if (team.MEMBERCOUNT < 1)
                    {
                        team.TEAMSTATUS = "out of service";
                    }
                    _db.Entry(team).State = EntityState.Modified;

                    string tememberRole = "team member";
                    if (registerModel.Role == "Crew Teams Manager")
                    {
                        tememberRole = "team leader";
                    }
                    _db.TeamMemberEntries.Add(new TeamMemberEntry() { TEAMCODE = registerModel.Team, TEAMMEMBERNAME = registerModel.UserName, TEAMMEMBERNO = registerModel.PHONE, TEAMMEMBERROLE = tememberRole });
                    TeamItem team1 = _db.TeamItems.First(f => f.TEAMNAME == registerModel.Team);
                    count = team1.MEMBERCOUNT;
                    team1.MEMBERCOUNT = count + 1;
                    if (team1.TEAMSTATUS == "out of service")
                    {
                        team1.TEAMSTATUS = "free";
                    }
                    _db.Entry(team1).State = EntityState.Modified;

                    _db.SaveChanges();

                }
                
                teamEntry.USENAME = registerModel.UserName;
                teamEntry.PHONE = registerModel.PHONE;
                teamEntry.EMAIL = registerModel.EMAIL;
                teamEntry.ROLE = registerModel.Role;
                teamEntry.TEAMCODE = registerModel.Team;
                teamEntry.NAME = registerModel.NAME;

                Users user = _db.Users.First(m=> m.UserName == userName && m.SeletUserId == registerModel.SelectUserId);
                user.Password = registerModel.Password;
                user.UserName = registerModel.UserName;
                user.Role = registerModel.Role;
                
                _db.Entry(teamEntry).State = EntityState.Modified;
                _db.Entry(user).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        public ActionResult Delete(int id = 0)
        {
            if (Session["UserName"] != null)
            {
                if ((string)Session["Role"] == "Administrator")
                {
                    @ViewBag.ActivePage = "USERS";
                    
                    TeamEntry teamEntry= _db.TeamEntries.Find(id);
                    //ViewBag.TeamEntry = teamEntry;
                    Users user = _db.Users.First(m => m.UserName == teamEntry.USENAME && m.SeletUserId == id);
                    _db.Entry(teamEntry).State = EntityState.Deleted;
                    _db.Entry(user).State = EntityState.Deleted;
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("TeamMemberDelete");
                }
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        //public ActionResult TeamMemberIndex(string teamcode = "")
        //{
        //    if (Session["UserName"] != null)
        //    {
        //        //ViewBag.ActivePage = "TEAMS";
        //        //ViewBag.TeamMemberEntries = _db.TeamMemberEntries.ToList();

        //        return RedirectToAction("AddEditTeamMembers");
        //    }
        //    else
        //    {
        //        return RedirectToAction("Login", "MyAccount");
        //    }

        //}

        public ActionResult AddEditTeamMembers(string teamcode = "")
        {
            if (Session["UserName"] != null)
            {

                ViewBag.ActivePage = "TEAMS";
                ViewBag.TeamMemberEntries = _db.TeamMemberEntries.ToList();
                List<TeamItem> teamItems = _db.TeamItems.ToList();
                List<string> teamCodeCategory = new List<string>();
                foreach (TeamItem teamMember in teamItems)
                {
                    teamCodeCategory.Add(teamMember.TEAMNAME);
                }
                ViewBag.TeamCodes  = new SelectList(teamCodeCategory);
                //= TeamCodes;
                //SelectedTeamCode = teamcode;
                ViewBag.DistrictCategory = MyAccountController.DistrictCategory;
                ViewBag.TeamMemberRole = TeamMemberRole;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        [HttpPost]
        public ActionResult AddEditTeamMembers(TeamMemberEntry teamMemberEntry)
        {
            if (Session["UserName"] != null)
            {
                _db.TeamMemberEntries.Add(teamMemberEntry);
                TeamItem team = _db.TeamItems.First(f => f.TEAMNAME == teamMemberEntry.TEAMCODE);
                int count = team.MEMBERCOUNT;
                team.MEMBERCOUNT = count + 1;
                if (team.TEAMSTATUS == "out of service")
                {
                    team.TEAMSTATUS = "free";
                }
                _db.Entry(team).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("AddEditTeamMembers");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        public ActionResult TeamMemberEdit(int id = 0)
        {
            if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "TEAMS";
                ViewBag.TeamMemberEntry = _db.TeamMemberEntries.Find(id);
                if (ViewBag.TeamMemberEntry == null)
                {
                    return HttpNotFound();
                }
                ViewBag.TeamMemberRole = TeamMemberRole;
                List<TeamItem> teamItems = _db.TeamItems.ToList();
                List<string> teamCodeCategory = new List<string>();
                foreach (TeamItem teamMember in teamItems)
                {
                    teamCodeCategory.Add(teamMember.TEAMNAME);
                }
                ViewBag.TeamCodes = new SelectList(teamCodeCategory);
                
                return View("TeamMemberEdit", ViewBag.TeamMemberEntry);
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }
        [HttpPost]
        public ActionResult TeamMemberEdit(TeamMemberEntry teamMemberEntry)
        {
            if (Session["UserName"] != null)
            {
                _db.Entry(teamMemberEntry).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("AddEditTeamMembers");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        public ActionResult TeamMemberDelete(int id = 0)
        {
            if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "TEAMS";
                var TeamMemberEntryItem = _db.TeamMemberEntries.Find(id);
                _db.Entry(TeamMemberEntryItem).State = EntityState.Deleted;

                TeamItem team = _db.TeamItems.FirstOrDefault(f => f.TEAMNAME == TeamMemberEntryItem.TEAMCODE);
                if (team != null)
                {
                    int count = team.MEMBERCOUNT;
                    team.MEMBERCOUNT = count - 1;
                    if (team.MEMBERCOUNT < 1)
                    {
                        team.TEAMSTATUS = "out of service";
                        _db.Entry(team).State = EntityState.Modified;
                    }
                }

               _db.SaveChanges();
                
                return RedirectToAction("AddEditTeamMembers");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        ////////////////////Team Edit/////////////////////////////
        public ActionResult TeamIndex()
        {
            if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "TEAMS";
                ViewBag.TeamItems = _db.TeamItems.ToList();
                
                return RedirectToAction("AddEditTeam");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }
        public ActionResult AddEditTeam()
        {
            if (Session["TeamEditStatus"] != null)
            {
                Session["TeamEditStatus"] = false;
            }

            if (Session["UserName"] != null)
            {
                editTeamFlag = false;
                ViewBag.ActivePage = "TEAMS";
                ViewBag.TeamItems = _db.TeamItems.ToList();
                ViewBag.DistrictCategory = MyAccountController.DistrictCategory;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        [HttpPost]
        public ActionResult AddEditTeam(TeamItem teamItem)
        {
            if (Session["UserName"] != null)
            {
                if (teamItem.TEAMNAME == null)
                {
                    return RedirectToAction("TeamIndex");
                }
                teamItem.MEMBERCOUNT = 0;
                teamItem.TEAMSTATUS = "out of service";
                _db.TeamItems.Add(teamItem);
                _db.SaveChanges();
                return RedirectToAction("TeamIndex");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        public ActionResult TeamItemEdit(int id = 0)
        {
            if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "TEAMS";
                ViewBag.title = "TEAM EDIT";
                ViewBag.DistrictCategory = MyAccountController.DistrictCategory;
                TeamItem item = _db.TeamItems.Find(id);
                if (item == null)
                {
                    return HttpNotFound();
                }
                string district = item.DISTRICT;
                if (district != null && district != "")
                {
                    string[] listStr = district.Split(new char[]{','});
                    string[] SelectedDistrict = new string[listStr.Count()];
                    for (int i=0; i< listStr.Count(); i++)
                    {
                        string txtItem = listStr[i];
                        if (listStr[i].Substring(0, 1) == " ")
                        {
                            txtItem = listStr[i].Trim();
                            
                        }
                        SelectedDistrict[i] = txtItem;
                    }
                    ViewBag.SelectedDistricts = SelectedDistrict;
                }
                if (Session["TeamEditStatus"] != null)
                {
                    Session["TeamEditStatus"] = true;
                }
                else
                {
                    Session.Add("TeamEditStatus", true);
                }
                return View("TeamItemEdit", item);
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }
        [HttpPost]
        public ActionResult TeamItemEdit(TeamItem teamItem)
        {
            if (Session["UserName"] != null)
            {
                _db.Entry(teamItem).State = EntityState.Modified;
                _db.SaveChanges();
                if (Session["TeamEditStatus"] != null)
                {
                    Session["TeamEditStatus"] = false;
                }
                return RedirectToAction("TeamIndex");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        public ActionResult TeamItemDelete(int id = 0)
        {
            if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "TEAMS";
                TeamItem TeamItem = _db.TeamItems.Find(id);
                _db.Entry(TeamItem).State = EntityState.Deleted;
                _db.SaveChanges();
                var data = _db.TeamMemberEntries.Where(f => f.TEAMCODE == TeamItem.TEAMNAME).ToList();
                foreach (TeamMemberEntry p in data)
                {
                    p.TEAMCODE = "";
                    _db.Entry(p).State = EntityState.Modified;
                    _db.SaveChanges();
                }

                
                return RedirectToAction("TeamIndex");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        public PartialViewResult SearchMember(string keyword)
        {
            if (keyword == "")
            {
                return PartialView(_db.TeamEntries.ToList());
            }
            else
            {
                var data = _db.TeamEntries.Where(f => f.NAME.Contains(keyword) || f.USENAME.Contains(keyword) || f.TEAMCODE.Contains(keyword)
                    || f.PHONE.Contains(keyword) || f.EMAIL.Contains(keyword)).ToList();
                return PartialView(data);
            }

        }
        public PartialViewResult SearchTeam(string keyword)
        {
            if (keyword == "")
            {
                return PartialView(_db.TeamItems.ToList());
            }
            else
            {
                var data = _db.TeamItems.Where(f =>  f.TEAMNAME.Contains(keyword) || f.DISTRICT.Contains(keyword) ||f.TEAMSTATUS.Contains(keyword)).ToList();
                return PartialView(data);
                //return PartialView(_db.TeamItems.ToList());
            }

        }
        public PartialViewResult SearchTeamMember(string keyword)
        {
            if (keyword == "")
            {
                //return PartialView(_db.TeamItems.Where(f => f.TEAMCODE.Contains(SelectedTeamCode)).ToList());
                return PartialView(_db.TeamMemberEntries.ToList());

            }
            else
            {
                var data = _db.TeamMemberEntries.Where(f => f.TEAMCODE.Contains(keyword) || f.TEAMMEMBERCODE.Contains(keyword) || f.TEAMMEMBERNAME.Contains(keyword) || f.TEAMMEMBERNO.Contains(keyword) || f.TEAMMEMBERROLE.Contains(keyword) || f.DESCRIPTION.Contains(keyword)).ToList();
                return PartialView(data);
                //return PartialView(_db.TeamItems.ToList());
            }

        }

        public ActionResult CheckTeamNameExist(string TeamName)
        
        {
            if (Session["TeamEditStatus"] != null && (bool)Session["TeamEditStatus"])
            {
                return Content("true"); 
            }
            //bool UserExist = true;

            //another codes to check wheather user exists
            var existingUser = _db.TeamItems.FirstOrDefault(u => u.TEAMNAME == TeamName);
            if (existingUser != null)
            {
                return Content("false");
            }
            else
            {
                return Content("true");
            }
        }
    }
}
