using CommonLibrary.Common.Common;
using CommonLibrary.DbContext;
using CommonLibrary.Model.DataEntityModel;
using DeMarketAPI.Common.Model.HttpApiModel.RequestModel;
using DeMarketAPI.Common.Model.HttpApiModel.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DeMarketAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : BaseController
    {
        private MySqlMasterDbContext _mySqlMasterDbContext;

        public TokenController(MySqlMasterDbContext mySqlMasterDbContext)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
        }

        /// <summary>
        /// 获取热门代币
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("list")]
        [ProducesResponseType(typeof(PagedModel<TokenViewModel>), 200)]
        [AllowAnonymous]
        public async Task<JsonResult> list([FromBody] TokenQueryRequest req)
        {
            var queryEntities = _mySqlMasterDbContext.tokens.Where(p => p.status == 1).AsNoTracking().AsQueryable();
            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.OrderBy(a => a.weight).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<tokens, TokenViewModel>(list);
            var res = new PagedModel<TokenViewModel>(totalCount, viewList);
            return Json(new WebApiResult(1, "预售热门代币列表", res));
        }
    }
}