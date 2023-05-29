using deMarketService.Common.Common;
using deMarketService.Common.Model.DataEntityModel;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChainController : BaseController
    {
        MySqlMasterDbContext _mySqlMasterDbContext;

        public ChainController(MySqlMasterDbContext mySqlMasterDbContext)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
        }

        /// <summary>
        /// 根据链ID获取热门代币
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("list")]
        [ProducesResponseType(typeof(PagedModel<ChainTokenViewModel>), 200)]
        public async Task<JsonResult> list([FromBody] ChainTokenQueryModel req)
        {
            var queryEntities = _mySqlMasterDbContext.chain_tokens.Where(p => p.chain_id == req.chainId).AsNoTracking().AsQueryable();
            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(list);
            var res = new PagedModel<ChainTokenViewModel>(totalCount, viewList);
            return Json(new WebApiResult(1, "热门代币列表", res));
        }
    }
}
