﻿using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Example {

    public class MvcApplication : HttpApplication {

        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            BundleHelper.RegisterBundles(); // see BundleHelper.cs
        }

    }

}