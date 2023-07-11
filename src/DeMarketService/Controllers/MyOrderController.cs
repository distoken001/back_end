using deMarketService.Common.Common;
using deMarketService.Common.Model;
using deMarketService.Common.Model.DataEntityModel;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using deMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyOrderController : BaseController
    {
        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;

        public MyOrderController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
        }
        /// <summary>
        /// 我的订单列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("list")]
        [ProducesResponseType(typeof(PagedModel<OrderResponse>), 200)]
        public async Task<JsonResult> list([FromBody] ReqOrderVo req)
        {
            var queryEntities = _mySqlMasterDbContext.orders.AsNoTracking().AsQueryable();
            var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking().ToList();
            var currentLoginAddress = this.CurrentLoginAddress;

            queryEntities = queryEntities.Where(p => p.buyer.Equals(currentLoginAddress,StringComparison.OrdinalIgnoreCase) || p.seller.Equals(currentLoginAddress,StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(req.name))
            {
                queryEntities = queryEntities.Where(p => p.name.Contains(req.name,StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(req.description))
            {
                queryEntities = queryEntities.Where(p => p.description.Contains(req.description,StringComparison.OrdinalIgnoreCase));
            }

            if (req.chain_id != 0)
                queryEntities = queryEntities.Where(p => p.chain_id == req.chain_id);

            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<orders, OrderResponse>(list);
            foreach (var a in viewList)
            {
                var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token,StringComparison.OrdinalIgnoreCase));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                a.token_des = tokenView;
            }
            var res = new PagedModel<OrderResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, currentLoginAddress, res));
        }
    }
}
