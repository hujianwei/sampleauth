﻿using System.Web;
using System.Web.Mvc;

namespace LaiCai.Auth
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}