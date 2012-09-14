using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.MvcIntegration;
using Raven.Database.Server;
using StackExchange.Profiling;

namespace SixDegrees
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_BeginRequest()
        {
            if (Request.IsLocal) { MiniProfiler.Start(); } //or any number of other checks, up to you 
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Stop(); //stop as early as you can, even earlier with MvcMiniProfiler.MiniProfiler.Stop(discardResults: true);
        }


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
            InitializeDocumentStore();
        }

        protected void Application_End()
        {
            if (DocumentStore != null)
            {
                DocumentStore.Dispose();
            }
        }

        public static IDocumentStore DocumentStore { get; private set; }

        private static void InitializeDocumentStore()
        {
            if (DocumentStore != null) return; // prevent misuse

            var portnum = int.Parse(ConfigurationManager.AppSettings["EmbeddedRavenServerPort"]);

            NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(portnum);
            DocumentStore = new EmbeddableDocumentStore
            {
                UseEmbeddedHttpServer = true,
                ConnectionStringName = "RavenDB",
                Configuration = { Port = portnum }
            }
            .Initialize();

            RavenProfiler.InitializeFor(DocumentStore,
                //Fields to filter out of the output
                                        "Email", "HashedPassword", "AkismetKey", "GoogleAnalyticsKey", "ShowPostEvenIfPrivate",
                                        "PasswordSalt", "UserHostAddress");
        }

    }
}