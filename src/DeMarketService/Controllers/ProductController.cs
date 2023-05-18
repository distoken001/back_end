using deMarketService.Common.Common;
using deMarketService.Common.Model.DataEntityModel;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using deMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : BaseController
    {
        MySqlMasterDbContext _mySqlMasterDbContext;

        public ProductController(MySqlMasterDbContext mySqlMasterDbContext)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
        }

        /// <summary>
        /// 商品列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("list")]
        [ProducesResponseType(typeof(PagedModel<OrdersResponse>), 200)]
        public async Task<JsonResult> list([FromBody] ReqOrdersVo req)
        {
            var queryEntities = _mySqlMasterDbContext.orders.AsNoTracking().AsQueryable();
            req.searchType = 0;
            queryEntities = queryEntities.Where(p => p.status == 0);


            if (!string.IsNullOrEmpty(req.name))
            {
                queryEntities = queryEntities.Where(p => p.name.ToLower().Contains(req.name.ToLower()));
            }
            if (!string.IsNullOrEmpty(req.description))
            {
                queryEntities = queryEntities.Where(p => p.description.ToLower().Contains(req.description.ToLower()));
            }

            //if (req.order_id.HasValue)
            //    queryEntities = queryEntities.Where(p => p.order_id == req.order_id);
            if (req.chain_id != 0)
                queryEntities = queryEntities.Where(p => p.chain_id == req.chain_id);

            if (req.priceMin.HasValue)
                queryEntities = queryEntities.Where(p => p.price >= req.priceMin);

            if (req.priceMax.HasValue)
                queryEntities = queryEntities.Where(p => p.price <= req.priceMax);

            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<orders, OrdersResponse>(list);
            var res = new PagedModel<OrdersResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "商品列表", res));
        }
    }
}
