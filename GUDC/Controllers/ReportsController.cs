using GUDC.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace GUDC.Controllers
{
    public class ReportsController : Controller
    {
        //
        // GET: /Reports/
        GUDCContext _db = new GUDCContext();
        //public List<string> mTasks = new List<string>();
        public ActionResult Teams()
        {
            if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "REPORTS";
                List<TeamItem> teams = _db.TeamItems.ToList();
                List<string> teamNames = new List<string>();
                teamNames.Add("All");
                foreach (TeamItem team in teams)
                {
                    teamNames.Add(team.TEAMNAME);
                }
                SelectList TeamNames = new SelectList(teamNames);
                ViewBag.TeamNames = TeamNames;

                SelectList TaskStatus = new SelectList(new string[] {"All", "Open", "Pending", "Closed" });
                ViewBag.TaskStatus = TaskStatus;

               
                ViewBag.DistrictList = MyAccountController.DistrictCategory;

                List<TaskEntry> tasks = _db.TaskEntries.ToList();
                List<string> startDates = new List<string>();
                startDates.Add("All");
                List<string> endDates = new List<string>();
                endDates.Add("All");

                foreach (TaskEntry task in tasks)
                {
                    startDates.Add(task.START.ToString());
                    if (task.STATUS == "Closed") endDates.Add(task.END.ToString());
                }
                ViewBag.StartDates = new SelectList(startDates);
                ViewBag.EndDates = new SelectList(endDates);
                return View();
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }
        }
        public ActionResult Districts()
        {
            if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "REPORTS";
                ViewBag.DistrictList = MyAccountController.DistrictCategory;

                List<TaskEntry> tasks = _db.TaskEntries.ToList();
                List<string> startDates = new List<string>();
                startDates.Add("All");
                List<string> endDates = new List<string>();
                endDates.Add("All");

                foreach (TaskEntry task in tasks)
                {
                    startDates.Add(task.START.ToString());
                    if (task.STATUS == "Closed") endDates.Add(task.END.ToString());
                }
                ViewBag.StartDates = new SelectList(startDates);
                ViewBag.EndDates = new SelectList(endDates);

                return View();
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }
        }
        public ActionResult TrackTask()
        {
            if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "REPORTS";
                List<string> strTasks = new List<string>();
                var TaskItems = _db.TrackInfoTables.ToList();
                //strTasks.Add("All");
                List<string> startDates = new List<string>();
                //startDates.Add("All");
                string taskName = null;
                DateTime dt = new DateTime();
                foreach (TrackInfoTable pt in TaskItems)
                {
                    //if (taskName != pt.taskName )
                    //{
                        taskName = pt.taskName;
                        strTasks.Add(taskName);
                    //}
                    if (!startDates.Contains(pt.date.ToString("yyyy/MM/dd")))
                    {
                        //dt = pt.date;
                        startDates.Add(pt.date.ToString("yyyy/MM/dd"));
                    }

                }
                //mTasks = strTasks;
                ViewBag.TaskList = new SelectList(strTasks);
                ViewBag.Dates = new SelectList(startDates);
                ViewBag.TrackInfos = TaskItems;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }
        }
        [HttpPost]
        public ActionResult getTaskRoutePoints(string taskname)
        {
            if (Request.IsAjaxRequest())
            {
                if (!String.IsNullOrEmpty(taskname))
                {
                    var ptInfo = _db.TrackInfoTables.FirstOrDefault(m => m.taskName == taskname);
                    if (ptInfo != null)
                    {
                        var pts = _db.PointArrayTable.Where(m => m.taskName == taskname);
                        return Json(pts);

                    }
                }
            }
            return Json("");
        }
        [HttpPost]
        public ActionResult getIdPoints(string id)
        {
            if (Request.IsAjaxRequest())
            {
                var ptInfo = _db.TrackInfoTables.FirstOrDefault(m => m.id == int.Parse(id));
                if (ptInfo != null)
                {
                    var pts = _db.PointArrayTable.Where(m => m.taskName == ptInfo.taskName);
                    return Json(pts);
                }

            }
            return Json("");
        }
        [HttpPost]
        public ActionResult getDateRoutePoints(string selectedDate)
        {
            if (Request.IsAjaxRequest())
            {
                //if (!String.IsNullOrEmpty(selectedDate))
                //{
                DateTime sdt = Convert.ToDateTime(selectedDate);
                DateTime edt = sdt.Add(new TimeSpan(23, 59, 59));

                var infos = _db.TrackInfoTables.Where(m => sdt < m.date && m.date < edt).ToList();
                    List<PointsTable[]> dateListPoints = new List<PointsTable[]>() ;
                    foreach (TrackInfoTable info in infos)
                    {
                        var pts = _db.PointArrayTable.Where(m => m.taskName == info.taskName).ToArray();
                        dateListPoints.Add(pts);
                    }
                    return Json(dateListPoints);
                //}
            }
            return Json("");
        }
        [HttpPost]
        public ActionResult delete(string taskname, string txtdate)
        {
            if (!String.IsNullOrEmpty(txtdate))
            {
                DateTime xdt = Convert.ToDateTime(txtdate);
                var infos = _db.TrackInfoTables.Where(m => m.date == xdt);
                foreach (TrackInfoTable info in infos)
                {
                    var pts = _db.PointArrayTable.Where(m => m.taskName == info.taskName);
                    foreach (PointsTable pt in pts)
                    {
                        _db.Entry(pt).State = EntityState.Deleted;
                    }
                    _db.Entry(info).State = EntityState.Deleted;

                }
                _db.SaveChanges();
                return Json("success");
            }
            else if (!String.IsNullOrEmpty(taskname))
            {
                TrackInfoTable info = _db.TrackInfoTables.FirstOrDefault(m => m.taskName == taskname);
                //foreach (TrackInfoTable info in infos)
                //{
                    var pts = _db.PointArrayTable.Where(m => m.taskName == taskname);
                    foreach (PointsTable pt in pts)
                    {
                        _db.Entry(pt).State = EntityState.Deleted;
                    }
                    _db.Entry(info).State = EntityState.Deleted;

                //}
                _db.SaveChanges();
                return Json("success");
            }
            return Json("");
        }
        public ActionResult Delete(int id)
        {
            if (id != null)
            {
                //DateTime xdt = Convert.ToDateTime(txtdate);
                var infos = _db.TrackInfoTables.FirstOrDefault(m => m.id == id);
                if (infos != null)
                {
                    _db.Entry(infos).State = EntityState.Deleted;
                    var pts = _db.PointArrayTable.Where(m => m.taskName == infos.taskName).ToList();
                    foreach (PointsTable pt in pts)
                    {
                        _db.Entry(pt).State = EntityState.Deleted;
                        _db.SaveChanges();
                    }
                    return RedirectToAction("TrackTask");
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult setTable(string teamname, string status, string district, string start, string end)
        {
            if (Request.IsAjaxRequest())
            {
                if (teamname =="All" && status =="All"  && start == "All" && end == "All" && district == "All")
                {
                    var resultTask = (from ta in _db.TaskEntries join te in _db.TeamItems on ta.TEAMNAME equals te.TEAMNAME
                                      select new { ta.TASK, ta.STATUS, ta.START, ta.END, ta.LOCATION, te.DISTRICT, ta.TEAMNAME});
                    
                    return Json(resultTask);
                }
                else
                {
                    var resultTask = (from ta in _db.TaskEntries
                                        join te in _db.TeamItems on ta.TEAMNAME equals te.TEAMNAME
                                        select new { ta.TASK, ta.STATUS, ta.START, ta.END, ta.LOCATION, te.DISTRICT, ta.TEAMNAME });

                    if (teamname != "All")
                    {
                        resultTask = resultTask.Where(ta => ta.TEAMNAME == teamname);
                    }
                    if (status != "All")
                    {
                        resultTask = resultTask.Where(ta => ta.STATUS == status);
                    }
                    if (start != "All")
                    {
                        DateTime starttime = DateTime.Parse(start);
                        resultTask = resultTask.Where(ta => ta.START >= starttime);
                    }
                    if (end != "All")
                    {
                        DateTime endtime = DateTime.Parse(end);
                        resultTask = resultTask.Where(ta => ta.END <= endtime);
                    }
                    if (district != "All")
                    {
                        resultTask = resultTask.Where(ta => ta.DISTRICT.Contains(district));
                    }

                    return Json(resultTask);
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult setDistrictTable(string district, string start, string end)
        {
            if (Request.IsAjaxRequest())
            {
                if (start == "All" && end == "All" && district == "All")
                {
                    var resultTask = (from ta in _db.TaskEntries
                                      join te in _db.TeamItems on ta.TEAMNAME equals te.TEAMNAME
                                      select new { ta.TASK, ta.STATUS, ta.START, ta.END, ta.LOCATION, te.DISTRICT, ta.TEAMNAME });

                    return Json(resultTask);
                }
                //else
                //{
                //    _db.TaskEntries
                //}
                else
                {
                    var resultTask = (from ta in _db.TaskEntries
                                      join te in _db.TeamItems on ta.TEAMNAME equals te.TEAMNAME
                                      select new { ta.TASK, ta.STATUS, ta.START, ta.END, ta.LOCATION, te.DISTRICT, ta.TEAMNAME });

                    if (district != "All")
                    {
                        resultTask = resultTask.Where(ta => ta.DISTRICT.Contains(district));
                    }


                    if (start != "All")
                    {
                        DateTime starttime = DateTime.Parse(start);
                        resultTask = resultTask.Where(ta => ta.START >= starttime);
                    }
                    if (end != "All")
                    {
                        DateTime endtime = DateTime.Parse(end);
                        resultTask = resultTask.Where(ta => ta.END <= endtime);
                    }
                    return Json(resultTask);
                }
            }
            return View();
        }


    }
}
