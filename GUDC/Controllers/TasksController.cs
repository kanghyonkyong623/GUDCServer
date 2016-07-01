using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using WebMatrix.WebData;
using GUDC.Models;

namespace GUDC.Controllers
{


    public class TasksController : Controller
    {
        //
        // GET: /Tasks/
        public static SelectList TaskStatus = new SelectList(new string[] { "Open", "Pending", "Closed" });
        GUDCContext _db = new GUDCContext();
        
        public ActionResult Index()
        {
            if (Session["UserName"] != null)
            {
                
                ViewBag.ActivePage = "TASKS";
                //ViewBag.TaskEntries = _db.TaskEntries.ToList();
                //List<TeamItem> teamItems = _db.TeamItems.ToList();
                //List<string> teamCodeCategory = new List<string>();
                //foreach (TeamItem teamMember in teamItems)
                //{
                //    teamCodeCategory.Add(teamMember.TEAMCODE);
                //}
                //SelectList TeamCodes1 = new SelectList(teamCodeCategory);
                //ViewBag.TeamCodes = TeamCodes1;
                //if (flagTask)
                //{
                //    ViewBag.CurrentDate = DateTime.Now;
                //}
                //else
                //{
                //    ViewBag.CurrentDate = null;
                //}
                return View();
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }
            
        }


        public ActionResult AddNewTask()
        {

            if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "TASKS";
                List<TeamItem> teamItems = _db.TeamItems.ToList();
                List<string> teamCodeCategory = new List<string>();
                foreach (TeamItem teamMember in teamItems)
                {
                    //if (teamMember.TEAMSTATUS == "free") teamCodeCategory.Add(teamMember.TEAMNAME);
                    teamCodeCategory.Add(teamMember.TEAMNAME);
                }
                SelectList TeamCodes1 = new SelectList(teamCodeCategory);
                ViewBag.TeamCodes = TeamCodes1;
                ViewBag.TaskStatus = TaskStatus;
                ViewBag.CurrentDate = DateTime.Now.ToString("yyyy'/'MM'/'dd' 'HH':'mm");
                
                return View();
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

            
         }

        [HttpPost]
        public ActionResult AddNewTask(TaskEntry taskEntry)
        {
            if (Session["UserName"] != null)
            {
                TeamItem team = _db.TeamItems.First(f => f.TEAMNAME == taskEntry.TEAMNAME);
                //if (taskEntry.STATUS == "Open")
                //{
                //    team.TEAMSTATUS = "on mission";
                //    _db.Entry(team).State = EntityState.Modified;
                //}
                //taskEntry.COORDINATE = "31.5,31.5";
                taskEntry.END = taskEntry.START;

                _db.TaskEntries.Add(taskEntry);
                _db.SaveChanges();
                
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }
        }
        public ActionResult EditTask(int id = 0)
        {
            if (Session["UserName"] != null)
            {
                List<TeamItem> teamItems = _db.TeamItems.ToList();
                List<string> teamCodeCategory = new List<string>();
                foreach (TeamItem teamMember in teamItems)
                {
                    teamCodeCategory.Add(teamMember.TEAMNAME);
                }
                SelectList TeamCodes1 = new SelectList(teamCodeCategory);
                ViewBag.TeamCodes = TeamCodes1;
                ViewBag.TaskStatus = TaskStatus;

                    ViewBag.ActivePage = "TASKS";
                    ViewBag.Title = "Edit Task";
                    TaskEntry taskEntry = _db.TaskEntries.Find(id);
                    
                    return View("AddNewTask", taskEntry);
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }
        [HttpPost]
        public ActionResult readTeamLocation(string teamname)
        {
            List<TeamItem> teamItems = _db.TeamItems.ToList();
            //List<string> teamCodeCategory = new List<string>();
            foreach (TeamItem team in teamItems)
            {
                if (team.TEAMNAME == teamname && !String.IsNullOrEmpty(team.TEAMLOCATION))
                {
                    return Json(team.TEAMLOCATION);
                }
            }
            return Json("");
        }
        [HttpPost]
        public ActionResult EditTask(TaskEntry taskEntry)
        {
            if (Session["UserName"] != null)
            {
                //DateTime initDate = new DateTime();
                
                if (taskEntry.STATUS == "Closed")
                {
                    taskEntry.END = DateTime.Now;
                    TeamItem team = _db.TeamItems.First(f => f.TEAMNAME == taskEntry.TEAMNAME);
                    team.TEAMSTATUS = "free";
                    
                    _db.Entry(team).State = EntityState.Modified;
                }
                
                _db.Entry(taskEntry).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }

        public ActionResult DeleteTask(int id = 0)
        {
            if (Session["UserName"] != null)
            {
                    ViewBag.ActivePage = "TASKS";
                    TaskEntry taskEntry= _db.TaskEntries.Find(id);
                    _db.Entry(taskEntry).State = EntityState.Deleted;
                    _db.SaveChanges();
                    return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }

        }
        public PartialViewResult SearchTask(string keyword)
        {
            if (keyword == "")
            {
                var data = _db.TaskEntries.ToList();
                data.Sort(CompareDate);
                return PartialView(data);
            }
            else
            {
                var data = _db.TaskEntries.Where(f => f.STATUS.Contains(keyword) || f.LOCATION.Contains(keyword) || f.TEAMNAME.Contains(keyword) || f.DETAILS.Contains(keyword)).ToList();
                data.Sort(CompareDate);
                return PartialView(data);
            }

        }
        private static int CompareDate(TaskEntry x, TaskEntry y)
        {
            if (x.END == null)
            {
                if (y.END == null)
                {
                    // If x is null and y is null, they're 
                    // equal.  
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y 
                    // is greater.  
                    return -1;
                }
            }
            else
            {
                // If x is not null... 
                // 
                if (y.END == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the  
                    // lengths of the two strings. 
                    // 
                    //DateTime xdt = Convert.ToDateTime(x.END);
                    //DateTime ydt = Convert.ToDateTime(y.END);
                    DateTime xdt = (x.END);
                    DateTime ydt = (y.END);



                    int retval = xdt.CompareTo(ydt);

                    if (retval != 0)
                    {
                        // If the strings are not of equal length, 
                        // the longer string is greater. 
                        // 
                        return retval;
                    }
                    else
                    {
                        // If the strings are of equal length, 
                        // sort them with ordinary string comparison. 
                        // 
                        return xdt.CompareTo(ydt);
                    }
                }
            }
        }
 

    }
}
