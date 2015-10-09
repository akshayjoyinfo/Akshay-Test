using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SingalRMVCApp.Startup))]
namespace SingalRMVCApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new DeltaXUserIdProvider());
            app.MapSignalR();
        }
    }
}
