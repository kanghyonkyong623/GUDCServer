using GUDC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GUDC.Controllers
{
    public class MobileController : Controller
    {
        //
        // GET: /Mobile/
        GUDCContext _db = new GUDCContext();
        public TaskEntry selectedTask = null;
        public bool isTaskAccept = false;
        public string mTaskName = null;
        public int mCount = 0;
        public string mTeamName = null;
        public ActionResult Index(string username = "", string teamloc = "")
        {
            if ((_db.Users.FirstOrDefault(u => u.UserName == username)) != null)
            {

                var user = _db.TeamMemberEntries.FirstOrDefault(u => u.TEAMMEMBERNAME == username);
                if (user != null)
                {                     
                    var teamName = user.TEAMCODE;
                  
                    var team = _db.TeamItems.FirstOrDefault(u => u.TEAMNAME == teamName);
                    if (team != null)
                    {
                        mTeamName = team.TEAMNAME;
                        if (!String.IsNullOrEmpty(teamloc))
                        {
                            //firstTeamloc = teamloc;
                            team.TEAMLOCATION = teamloc;
                            _db.Entry(team).State = EntityState.Modified;
                            _db.SaveChanges();
                        }
                        ViewBag.TeamLoction = teamloc;
                        ViewBag.TeamName = team.TEAMNAME;
                    }
                    var taskList = _db.TaskEntries.Where(u => u.TEAMNAME == user.TEAMCODE && u.STATUS == "Open").ToList();
                    

                    if (taskList != null)
                    {
                        ViewBag.taskList = taskList;
                        //selectedTask = task;
                    }
                }
            }
            return View();
            //}
            //else
            //{
            //    return RedirectToAction("Login", "MyAccount");
            //}

            //return View();
        }
       [HttpPost]
        public ActionResult TaskStatus(string status, string teamname, string taskname)
        {
            if (!string.IsNullOrWhiteSpace(teamname))
            {
                //var teamMem = _db.TeamMemberEntries.FirstOrDefault(u => u.TEAMMEMBERNAME == username);
                //if (teamMem != null)
                //{
                    var task = _db.TaskEntries.FirstOrDefault(u => u.TEAMNAME == teamname && u.STATUS == "Open" && u.TASK == taskname);

                    //selectedTask.STATUS = status;
                    if (task != null)
                    {
                        task.STATUS = status;
                        _db.Entry(task).State = EntityState.Modified;
                        if (status == "Closed")
                        {
                            var teamName = task.TEAMNAME;
                            var team = _db.TeamItems.FirstOrDefault(u => u.TEAMNAME == teamName);
                            if (team != null)
                            {
                                team.TEAMSTATUS = "free";
                                _db.Entry(team).State = EntityState.Modified;


                            }

                        }
                        _db.SaveChanges();
                    }

                }
            
            return Json("");
        }
       [HttpPost]
       public ActionResult TaskAccept(string teamloc, string teamname, string taskname)
       {
           if (!string.IsNullOrWhiteSpace(teamloc) && !string.IsNullOrWhiteSpace(taskname))
           {
               isTaskAccept = true;
               mTaskName = taskname;
               PointsTable fPoint = new PointsTable();
               fPoint.taskName = taskname;
               fPoint.lat = double.Parse(teamloc.Split(',')[0]);
               fPoint.lng = double.Parse(teamloc.Split(',')[1]);
               fPoint.index = 0;
               TrackInfoTable info = new TrackInfoTable();
               info.date = DateTime.Now;
               info.teamName = teamname;
               info.taskName = fPoint.taskName;
               _db.PointArrayTable.Add(fPoint);
               _db.TrackInfoTables.Add(info);
               _db.SaveChanges();
           }
           mCount = 0;
           return Json("");
       }

       [HttpPost]
       public ActionResult ChangeTeamLocation(string teamLocation, string teamname,string taskname="", int isaccept = 0)
       {

           if (!String.IsNullOrWhiteSpace(teamLocation) && !String.IsNullOrWhiteSpace(teamname))
           {
               //var teamMem = _db.TeamMemberEntries.FirstOrDefault(u => u.TEAMMEMBERNAME == teamname);
               // if (teamMem != null)
               // {
                    var team = _db.TeamItems.FirstOrDefault(u => u.TEAMNAME == teamname);

                    if (team != null)
                    {
                        team.TEAMLOCATION = teamLocation;
                        _db.Entry(team).State = EntityState.Modified;
                        _db.SaveChanges();
                        if (isaccept > 0)
                        {
                            PointsTable pt = new PointsTable();
                            pt.taskName = taskname;
                            pt.index = isaccept;
                            pt.lat = double.Parse(teamLocation.Split(',')[0]);
                            pt.lng = double.Parse(teamLocation.Split(',')[1]);
                            _db.PointArrayTable.Add(pt);
                            //TrackInfoTable info = new TrackInfoTable();
                            //info.date = DateTime.Now.Date;
                            //info.teamName = team.TEAMNAME;
                            //info.taskName = mTaskName;
                            //_db.TrackInfoTables.Add(info);
                            _db.SaveChanges();
                            return Json("success");
                        }                        
                    }
                    
                //}
           }
           return Json("");
       }
       [HttpPost]
       public ActionResult TaskArrived(string taskname, string teamname, int index)
       {
           
               isTaskAccept = false;
               PointsTable pt = new PointsTable();
               //pt.teamName = mTeamName;
               pt.taskName = taskname;
               //pt.date = DateTime.Now.Date;
               pt.index = index;
               var TaskItem = _db.TaskEntries.FirstOrDefault(m => m.TEAMNAME == teamname && m.TASK == taskname);
               pt.lat = double.Parse(TaskItem.COORDINATE.Split(',')[0]);
               pt.lng = double.Parse(TaskItem.COORDINATE.Split(',')[1]);
               _db.PointArrayTable.Add(pt);
               _db.SaveChanges();
          
           return Json(null);
       }
       [HttpPost]
       public ActionResult readTask(string teamName)
       {
           if (Request.IsAjaxRequest())
           {
                var TaskItems = _db.TaskEntries.Where(m => m.TEAMNAME == teamName && m.STATUS == "Open").ToList();

                return Json(TaskItems);

           }
           return Json(null);
       }
    }
}
