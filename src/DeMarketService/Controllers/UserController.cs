using deMarketService.Common.Common;
using deMarketService.Common.Model;
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
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using TencentCloud.Ckafka.V20190819.Models;

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
            if (string.IsNullOrEmpty(req.parentAddress))
            {
                req.parentAddress = null;
            }
            //对签名消息，账号地址三项信息进行认证，判断签名是否有效
            if (!EthereumSignatureVerifier.Verify(req.signature, req.address))
            {
                return new WebApiResult(-1, "signature verification failure");
            }
            // var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(req.address) && p.chain_id == req.chain_id);
            var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(req.address, StringComparison.OrdinalIgnoreCase));
            if (users == null)
            {
                users = new Common.Model.DataEntityModel.users
                {
                    address = req.address,
                    status = 1,
                    create_time = DateTime.Now,
                    update_time = DateTime.Now,
                    parent_address = req.parentAddress,
                };
                try
                {
                    await _mySqlMasterDbContext.users.AddAsync(users);
                    await _mySqlMasterDbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return new WebApiResult(-1, "database error" + ex);
                }
            }
            Claim[] userClaims = ConvertToClaims(users);
            var token = TokenHelper.GenerateToken(StringConstant.secretKey, StringConstant.issuer, StringConstant.audience, 365, userClaims);
            return new WebApiResult(1, "登录成功", new LoginResponse { token = token, avatar = users.avatar, nick_name = users.nick_name, email = users.email });
        }
        /// <summary>
        /// 重置ip
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("reset")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        public async Task<WebApiResult> reset()
        {
            var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            if (users == null)
            {

            }
            else
            {
                users.ip = GetClientIP();
                await _mySqlMasterDbContext.SaveChangesAsync();
            }
            return new WebApiResult(1, "成功");
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
            var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));

            return new WebApiResult(1, data: users);
        }
        /// <summary>
        /// 被邀请人列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpGet("invite/list")]
        [ProducesResponseType(typeof(string), 200)]
        public WebApiResult invitelist([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            var usersAll = _mySqlMasterDbContext.users.AsNoTracking().Where(p => p.parent_address.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            var totalCount = usersAll.Count();
            var list = usersAll.OrderByDescending(p => p.create_time).Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(a => a.address).ToList();
            var res = new PagedModel<string>(totalCount, list);
            return new WebApiResult(1, "获取成功", res);
        }

        /// <summary>
        /// 当前登录人的佣金明细
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpGet("inviteBates/list")]
        [ProducesResponseType(typeof(List<InviterRebatesItemReponse>), 200)]
        public WebApiResult GetinviteRebateList([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            var bates = _mySqlMasterDbContext.inviter_rebates.AsNoTracking().Where(p => p.inviter_address.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            var totalCount = bates.Count();
            var list = bates.OrderByDescending(p => p.create_time).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<inviter_rebates, InviterRebatesItemReponse>(list);

            var res = new PagedModel<InviterRebatesItemReponse>(totalCount, viewList);
            return new WebApiResult(1, "获取成功", res);
        }

        /// <summary>
        /// 当前登录人佣金汇总列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("inviteBates/collect")]
        public WebApiResult GetInviteRebateTotalList()
        {
            var res = new List<string>();
            var bates = _mySqlMasterDbContext.inviter_rebates.AsNoTracking().Where(p => p.inviter_address.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            if (bates.Count() <= 0)
                return new WebApiResult(1, "暂无数据", res);

            var group = bates.ToList().GroupBy(p => p.token_name);
            foreach (var item in group)
            {
                res.Add($"{item.Key}:{item.Sum(p => p.amount)}");
            }
            return new WebApiResult(1, "获取成功", res);
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("edit/user")]
        public async Task<WebApiResult> EditUser([FromBody] EditUserCommand command)
        {
            var user = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(this.CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            user.avatar = command.Avatar;
            await _mySqlMasterDbContext.SaveChangesAsync();
            return new WebApiResult(1, "修改用户", true);
        }
        /// <summary>
        /// 修改昵称
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("edit/usernick")]
        public async Task<WebApiResult> EditUserNick([FromBody] EditUserNickCommand command)
        {
            if (command.NickName.Contains("DeMarket", StringComparison.OrdinalIgnoreCase) || command.NickName.Contains("德玛", StringComparison.OrdinalIgnoreCase) || command.NickName.Contains("黑名单", StringComparison.OrdinalIgnoreCase))
            {
                return new WebApiResult(-1, "不能包含官方敏感词汇");
            }
            var length = command.NickName.Length;
            if (length > 15)
            {
                return new WebApiResult(-1, "您输入的昵称过长");
            }
            command.NickName = string.IsNullOrEmpty(command.NickName) ? null : command.NickName;
            if (command.NickName != null)
            {
                var userNick = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.nick_name.Equals(command.NickName, StringComparison.OrdinalIgnoreCase));
                if (userNick != null)
                {
                    return new WebApiResult(-1, "该昵称已经被占用");
                }
            }
            var user = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                return new WebApiResult(-1, "未找到该用户" + CurrentLoginAddress);
            }
            else if (user.nick_name != null && user.nick_name.Contains("黑名单用户", StringComparison.OrdinalIgnoreCase))
            {
                return new WebApiResult(-1, "您已经被拉入黑名单");
            }
            else
            {
                user.nick_name = command.NickName;
                await _mySqlMasterDbContext.SaveChangesAsync();
                return new WebApiResult(1, "修改昵称成功");
            }
        }
        /// <summary>
        /// 修改用户邮箱
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("edit/useremail")]
        public async Task<WebApiResult> EditUserEmail([FromBody] EditUserEmaiCommand command)
        {
            var user = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(this.CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            user.email = command.Email;
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
