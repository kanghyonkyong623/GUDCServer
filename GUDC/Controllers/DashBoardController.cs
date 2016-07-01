using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using GUDC.Models;

namespace GUDC.Controllers
{
    public class DashBoardController : Controller
    {
        //
        // GET: /DashBoard/
        GUDCContext _db = new GUDCContext();
        public ActionResult Index()
        {
            
           if (Session["UserName"] != null)
            {
                ViewBag.ActivePage = "DASHBOARD";
                ViewBag.TaskEntries = _db.TaskEntries.ToList();
                ViewBag.TeamItems = _db.TeamItems.ToList();
                ViewBag.TeamMemberEntries = _db.TeamMemberEntries.ToList();
                var taskOpens = _db.TaskEntries.Where(m => m.STATUS == "Open").ToList();
                ViewBag.CountTaskOpen = taskOpens.Count;
                var taskCloses = _db.TaskEntries.Where(m => m.STATUS == "Closed").ToList();
                ViewBag.CountTaskClose = taskCloses.Count;
                var taskPendings = _db.TaskEntries.Where(m => m.STATUS == "Pending").ToList();
                ViewBag.CountTaskPending = taskPendings.Count;
                var teamFrees = _db.TeamItems.Where(m => m.TEAMSTATUS == "free").ToList();
                ViewBag.CountTeamFree = teamFrees.Count;
                var teamOnMission = _db.TeamItems.Where(m => m.TEAMSTATUS == "on mission").ToList();
                ViewBag.CountTeamOnMission = teamOnMission.Count;
                var teamOutService = _db.TeamItems.Where(m => m.TEAMSTATUS == "out of service").ToList();
                ViewBag.CountTeamOutService = teamOutService.Count;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "MyAccount");
            }
        }

        [HttpPost]
        public ActionResult readCsv()
        {
            if (Request.IsAjaxRequest())
            {
                var asd = Request.PhysicalApplicationPath;
                string[] lines = System.IO.File.ReadAllLines(asd + @"/App_Data/teams.csv");

                return Json(lines);
                
            }
            return View();
        }

        [HttpPost]
        public ActionResult readTeamLocation()
        {
            if (Request.IsAjaxRequest())
            {
                var asd = Request.PhysicalApplicationPath;
                var TeamItems = _db.TeamItems.ToList();

                return Json(TeamItems);

            }
            return View();
        }

    }
}
