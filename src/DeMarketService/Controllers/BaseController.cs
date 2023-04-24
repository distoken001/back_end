using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace deMarketService.Controllers
{
    public class BaseController : Controller
    {
        public string CurrentLoginAddress
        {
            get
            {
                var u = User.Claims.FirstOrDefault(x => x.Type == "address")?.Value;
                if (string.IsNullOrEmpty(u)) return string.Empty;
                return u;
            }
        }

        public long CurrentLoginChain
        {
            get
            {
                try
                {
                    var login_chain = User.Claims.FirstOrDefault(x => x.Type == "login_chain")?.Value;
                    if (!string.IsNullOrEmpty(login_chain))
                    {
                        return long.Parse(login_chain);
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }
    }
}
