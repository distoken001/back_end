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

        public int CurrentLoginChain
        {
            get
            {
                try
                {
                    var login_chain = User.Claims.FirstOrDefault(x => x.Type == "login_chain")?.Value;
                    if (!string.IsNullOrEmpty(login_chain))
                    {
                        return int.Parse(login_chain);
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 链ID
        /// </summary>
        public string ChainId
        {
            get
            {
                return HttpContext.Request.Headers["chain_id"];
            }
        }
    }
}
