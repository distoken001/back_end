using deMarketService.Common.Common;
using deMarketService.Common.Model.DataEntityModel;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using deMarketService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("login")]
        public async Task<WebApiResult> login([FromBody] ReqUsersVo req)
        {
            //对签名消息，账号地址三项信息进行认证，判断签名是否有效
            if (!EthereumSignatureVerifier.Verify(req.signature, req.address))
            {
                return new WebApiResult(-1, "signature verification failure");
            }

            var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(req.address) && p.chain.Equals(req.chain));
            if (users == null)
            {
                users = new Common.Model.DataEntityModel.users
                {
                    address = req.address,
                    chain = req.chain,
                    status = 1,
                    create_time = DateTime.Now
                };

                try
                {
                    await _mySqlMasterDbContext.users.AddAsync(users);
                    await _mySqlMasterDbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return new WebApiResult(-1, "database error");
                }
            }
            Claim[] userClaims = ConvertToClaims(users);
            var token = TokenHelper.GenerateToken(StringConstant.secretKey, StringConstant.issuer, StringConstant.audience, 7, userClaims);
            return new WebApiResult(1, data: new { token = token });
        }



        /// <summary>
        /// 看名称好形势-我的订单列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("list")]
        [ProducesResponseType(typeof(orders), 200)]
        public async Task<WebApiResult> list([FromBody] ReqOrdersVo req)
        {
            var list = await _mySqlMasterDbContext.orders.AsTracking().Where(p => p.buyer.Equals(req.buyer) && p.seller.Equals(req.seller) && p.chain_id.Equals(req.chain_id)).ToListAsync();

            return new WebApiResult(1, data : list);
        }



        private Claim[] ConvertToClaims(object obj)
        {
            List<Claim> claims = new List<Claim>();
            if (obj != null)
            {
                PropertyInfo[] properties = obj.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    object value = property.GetValue(obj);
                    if (value != null)
                    {
                        claims.Add(new Claim(property.Name, value.ToString()));
                    }
                }
            }
            return claims.ToArray();
        }
    }
}
