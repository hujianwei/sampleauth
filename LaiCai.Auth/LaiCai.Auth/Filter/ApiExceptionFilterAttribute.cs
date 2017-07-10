using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LaiCai.Auth.IServices;
using System.Web.Http.Filters;

namespace LaiCai.Auth.Filter
{
    public class ApiExceptionFilterAttribute:ExceptionFilterAttribute
    {
        public  ILog Log { set; get; }

        public ApiExceptionFilterAttribute() { }


        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            Log.Error( actionExecutedContext.Exception);
            base.OnException(actionExecutedContext);
        }
    }
}