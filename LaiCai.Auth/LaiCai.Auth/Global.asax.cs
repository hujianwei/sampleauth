using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Reflection;
using Autofac;
using Autofac.Integration;
using Autofac.Configuration;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Xml;
using LaiCai.Auth.Filter;

namespace LaiCai.Auth
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static System.Web.Mvc.IDependencyResolver mvcResolver;
        public static System.Web.Http.Dependencies.IDependencyResolver apiResolver;
        protected void Application_Start()
        {
            var builder = new ContainerBuilder();
            SetupResolveRules(builder);
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());


            builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);

            var container = builder.Build();


            //mvc resolver 
            mvcResolver = new AutofacDependencyResolver(container);
            DependencyResolver.SetResolver(mvcResolver);
            //webapi resolver
            var webApiResolver = new AutofacWebApiDependencyResolver(container);
            apiResolver = webApiResolver;
            GlobalConfiguration.Configuration.DependencyResolver = webApiResolver;


            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            //读取log4net配置
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(Server.MapPath("~/Web.config")));
        }


        private void SetupResolveRules(ContainerBuilder builder)
        {
            var config = new ConfigurationBuilder();
            config.AddXmlFile("autofac.xml");
            var module = new ConfigurationModule(config.Build());
            builder.RegisterModule(module);
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(DependencyResolver.Current.GetService<ApiExceptionFilterAttribute>());
        }
    }
}
