using System;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using System.Threading.Tasks;
using CommonLibrary.DbContext;
using deMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CommonLibrary.Common.Common;
using CommonLibrary.Common.Model.DataEntityModel;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : BaseController
    {
        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;
        private readonly IConfiguration _configuration;

        public StoreController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService, IConfiguration configuration)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
            _configuration = configuration;
        }
        /// <summary>
        /// 店铺(社区)列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("list")]
        [ProducesResponseType(typeof(UsersResponse), 200)]
        public async Task<JsonResult> list([FromBody] GetStoreRequest req)
        {
            if (req.type != 2 && req.type != 4)
            {
                return Json(new WebApiResult(-1, "获取失败"));
            }
            var queryEntities = _mySqlMasterDbContext.users.AsNoTracking().Where(a=>(a.type&req.type)>0).AsQueryable();
            if(!string.IsNullOrEmpty(req.address.Trim()))
            {
                queryEntities = queryEntities.Where(a => a.address.Equals(req.address.Trim()));
            }
            if (!string.IsNullOrEmpty(req.name.Trim()))
            {
                if (req.type == 2)
                {
                    queryEntities = queryEntities.Where(a => a.store_name.Contains(req.name.Trim()));
                }
                else if (req.type == 4)
                {
                    queryEntities = queryEntities.Where(a => a.club_name.Contains(req.name.Trim()));
                }
            }
            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<users, UsersResponse>(list);
            var res = new PagedModel<UsersResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "获取成功", res));
        }
    }
}

