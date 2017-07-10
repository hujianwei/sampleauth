
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Web.Http.Results;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LaiCai.Auth.Filter;
using System.Net;
using System.Web.Http.Controllers;

namespace LaiCai.Auth.Controllers
{
    [ApiExceptionFilter]
    public class BaseApiController : ApiController
    {
        protected new JsonResult<T> Json<T>(T content)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            jsetting.NullValueHandling = NullValueHandling.Ignore;
            var json = JsonConvert.SerializeObject(content, Formatting.Indented, jsetting);
            return new JsonResult<T>(content, jsetting, Encoding.Default, this.Request);
            
        }


    }

    public class CustomActionResult : IHttpActionResult
    {
        object _value;
        HttpRequestMessage _request;
        private int _code;
        private string _msg;



        /// <summary>
        /// 响应的status
        /// </summary>
        public HttpStatusCode statusCode = HttpStatusCode.OK;
        public CustomActionResult(object value, HttpRequestMessage request)
        {
            _value = value;
            _request = request;
        }

        public CustomActionResult(int code,string message,object value, HttpRequestMessage request)
        {
            _code = code;
            _msg = message;
            _value = value;
            _request = request;
        }

        public CustomActionResult(int code, string message, object value, HttpStatusCode status, HttpRequestMessage request)
        {
            _code = code;
            _msg = message;
            _value = value;
            _request = request;
            statusCode = status;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            jsetting.NullValueHandling = NullValueHandling.Ignore;
            jsetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            string json = "";
            if (_code == 200||_code==0)
                json = JsonConvert.SerializeObject(_value, Formatting.Indented, jsetting);
            else
                json = JsonConvert.SerializeObject(new JosnReturnObject(_code, _msg, _value), Formatting.Indented, jsetting);
            var response = new HttpResponseMessage()
            {
                Content = new StringContent(json),
                RequestMessage = _request
            };
            response.StatusCode = statusCode;
            
            return Task.FromResult(response);
        }
    }

    public class JosnReturnObject
    {
        public int code;
        public string msg;
        public object result;

        public JosnReturnObject(int code,string msg,object result)
        {
            this.code = code;
            this.msg = msg;
            this.result = result;
        }

    }

}
