using Microsoft.AspNetCore.Mvc;
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
    }
}
