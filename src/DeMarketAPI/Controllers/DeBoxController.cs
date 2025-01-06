using DeMarketAPI.Common.Model.HttpApiModel.RequestModel;
using DeMarketAPI.Common.Model.HttpApiModel.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DeMarketAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeBoxController : BaseController
    {
        [HttpPost("receivemessage")]
        [AllowAnonymous]
        public async Task<JsonResult> ReceiveMessage([FromBody] GetCardTypeListRequest req)
        {
            return Json(new WebApiResult(1, "成功"));
        }
    }
}