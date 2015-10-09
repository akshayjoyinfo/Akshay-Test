using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SingalRMVCApp.Models
{
    public class BaseEntity
    {

        [DataType(DataType.DateTime)]
        public DateTime? CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }
    }

    public class Scope : BaseEntity
    {
        public string Id { set; get; }
        public int AgencyId { set; get; }
        public int BusinessProfileId { set; get; }

        public string Details { get; set; }

        public int ScopeTypeId { get; set; }
        public int ScopeStatusId { get; set; }
        public int UserId { get; set; }

        public bool IsNotified { get; set; }

        public string TaskArguments { get; set; }
    }

    public class ScopeRepository
    {
        public static List<Scope> GetAllCompletedScopes()
        {
            using (
                var connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                connection.Open();
                using (
                    SqlCommand command =
                        new SqlCommand(@"SELECT Id,ScopeTypeId,ScopeStatusId,UpdatedAt,UserId,ScopeTypeId
               FROM [Foundation].[Scopes] WHERE ScopeStatusId = 8 AND (IsNotified =0 OR IsNotified is NULL)", connection)
                    )
                {

                    command.Notification = null;

                    SqlDependency dependency = new SqlDependency(command);
                    dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);
                        

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var reader = command.ExecuteReader())
                        return reader.Cast<IDataRecord>()
                            .Select(x => new Scope()
                            {
                                Id = x.GetString(0),
                                ScopeTypeId = x.GetInt32(1),
                                ScopeStatusId = x.GetInt32(2),
                                UpdatedAt = DateTime.Now,
                                UserId = x.GetInt32(4)
                            }).ToList();

                }
            }
        }

        private static void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            SqlNotificationInfo s = e.Info;
            List<Scope> listScopes = ScopeRepository.GetAllCompletedScopes();
            foreach (Scope obj in listScopes)
            {
                ScopeHub.Show(obj);
            }
        }


        public static bool UpdateScopeStatus(Scope scopeObj)
        {
            bool status = false;
            using (
                var connection =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {

                connection.Open();
                using (
                    SqlCommand command =
                        new SqlCommand(@"UPDATE Foundation.Scopes SET IsNotified =1 where Id = @ScopeId", connection))
                {

                    try
                    {
                        if (connection.State == ConnectionState.Closed)
                            connection.Open();

                        command.Parameters.AddWithValue("@ScopeId", scopeObj.Id);
                        command.ExecuteNonQuery();
                        status = true;
                    }
                    catch (Exception exp)
                    {
                        status = false;
                    }
                }
            }
            return status;
        }
    }
}



