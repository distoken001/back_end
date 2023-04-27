using System;
using deMarketService.Common.Common;
using deMarketService.Common.Model.DataEntityModel;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using deMarketService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using deMarketService.Services.Interfaces;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : BaseController
    {

        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;

        public EventController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
        }

        /// <summary>
        /// 日志列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("list")]
        [ProducesResponseType(typeof(PagedModel<orders>), 200)]
        public async Task<JsonResult> list([FromBody] ReqLogsVo req)
        {
            var queryEntities = _mySqlMasterDbContext.event_logs.Where(a => a.operater == CurrentLoginAddress).AsNoTracking().AsQueryable();
            if (req.chain_id != 0)
            {
                queryEntities = queryEntities.Where(p => p.chain_id == req.chain_id);
            }
            else { return Json(new WebApiResult(1, "日志列表", new PagedModel<event_logs>(0, new List<event_logs>()))); }

            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var res = new PagedModel<event_logs>(totalCount, list);
            return Json(new WebApiResult(1, "日志列表", res));
        }

    }
}


