using Microsoft.Owin.Cors;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(SignalR.Startup))]

namespace SignalR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HubConfiguration hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableDetailedErrors = true;
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR(hubConfiguration);
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            // Configure Databases at Startup

            /* Set the following tables to an initialized state at startup
                sashaSessions (empty)
                chatSessions (empty)
                chatHelpers (connectionStatus to notConnected)
                Database.InitializeTables();
            */
        }
    }
}
