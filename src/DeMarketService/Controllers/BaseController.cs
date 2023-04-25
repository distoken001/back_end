using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace deMarketService.Controllers
{
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

        //public int CurrentLoginChain
        //{
        //    get
        //    {
        //        try
        //        {
        //            var chain_id = User.Claims.FirstOrDefault(x => x.Type == "chain_id")?.Value;
        //            if (!string.IsNullOrEmpty(chain_id))
        //            {
        //                return int.Parse(chain_id);
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
        public int ChainId
        {
            get
            {
                try
                {
                    return int.Parse(HttpContext.Request.Headers["chain_id"].FirstOrDefault());
                }
                catch(Exception ex)
                {
                    return 0;
                }
            }
        }
    }
}
