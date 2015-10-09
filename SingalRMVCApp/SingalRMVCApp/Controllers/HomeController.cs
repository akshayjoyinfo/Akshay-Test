using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.AspNet.SignalR;
using SingalRMVCApp.Models;

namespace SingalRMVCApp.Controllers
{
    
    public class HomeController : Controller
    {
        
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetScopes(string uId)
        {
            IEnumerable<Scope> listScopes = GetData(uId);
            IEnumerable<Scope> listUserScopes = listScopes.Where(x => x.UserId == Convert.ToInt32(uId)).ToList();   
            return Json(listUserScopes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public IEnumerable<Scope> GetData(string userId)
        {
            List<Scope> listScopes = new List<Scope>();
            using (
                var connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                connection.Open();
                using (
                    SqlCommand command =
                        new SqlCommand(@"SELECT Id,ScopeTypeId,ScopeStatusId,UpdatedAt,UserId
               FROM [Foundation].[Scopes] WHERE ScopeStatusId = 8 AND (IsNotified =0 OR IsNotified is NULL)", connection))
                {
                    // Make sure the command object does not already have
                    // a notification object associated with it.
                    command.Notification = null;

                    SqlDependency dependency = new SqlDependency(command);
                    dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);
                        
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        listScopes= reader.Cast<IDataRecord>()
                            .Select(x => new Scope()
                            {
                                Id = x.GetString(0),
                                ScopeTypeId = x.GetInt32(1),
                                ScopeStatusId = x.GetInt32(2),
                                UpdatedAt = DateTime.Now,
                                UserId =  x.GetInt32(4),
                            }).ToList();

                        
                        return listScopes;
                    }
                }
            }

        }

        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            SqlNotificationInfo s = e.Info;
            List<Scope> listScopes = ScopeRepository.GetAllCompletedScopes();
            foreach (Scope obj in listScopes)
            {
                ScopeHub.Show(obj);
            }
        }

    }
}