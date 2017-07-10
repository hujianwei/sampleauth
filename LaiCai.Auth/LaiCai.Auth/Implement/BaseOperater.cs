using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LaiCai.Auth.IServices;
using LaiCai.Auth.Models;

namespace LaiCai.Auth.Implement
{
    public abstract class BaseOperater
    {
        public ICache Cache { set; get; }
        public ILog Log { set; get; }

       public BaseOperater()
        {
        }

    }
}