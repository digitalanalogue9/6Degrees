using System;
using System.Web.Mvc;
using System.Xml.Linq;
using Raven.Database.Config;
using Raven.Database.Server;
using SixDegrees.Mvc;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using StackExchange.Profiling;

namespace SixDegrees.Controllers
{
    public abstract class RavenController : Controller
    {
        public new IDocumentSession Session { get; set; }
        private static IDisposable _step;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Session = MvcApplication.DocumentStore.OpenSession();
            var profiler = MiniProfiler.Current; // it's ok if this is null
            _step = profiler.Step(string.Format("{0} {1}",
                filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                filterContext.ActionDescriptor.ActionName));

        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            using (Session)
            {
                if (Session != null && filterContext.Exception == null)
                    Session.SaveChanges();
            }
            _step.Dispose();
        }
    }
}