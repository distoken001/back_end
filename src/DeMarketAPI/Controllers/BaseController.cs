using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace DeMarketAPI.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        /// <summary>
        /// 当前钱包地址
        /// </summary>
        public string CurrentLoginAddress
        {
            get
            {
                var u = User.Claims.FirstOrDefault(x => x.Type == "address")?.Value;
                if (string.IsNullOrEmpty(u)) return string.Empty;
                return u;
            }
        }

        public string ClientIP
        {
            get
            {
                string clientIP = HttpContext.Request.Headers["X-Forwarded-For"];
                Console.WriteLine("X-Forwarded-For: " + clientIP);
                if (string.IsNullOrEmpty(clientIP))
                {
                    clientIP = HttpContext.Request.Headers["X-Real-IP"];
                    Console.WriteLine("X-Real-IP: " + clientIP);
                }
                return clientIP;
            }
        }

        ///// <summary>
        ///// 获取用户身份
        ///// </summary>
        ///// <param name="buyer"></param>
        ///// <param name="seller"></param>
        ///// <returns></returns>
        //[HttpGet("getBelongUserEnum")]
        //public BelongUserEnum getBelongUserEnum(string buyer, string seller)
        //{
        //    if (string.IsNullOrEmpty(CurrentLoginAddress))
        //    {
        //        return BelongUserEnum.未知;
        //    }
        //    else if (CurrentLoginAddress.Equals(buyer, StringComparison.OrdinalIgnoreCase))
        //    {
        //        return BelongUserEnum.买家;
        //    }
        //    else if (CurrentLoginAddress.Equals(seller, StringComparison.OrdinalIgnoreCase))
        //    {
        //        return BelongUserEnum.卖家;
        //    }
        //    else
        //    {
        //        return BelongUserEnum.游客;
        //    };

        //}
        //public ChainEnum CurrentLoginChain
        //{
        //    get
        //    {
        //        try
        //        {
        //            var chain_str = User.Claims.FirstOrDefault(x => x.Type == "chain_id")?.Value;
        //            if (!string.IsNullOrEmpty(chain_str))
        //            {
        //             return (ChainEnum)Enum.Parse(typeof(ChainEnum), chain_str);
        //            }
        //            return 0;
        //        }
        //        catch (Exception ex)
        //        {
        //            return 0;
        //        }
        //    }
        //}

        /// <summary>
        /// 链ID
        /// </summary>
        //public int ChainId
        //{
        //    get
        //    {
        //        try
        //        {
        //            return int.Parse(HttpContext.Request.Headers["chain_id"].FirstOrDefault());
        //        }
        //        catch(Exception ex)
        //        {
        //            return 0;
        //        }
        //    }
        //}
    }
}