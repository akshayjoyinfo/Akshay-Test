using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using SingalRMVCApp.Models;

namespace SingalRMVCApp
{
    public class ScopeHub : Hub
    {
        private static Dictionary<string, List<string>> dxUsersConnections = new Dictionary<string, List<string>>();
        public void Hello()
        {
            Clients.All.hello();
        }
        public static void Show(Scope obj)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ScopeHub>();
            //context.Clients.All.updateMessages();
            if (dxUsersConnections.ContainsKey(obj.UserId.ToString()))
            {
                List<string> availConnections = dxUsersConnections[obj.UserId.ToString()];
                foreach (var connection in availConnections)
                {
                    string jsonObject = JsonConvert.SerializeObject(obj);
                    context.Clients.Client(connection).updateMessages(jsonObject);
                     
                }
                ScopeRepository.UpdateScopeStatus(obj);   
            }
        }
        public void RegisterMeAs(string userId)
        {
            bool alreadyExists = false;
            if (dxUsersConnections.Count == 0)
            {
                var connectionIds = new List<string> {Context.ConnectionId};
                dxUsersConnections.Add(userId, connectionIds);
            }
            else
            {
                foreach (string key in dxUsersConnections.Keys)
                {
                    if (key == userId)
                    {
                        List<string> existingConnections = dxUsersConnections[key];
                        existingConnections.Add(Context.ConnectionId);
                        dxUsersConnections[key] = existingConnections;
                        alreadyExists = true;
                        break;
                    }
                }
                if (!alreadyExists)
                {
                    var connectionIds = new List<string> { Context.ConnectionId };
                    dxUsersConnections.Add(userId, connectionIds);
                }
            }
        }
    }

    public class DeltaXUserIdProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
  
            string userId = "0";

            if (request.User.Identity.Name.ToLower().Equals("akshayjoy"))
                userId = "4";
            else if (request.User.Identity.Name.ToLower().Equals("hskrishna29"))
                userId = "5";
            else if (request.User.Identity.Name.ToLower().Equals("ayushjoyinfo"))
                userId = "6";

            return userId;
        }
    }
}