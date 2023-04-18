using deMarketService.Common.Common;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using deMarketService.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        MySqlMasterDbContext _mySqlMasterDbContext;

        public UserController(MySqlMasterDbContext mySqlMasterDbContext)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
        }

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
            //EthereumSignatureVerifier.Verify(req.signature, MESSAGE, req.address);
            var token = TokenHelper.GenerateToken(StringConstant.secretKey, StringConstant.issuer, StringConstant.audience, 180, new Claim("username", "sean"));
            return new WebApiResult(1, token);
        }



        /// <summary>
        /// 看名称好形势-我的订单列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("list")]
        public async Task<WebApiResult> list([FromBody] ReqUsersVo req)
        {
            //对签名消息，原始消息，账号地址三项信息进行认证，判断签名是否有效
            EthereumSignatureVerifier.Verify(req.signature, MESSAGE, req.address);
            var token = TokenHelper.GenerateToken(StringConstant.secretKey, StringConstant.issuer, StringConstant.audience, 180, new Claim("username", "sean"));
            return new WebApiResult(1, token);
        }
    }
}
