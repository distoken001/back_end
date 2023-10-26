using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.HttpApiModel.ResponseModel
{
    public class WebApiResult
    {
        public WebApiResult() { }
        public WebApiResult(int result, string msg = "") {
            ActionResult = result;
            Message = msg;
        }
        public WebApiResult(int result, string msg = "", object data = null) {
            ActionResult = result;
            Message = msg;
            Data = data;
        }

        public int ActionResult { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
