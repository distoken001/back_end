using deMarketService.Common.Common;
using deMarketService.Common.Model.DataEntityModel;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using deMarketService.Model;
using deMarketService.Services;
using deMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;

        public UserController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        public async Task<WebApiResult> login([FromBody] ReqUsersVo req)
        {
            //对签名消息，账号地址三项信息进行认证，判断签名是否有效
            if (!EthereumSignatureVerifier.Verify(req.signature, req.address))
            {
                return new WebApiResult(-1, "signature verification failure");
            }

            var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(req.address) && p.chain_id == req.chain_id);
            if (users == null)
            {
                users = new Common.Model.DataEntityModel.users
                {
                    address = req.address,
                    status = 1,
                    create_time = DateTime.Now,
                    chain_id =req.chain_id
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
            var token = TokenHelper.GenerateToken(StringConstant.secretKey, StringConstant.issuer, StringConstant.audience, 1, userClaims);
            return new WebApiResult(1, data: new LoginResponse { token = token, avatar = users.avatar, nick_name = users.nick_name });
        }

        /// <summary>
        /// 刷新token接口
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        public async Task<WebApiResult> refresh([FromBody] RefreshRequest req)
        {
            //对签名消息，账号地址三项信息进行认证，判断签名是否有效


            var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(this.CurrentLoginAddress) && p.chain_id == req.chain_id);
            if (users == null)
            {
                users = new Common.Model.DataEntityModel.users
                {
                    address = this.CurrentLoginAddress,
                    status = 1,
                    create_time = DateTime.Now,
                    chain_id = req.chain_id
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
                var token = "";
                users.chain_id = req.chain_id;
                Claim[] userClaims = ConvertToClaims(users);
                token = TokenHelper.GenerateToken(StringConstant.secretKey, StringConstant.issuer, StringConstant.audience, 7, userClaims);
            
            return new WebApiResult(1, data: new LoginResponse { token = token, avatar = users.avatar, nick_name = users.nick_name });
        }



        /// <summary>
        /// 看名称好形势-我的订单列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("list")]
        [ProducesResponseType(typeof(OrdersResponse), 200)]
        public async Task<WebApiResult> list([FromBody] ReqOrdersVo req)
        {
            if (!string.IsNullOrEmpty(req.buyer))
            {
                var list = await _mySqlMasterDbContext.orders.AsTracking().Where(p => p.buyer.Equals(req.buyer) && p.chain_id == CurrentLoginChain).ToListAsync();
                var viewList = AutoMapperHelper.MapDbEntityToDTO<orders, OrdersResponse>(list);
                return new WebApiResult(1, data: viewList);
            }
            else if (!string.IsNullOrEmpty(req.seller))
            {
                var list = await _mySqlMasterDbContext.orders.AsTracking().Where(p => p.seller.Equals(req.seller) && p.chain_id == CurrentLoginChain).ToListAsync();
                var viewList = AutoMapperHelper.MapDbEntityToDTO<orders, OrdersResponse>(list);
                return new WebApiResult(1, data: viewList);
            }
            return new WebApiResult(1, "");
        }

        /// <summary>
        /// 我的详情
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("detail")]
        [ProducesResponseType(typeof(UsersResponse), 200)]
        public async Task<WebApiResult> detail([FromBody] ReqOrdersVo req)
        {
            var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(this.CurrentLoginAddress) );

            return new WebApiResult(1, data: users);
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("edit/user")]
        public async Task<WebApiResult> EditUser([FromBody] EditUserCommand command)
        {
            var user = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(this.CurrentLoginAddress) && p.chain_id == this.CurrentLoginChain);
            user.nick_name = command.NickName;
            user.avatar = command.Avatar;
            await _mySqlMasterDbContext.SaveChangesAsync();
            return new WebApiResult(1, "修改用户", true);
        }

        private byte[] ToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
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
