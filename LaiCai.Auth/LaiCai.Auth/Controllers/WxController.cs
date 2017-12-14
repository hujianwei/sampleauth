using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
using System.Xml;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using LaiCai.Auth.IServices;
using LaiCai.Auth.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace LaiCai.Auth.Controllers
{
    public class WxController : Controller
    {
        private string appId = "wx782c26e4c19acffb";
        private IDictionary<string, string> headDict = new Dictionary<string, string>();
        private IRequestHelper _helper = null;

        public WxController(IRequestHelper helper)
        {
            _helper = helper;
            
            headDict.Add("referer", "https://wx.qq.com/");
        }

        /// <summary>
        /// 抛码
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {

            string redirect_uri = "https%3A%2F%2Fwx.qq.com%2Fcgi-bin%2Fmmwebwx-bin%2Fwebwxnewloginpage";
            string url = $"https://login.wx.qq.com/jslogin?appid={appId}&redirect_uri={redirect_uri}&fun=new&lang=zh_CN&_={GetUnixTime()}";
            var result = await _helper.HttpToServer(url, "", RequestMethod.POST, ContentType.FORM, "", headDict, "utf-8");
            string uuid = this.GetRegexValue(result.Item2, "uuid", "window.QRLogin.uuid = \"(?<uuid>[^\"]+)\"");
            ViewBag.UUID = uuid;
            ViewBag.QrUrl = $"https://login.weixin.qq.com/qrcode/{uuid}";
            return View();
        }
        /// <summary>
        /// 扫码成功后
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Scan()
        {
            //window.code=200; window.redirect_uri="https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxnewloginpage?ticket=AdM4WVLzwWISJIm0ButENtgk@qrticket_0&uuid=geTNCyj69w==&lang=zh_CN&scan=1512964448";
            string uuid = Request["uuid"];
            string url = $"https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?tip=1&uuid={uuid}&_={GetUnixTime()}";
            var result = await _helper.HttpToServer(url, "", RequestMethod.GET, ContentType.FORM, "", headDict, "utf-8");
            var returnStr = result.Item2;
            if (returnStr.IndexOf("window.code=200") != -1)
            {
                var returnUri = this.GetRegexValue(returnStr, "return", "window.redirect_uri=\"(?<return>[^\"]+)\"");
                url = returnUri + "&fun=new";
                result = await _helper.HttpToServer(url, "", RequestMethod.GET, ContentType.FORM, "", headDict, "utf-8");
                if (result.Item1 == 200)
                {
                    DataSet ds = new DataSet();
                    var stream = new StringReader(result.Item2);
                    var reader = new XmlTextReader(stream);
                    ds.ReadXml(reader);
                    var dt = ds.Tables[0];
                    if(dt.Rows[0]["ret"].ToString()!="0")
                        return Json(new { code = 0, msg = "读取webwxnewloginpage接口失败" });
                    var skey = dt.Rows[0]["skey"].ToString();
                    var wxsid = dt.Rows[0]["wxsid"].ToString();
                    var wxuin = dt.Rows[0]["wxuin"].ToString();
                    var pass_ticket = dt.Rows[0]["pass_ticket"].ToString();
                    var isgrayscale = dt.Rows[0]["isgrayscale"].ToString();
                    //deviceid 16位字符串,生成规则请查看https://res.wx.qq.com/a/wx_fed/webwx/res/static/js/index_e01fd8a.js
                    var deviceid = "e" + new Random().NextDouble().ToString().Substring(2);
                    if (deviceid.Length > 16)
                        deviceid = deviceid.Substring(0, 16);
                    else
                    {
                        deviceid = deviceid + DateTime.Now.Ticks.ToString().Reverse().ToString().Substring(0, 16 - deviceid.Length);
                    }
                    return Json(new { code = 1, msg = new { skey= Server.UrlEncode(skey), wxsid= Server.UrlEncode(wxsid), wxuin=Server.UrlEncode(wxuin), pass_ticket= Server.UrlEncode( pass_ticket), deviceid=deviceid } },JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { code = 0, msg = "读取webwxnewloginpage接口失败" }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json(new{code=0,msg=returnStr});
            }
        }

        public async Task<ActionResult> Contack()
        {
            
            return View();
        }

        public async Task<ActionResult> Info()
        {
            ViewBag.skey = Request["skey"];
            ViewBag.wxsid = Request["wxsid"];
            ViewBag.wxuin = Request["wxuin"];
            ViewBag.pass_ticket = Request["pass_ticket"];
            ViewBag.deviceid = Request["deviceid"];
            string url = $"https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxgetcontact?lang=zh_CN&pass_ticket=09xi5YD9GIoyVjne2z9IBYTR%252Bo7PS17ZoXcNd5OD%252F9L5vdIQxAtIaCRK84k7lPJs&r=1513239871126&seq=0&skey=@crypt_698622b_18a6efaf91d52aa6f616b99d21e46b97";
            var result = await _helper.HttpToServer(url, "", RequestMethod.GET, ContentType.FORM, "", headDict, "utf-8");
            Response.Write(result.Item2);
            return View();
        }







        /// <summary>  
        /// 获取时间戳  13位
        /// </summary>  
        /// <returns></returns>  
        private long GetUnixTime()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds * 1000);
        }

        /// <summary>
        /// 获取正则表达式内容
        /// </summary>
        /// <param name="inputStr"></param>
        /// <param name="groupName"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private  string GetRegexValue(string inputStr, string groupName, string pattern)
        {
            var match = new Regex(pattern).Match(inputStr);
            if (match == null)
                return string.Empty;
            if (string.IsNullOrEmpty(groupName))
                return match.Value;
            else
                return match.Groups[groupName].Value;
        }
    }
}