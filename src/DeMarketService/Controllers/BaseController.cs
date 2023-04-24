using Microsoft.AspNetCore.Mvc;
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
