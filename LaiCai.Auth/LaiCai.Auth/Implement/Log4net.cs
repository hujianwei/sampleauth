using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using LaiCai.Auth.IServices;
using LaiCai.Auth.Models;


namespace LaiCai.Auth.Implement
{
    public class Log4net:ILog
    {

        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Error(object obj)
        {
            logger.Error(obj);
        }

        public void Fatal(object obj)
        {
            logger.Fatal(obj);
        }

        public void Info(object obj)
        {
            logger.Info(obj);
        }

        public void Warn(object obj)
        {
            logger.Warn(obj);
        }
    }
}