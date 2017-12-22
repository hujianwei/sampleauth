﻿using System;
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
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace LaiCai.Auth.Controllers
{
   
    public class WxController : Controller
    {
        private LaiCai.Auth.Implement.RedisCli _redis = null;
        private string appId = "wx782c26e4c19acffb";
        private IDictionary<string, string> headDict = new Dictionary<string, string>();
        private IRequestHelper _helper = null;

        private string wxDirectory = @"D:\gitcode\auth\LaiCai.Auth\LaiCai.Auth\temp\";

        public WxController(IRequestHelper helper, Implement.RedisCli redis)
        {
            _helper = helper;
            _redis = redis;
            headDict.Add("referer", "https://wx.qq.com/");
            headDict.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:57.0) Gecko/20100101 Firefox/57.0");
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
            var result = await _helper.HttpToServer(url, "", RequestMethod.GET, ContentType.FORM, "", headDict, "utf-8",10,true);
           // System.IO.File.AppendAllText(@"D:\gitcode\auth\LaiCai.Auth\LaiCai.Auth\temp\login.txt",result.Item3);
            var returnStr = result.Item2;
            if (returnStr.IndexOf("window.code=200") != -1)
            {
                var returnUri = this.GetRegexValue(returnStr, "return", "window.redirect_uri=\"(?<return>[^\"]+)\"");
                url = returnUri + "&fun=new";
                result = await _helper.HttpToServer(url, "", RequestMethod.GET, ContentType.FORM, "", headDict, "utf-8",10,true);
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
                    //将响应的头信息的cookie写入文件
                    System.IO.File.WriteAllText($"{wxDirectory}webwxnewloginpage_{wxuin}.txt", result.Item3);

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
            string skey = Server.UrlDecode(Request["skey"]);
            string wxsid = Server.UrlDecode(Request["wxsid"]);
            string wxuin = Server.UrlDecode(Request["wxuin"]);
            string pass_ticket = Server.UrlDecode(Request["pass_ticket"]);
            string deviceid = Request["deviceid"];
            ViewBag.skey = skey;
            ViewBag.wxsid = wxsid;
            ViewBag.wxuin = wxuin;
            ViewBag.pass_ticket = pass_ticket;
            ViewBag.deviceid = deviceid;
            //webwxinit发送的cookie信息
            //mm_lang=zh_CN; MM_WX_NOTIFY_STATE=1; MM_WX_SOUND_STATE=1; refreshTimes=2; wxuin=2864823900; wxsid=uadHbbSWUxNdYnR4; wxloadtime=1513328849; webwx_data_ticket=gSeJTbuiBJuQpkN1EDVbhiBp; webwxuvid=ae2cf6518f801a36bb78a09f0ff3182303919381fde2b068838e2b20f747455b772262627df23877821fb9e0861a2a23; webwx_auth_ticket=CIsBEMX235IOGoABaH1cSXX1mwSFKVZfC3bgf0jlQSumigjTNnxdTcIs5fxYTGEJYQJDtOOhKjod1i/v8vINsaOdL2yElgsRrrfJvySUkGNsxfVTCTSF/1UCBVD3gMtGPx7zoYTF4wCHspj5SMGxpKBIhB3l5O8CSa7FRn9BnjM2If4oi4iiG+10CFk=; login_frequency=1; last_wxuin=2864823900
            var cookieDict = this.ResCookieDictionary(wxuin);
            var reqCookieStr = "";
            foreach(var keyvalue in cookieDict)
            {
                reqCookieStr += $"{keyvalue.Key}={keyvalue.Value};";
            }
            reqCookieStr = reqCookieStr.Substring(0, reqCookieStr.Length - 1);
            headDict.Add("Cookie", reqCookieStr);

            //微信初始化
            string url = $"https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxinit?r={Request["r"]}&lang=zh_CN&pass_ticket={Server.UrlEncode(pass_ticket)}";
            var sendObj = new { BaseRequest =new { DeviceID= deviceid, Sid= wxsid, Skey= skey, Uin= wxuin } };
            var sendObjStr = JsonConvert.SerializeObject(sendObj);
            var result = await _helper.HttpToServer(url, sendObjStr, RequestMethod.POST, ContentType.JSON, "", headDict, "utf-8",10,true);
            //初始化信息写入文件
            System.IO.File.WriteAllText($"{wxDirectory}webwxinit_{wxuin}.txt", result.Item2);

            //获取联系人列表
            url = $"https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxgetcontact?pass_ticket={Server.UrlEncode(pass_ticket)}&r={GetUnixTime()}&seq=0&skey={skey}";

            result = await _helper.HttpToServer(url, "", RequestMethod.GET, ContentType.JSON, "", headDict, "utf-8", 10, true);
            //联系人信息写入文件
            System.IO.File.WriteAllText($"{wxDirectory}webwxgetcontact{wxuin}.txt", result.Item2);

            //发送信息
            var clientId = GetUnixTime().ToString() + "0" + (100 + new Random().Next(0, 899)).ToString();
            url = $"https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxsendmsg?pass_ticket={Server.UrlEncode(pass_ticket)}";
            var sendMsg = new
                            {
                                BaseRequest = new
                                {
                                    DeviceID = deviceid,
                                    Sid = wxsid,
                                    Skey = skey,
                                    Uin = wxuin
                                },
                                Scene = 0,
                                Msg = new
                                {
                                    Type = 1,
                                    Content = "hello",
                                    FromUserName = "@f052fcb699587fe047bfdc9a83de562a",
                                    ToUserName = "@9bcbb744fbe30a11898cbb6b64114cda1179ef02f9866e54dad298f592a2852a",
                                    LocalID = clientId,
                                    ClientMsgId = clientId
                                }
                            };
            var sendMsgStr = JsonConvert.SerializeObject(sendMsg);
            reqCookieStr = "";
            foreach (var keyvalue in cookieDict)
            {
                if (keyvalue.Key == "wxloadtime")
                {
                    reqCookieStr += $"{keyvalue.Key}={GetUnixTime(10) + 2}_expired;";
                }
                else
                {
                    reqCookieStr += $"{keyvalue.Key}={keyvalue.Value};";
                }
            }
            reqCookieStr = reqCookieStr.Substring(0, reqCookieStr.Length - 1);
            if (headDict.ContainsKey("Cookie"))
            {
                headDict["Cookie"] = reqCookieStr;
            }
            else
            {
                headDict.Add("Cookie", reqCookieStr);
            }
            result = await _helper.HttpToServer(url, sendMsgStr, RequestMethod.POST, ContentType.JSON, "", headDict, "utf-8", 10, true);

            return View();
        }

        /// <summary>
        /// 测试
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Test()
        {
            string context = System.IO.File.ReadAllText(Server.MapPath("/temp/webwxinit_2864823900.txt"));
            var initInfo = GetSyncKeyList(context);
            _redis.Set("adfsad", initInfo);
            //var list = ContactList(System.IO.File.ReadAllText(Server.MapPath("/temp/webwxgetcontact2864823900.txt")));
            //Response.Write( JsonConvert.SerializeObject(initInfo) );
            Response.Write(JsonConvert.SerializeObject(initInfo));
            return View();
        }

        /// <summary>
        /// 获取微信初始化对象
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private WxUserInitInfo GetInitInfo(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;
            var jsonObj = JsonConvert.DeserializeObject<JObject>(content);
            if (jsonObj == null || jsonObj["User"] == null)
                return null;
            try
            {
                return JsonConvert.DeserializeObject<WxUserInitInfo>(jsonObj["User"].ToString());
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// 获取微信SyncKey列表
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private IList<WxSyncKey> GetSyncKeyList(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;
            var jsonObj = JsonConvert.DeserializeObject<JObject>(content);
            if (jsonObj == null || jsonObj["SyncKey"] == null)
                return null;
            try
            {
                var syncObj = (JObject)jsonObj["SyncKey"];
                return JsonConvert.DeserializeObject<IList<WxSyncKey>>(syncObj["List"].ToString());
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// 获取微信联系人列表
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private IList<WxContactInfo> ContactList(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;
            var jsonObj = JsonConvert.DeserializeObject<JObject>(content);
            if (jsonObj == null )
                return null;
            try
            {
                IList<WxContactInfo> list = new List<WxContactInfo>();
                var jsonList = jsonObj["MemberList"] as JArray;
                if (jsonList == null || jsonList.Count <= 0)
                    return null;
                foreach(var item in jsonList)
                {
                    try
                    {
                        var str = item.ToString();
;                        list.Add( JsonConvert.DeserializeObject<WxContactInfo>(str) );
                    }
                    catch(Exception e)
                    {

                    }
                }
                return list;
            }
            catch
            {
                return null;
            }
        }



        /// <summary>  
        /// 获取时间戳  13位
        /// </summary>  
        /// <returns></returns>  
        private long GetUnixTime()
        {
            return GetUnixTime(13);
        }

        /// <summary>
        /// 获取unix时间截
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private long GetUnixTime(int length)
        {
            if (length == 13)
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return Convert.ToInt64(ts.TotalSeconds * 1000);
            }
            else if(length==10)
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return Convert.ToInt64(ts.TotalSeconds );
            }
            return 0;
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

        /// <summary>
        /// 根据wxuin返回响应的cookie信息
        /// </summary>
        /// <param name="wxuin"></param>
        /// <returns></returns>
        private IDictionary<string,string> ResCookieDictionary( string wxuin )
        {
            var result = new Dictionary<string, string>();
            result.Add("mm_lang", "zh_CN");
            result.Add("MM_WX_NOTIFY_STATE", "1");
            result.Add("MM_WX_SOUND_STATE", "1");
            string filePath = $"{wxDirectory}webwxnewloginpage_{wxuin}.txt";
            var content = System.IO.File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(content))
            {
                return result;
            }
            result.Add("wxuin", this.GetRegexValue(content, "wxuin", "wxuin=(?<wxuin>[^;]+);"));
            result.Add("wxsid", this.GetRegexValue(content, "wxsid", "wxsid=(?<wxsid>[^;]+);"));
            result.Add("wxloadtime", this.GetRegexValue(content, "wxloadtime", "wxloadtime=(?<wxloadtime>[^;]+);"));
            result.Add("webwx_data_ticket", this.GetRegexValue(content, "webwx_data_ticket", "webwx_data_ticket=(?<webwx_data_ticket>[^;]+);"));
            result.Add("webwxuvid", this.GetRegexValue(content, "webwxuvid", "webwxuvid=(?<webwxuvid>[^;]+);"));
            result.Add("webwx_auth_ticket", this.GetRegexValue(content, "webwx_auth_ticket", "webwx_auth_ticket=(?<webwx_auth_ticket>[^;]+);"));
            return result;
        }

        
    }

    /*
     微信登陆信息
     "User": {
"Uin": 2864823900,
"UserName": "@f052fcb699587fe047bfdc9a83de562a",
"NickName": "胡建威",
"HeadImgUrl": "/cgi-bin/mmwebwx-bin/webwxgeticon?seq=1265359010&username=@f052fcb699587fe047bfdc9a83de562a&skey=@crypt_698622b_618bd542c7347492def9869140ed6db9",
"RemarkName": "",
"PYInitial": "",
"PYQuanPin": "",
"RemarkPYInitial": "",
"RemarkPYQuanPin": "",
"HideInputBarFlag": 0,
"StarFriend": 0,
"Sex": 1,
"Signature": "梦想从未实现",
"AppAccountFlag": 0,
"VerifyFlag": 0,
"ContactFlag": 0,
"WebWxPluginSwitch": 0,
"HeadImgFlag": 1,
"SnsFlag": 17
}
     * */
    public class WxUserInitInfo
    {
        /// <summary>
        /// 微信编号，不变
        /// </summary>
        public string Uin { set; get; }
        /// <summary>
        /// 用户名，每次登陆会变化
        /// </summary>
        public string UserName { set; get; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { set; get; }
        /// <summary>
        /// 头像地址
        /// </summary>
        public string HeadImgUrl { set; get; }
        /// <summary>
        /// 备注名称
        /// </summary>
        public string RemarkName { set; get; }
        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { set; get; }
        /// <summary>
        /// 性别,0:未知，公众号未知，1：男，2：女
        /// </summary>
        public int Sex { set; get; }
    }

    /// <summary>
    /// 微信联系人信息
    /// </summary>
    public class WxContactInfo: WxUserInitInfo
    {
        /// <summary>
        /// 会员数,>0微信群
        /// </summary>
        public int MemberCount { set; get; }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { set; get; }

        /// <summary>
        /// 城市 
        /// </summary>
        public string City { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public int AttrStatus { set; get; }

    }

    /// <summary>
    /// 微信sysncKey信息
    /// </summary>
    public class WxSyncKey
    {
        public int Key { set; get; }
        public int Val { set; get; }
    }
}