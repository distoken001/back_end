using deMarketService.Common.Model.DataEntityModel;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        MySqlMasterDbContext _mySqlMasterDbContext;

        public OrderController(MySqlMasterDbContext mySqlMasterDbContext)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("upload")]
        public async Task<WebApiResult> upload([FromBody] ReqOrdersVo req)
        {
            return new WebApiResult(1, "上传图片");
        }


        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("list")]
        public async Task<WebApiResult> list([FromBody] ReqOrdersVo req)
        {
            return new WebApiResult(1, "看名称好形势-我的订单列表");
        }


        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("detail")]
        public async Task<WebApiResult> detail([FromBody] ReqOrdersVo req)
        {
            try
            {
                orders order = _mySqlMasterDbContext.orders.Where(x => x.id == 24).FirstOrDefault();
                return new WebApiResult(1, "订单详情", order);
            }
            catch(Exception e)
            {
                return new WebApiResult(1, e.ToString());
            }

        }

    }
}
