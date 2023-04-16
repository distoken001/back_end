using deMarketService.Common.Common;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        MySqlSlaveDbContext sqlSlaveDbContext;
        MySqlSlaveDbContext MySqlMasterDbContext;

        private static string MESSAGE = "This signature is used to verify your identity.";

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("login")]
        public async Task<WebApiResult> login([FromBody] ReqUsersVo req)
        {
            //对签名消息，原始消息，账号地址三项信息进行认证，判断签名是否有效
            EthereumSignatureVerifier.Verify(req.signature, MESSAGE, req.address);
            return new WebApiResult(1, "");
        }
    }
}
