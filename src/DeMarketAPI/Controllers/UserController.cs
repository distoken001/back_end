using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using CommonLibrary.Model;
using CommonLibrary.Model.DataEntityModel;
using DeMarketAPI.Common.Model.HttpApiModel.RequestModel;
using DeMarketAPI.Common.Model.HttpApiModel.ResponseModel;
using DeMarketAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TencentCloud.Ckafka.V20190819.Models;

namespace DeMarketAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;
        private readonly IConfiguration _configuration;

        public UserController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService, IConfiguration configuration)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
            _configuration = configuration;
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [AllowAnonymous]
        public async Task<WebApiResult> login([FromBody] LoginRequest req)
        {
            if (req.address.Length != 42 || !req.address.StartsWith("0x"))
            {
                return new WebApiResult(-1, "登录请求地址不规范");
            }
            if (string.IsNullOrEmpty(req.parentAddress) || req.parentAddress.Length != 42 || !req.parentAddress.StartsWith("0x"))
            {
                req.parentAddress = null;
            }
            //对签名消息，账号地址三项信息进行认证，判断签名是否有效
            //if (!EthereumSignatureVerifier.Verify(req.signature, req.address))
            //{
            //    return new WebApiResult(-1, "signature verification failure");
            //}
            // var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(req.address) && p.chain_id == req.chain_id);
            var users = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(req.address, StringComparison.OrdinalIgnoreCase));
            bool is_first = false;
            if (users == null)
            {
                is_first = true;
                users = new CommonLibrary.Model.DataEntityModel.users
                {
                    address = req.address,
                    status = 1,
                    create_time = DateTime.Now,
                    update_time = DateTime.Now,
                    parent_address = req.parentAddress,
                };
                if (!string.IsNullOrEmpty(users.parent_address))
                {
                    users.rate = string.IsNullOrEmpty(_configuration["rate"]) ? 0.002M : decimal.Parse(_configuration["rate"]);
                }
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
            var token = TokenHelper.GenerateToken(StringConstant.secretKey, StringConstant.issuer, StringConstant.audience, 1, userClaims);
            var user_nfts = _mySqlMasterDbContext.user_nft.Where(a => a.address.Equals(req.address) && a.status == 1).ToList();
            var nfts = user_nfts.Select(a => a.nft).ToArray();
            if (nfts.Length > 0)
            {
                if (_configuration["Env"] == "dev")
                {
                    users.avatar = "https://api-dev.demarket.io/docs/" + nfts[0] + ".png";
                }
                else if (_configuration["Env"] == "prod")
                {
                    users.avatar = "https://api.demarket.io/docs/" + nfts[0] + ".png";
                }
            }
            return new WebApiResult(1, "登录成功", new LoginResponse { token = token, avatar = users.avatar, nick_name = users.nick_name, telegram_user_name = users.telegram_user_name, email = users.email, is_first = is_first, nfts = nfts });
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
                users.ip = ClientIP;
                await _mySqlMasterDbContext.SaveChangesAsync();
            }
            return new WebApiResult(1, "成功");
        }

        /// <summary>
        /// 我的详情
        /// </summary>
        /// <returns></returns>
        [HttpPost("detail")]
        [ProducesResponseType(typeof(UsersResponse), 200)]
        public async Task<WebApiResult> detail()
        {
            var user = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            var userView = AutoMapperHelper.MapDbEntityToDTO<users, UsersResponse>(user);
            var nfts = _mySqlMasterDbContext.user_nft.AsNoTracking().Where(a => a.address.Equals(CurrentLoginAddress) && a.status == 1).Select(a => a.nft).ToArray();
            userView.nfts = nfts;
            return new WebApiResult(1, "查询成功", userView);
        }

        /// <summary>
        /// 被邀请人列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
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
        /// 当前登录人佣金列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
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
        public async Task<WebApiResult> EditUser([FromBody] EditUserRequest command)
        {
            var user = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(this.CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            user.avatar = command.Avatar;
            await _mySqlMasterDbContext.SaveChangesAsync();
            return new WebApiResult(1, "修改用户", true);
        }

        ///// <summary>
        ///// 修改昵称
        ///// </summary>
        ///// <param name="command"></param>
        ///// <returns></returns>
        //[HttpPost("edit/usernick")]
        //public async Task<WebApiResult> EditUserNick([FromBody] EditUserNickRequest command)
        //{
        //    if (command.NickName.Contains("DeMarket", StringComparison.OrdinalIgnoreCase) || command.NickName.Contains("德玛", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return new WebApiResult(-1, "不能包含官方敏感词汇");
        //    }
        //    var length = command.NickName.Length;
        //    if (length > 15)
        //    {
        //        return new WebApiResult(-1, "您输入的Telegram用户名过长");
        //    }
        //    command.NickName = string.IsNullOrEmpty(command.NickName) ? null : command.NickName;
        //    var user = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
        //    if (user == null)
        //    {
        //        return new WebApiResult(-1, "未找到该用户" + CurrentLoginAddress);
        //    }
        //    else if (user.nick_name != null && user.nick_name.Contains("黑名单用户", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return new WebApiResult(-1, "您已经被拉入黑名单");
        //    }
        //    else
        //    {
        //        user.nick_name = command.NickName;
        //        await _mySqlMasterDbContext.SaveChangesAsync();
        //        return new WebApiResult(1, "修改成功");
        //    }
        //}

        /// <summary>
        /// 修改Telegram账户
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("edit/usertelegram")]
        public async Task<WebApiResult> EditUserTelegram([FromBody] EditUserNickRequest command)
        {
            var length = command.VerifyCode.Length;
            if (length > 20)
            {
                return new WebApiResult(-1, "您输入的验证码过长");
            }
            var user = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(p => p.address.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                return new WebApiResult(-1, "未找到该用户" + CurrentLoginAddress);
            }
            else
            {
                var fiveMinutesAgo = DateTime.Now.AddMinutes(-5);
                Console.WriteLine("FiveMinutesAgo:" + fiveMinutesAgo.ToString());
                var telegramUserChat = _mySqlMasterDbContext.telegram_user_chat.Where(a => a.verify_code.Equals(command.VerifyCode) && a.update_time>= fiveMinutesAgo && a.state == 1).FirstOrDefault();
                if (telegramUserChat != null)
                {
                    telegramUserChat.state = 0;
                    user.nick_name = "已绑定Telegram："+telegramUserChat.user_name; user.telegram_id = telegramUserChat.user_id; user.telegram_user_name = telegramUserChat.user_name;  
                    await _mySqlMasterDbContext.SaveChangesAsync();
                    return new WebApiResult(1, "绑定成功", telegramUserChat.user_name);
                }
                else
                {
                    return new WebApiResult(-1, "验证码不正确或已过期！");
                }
            }
        }

        /// <summary>
        /// 修改用户邮箱
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("edit/useremail")]
        public async Task<WebApiResult> EditUserEmail([FromBody] EditUserEmaiRequest command)
        {
            string pattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

            Regex regex = new Regex(pattern);
            bool isValid = regex.IsMatch(command.Email);
            if (isValid == false)
            {
                return new WebApiResult(-1, "不合法的邮箱");
            }
            var length = command.Email.Length;
            if (length > 40)
            {
                return new WebApiResult(-1, "您输入的邮箱过长");
            }
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