using System;
using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using CommonLibrary.Model.DataEntityModel;
using DeMarketAPI.Common.Model.HttpApiModel.RequestModel;
using DeMarketAPI.Common.Model.HttpApiModel.ResponseModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DeMarketAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeBoxController :BaseController
	{
        [HttpPost("receivemessage")]
        [AllowAnonymous]
        public async Task<JsonResult> ReceiveMessage([FromBody] GetCardTypeListRequest req)
        {
          
            return Json(new WebApiResult(1, "成功" ));
        }
    }
}

