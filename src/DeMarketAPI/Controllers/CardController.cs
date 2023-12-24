using CommonLibrary.Common.Common;
using DeMarketAPI.Common.Model;
using DeMarketAPI.Common.Model.HttpApiModel.RequestModel;
using DeMarketAPI.Common.Model.HttpApiModel.ResponseModel;
using CommonLibrary.DbContext;
using DeMarketAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TencentCloud.Ecm.V20190719.Models;
using TencentCloud.Tcss.V20201101.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CommonLibrary.Model.DataEntityModel;
using TencentCloud.Pds.V20210701.Models;

namespace DeMarketAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : BaseController
    {
        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public CardController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService, IHostEnvironment environment, IConfiguration configuration)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
            _environment = environment;
        }


        /// <summary>
        /// 刮刮卡未刮开列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("not_opened_list")]
        [ProducesResponseType(typeof(PagedModel<CardNotOpenedResponse>), 200)]
        public async Task<JsonResult> not_opened_list([FromBody] GetNotOpenedCardListRequest req)
        {
            var queryEntities = _mySqlMasterDbContext.card_not_opened.AsNoTracking().AsQueryable();
            var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking();
            var cardTypes = _mySqlMasterDbContext.card_type.AsNoTracking();
            queryEntities = queryEntities.Where(p => p.buyer.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase) && p.amount != 0);

            if (req.chain_id != 0)
            {
                queryEntities = queryEntities.Where(p => p.chain_id == req.chain_id);
            }
            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<card_not_opened, CardNotOpenedResponse>(list);

            foreach (var a in viewList)
            {
                var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                var cardType = cardTypes.FirstOrDefault(c => c.chain_id == a.chain_id && c.type.Equals(a.card_type));
                var cardTypeView = AutoMapperHelper.MapDbEntityToDTO<card_type, CardTypeResponse>(cardType);
                a.token_des = tokenView;
                a.card_type_des = cardTypeView;
            }
            var res = new PagedModel<CardNotOpenedResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "刮刮卡未刮开列表" + CurrentLoginAddress, res));
        }
        /// <summary>
        /// 已刮开列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("opened_list")]
        [ProducesResponseType(typeof(PagedModel<CardOpenedResponse>), 200)]
        public async Task<JsonResult> opened_list([FromBody] GetOpenedCardListRequest req)
        {
            var queryEntities = _mySqlMasterDbContext.card_opened.AsNoTracking().AsQueryable();
            var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking();
            var cardTypes = _mySqlMasterDbContext.card_type.AsNoTracking();
            queryEntities = queryEntities.Where(p => p.buyer.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));
            if (req.chain_id != 0)
            {
                queryEntities = queryEntities.Where(p => p.chain_id == req.chain_id);
            }
            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<card_opened, CardOpenedResponse>(list);

            foreach (var a in viewList)
            {
                var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                var cardType=  cardTypes.FirstOrDefault(c => c.chain_id == a.chain_id && c.type.Equals(a.card_type));
                var cardTypeView = AutoMapperHelper.MapDbEntityToDTO<card_type, CardTypeResponse>(cardType);
                a.token_des = tokenView;
                a.card_type_des = cardTypeView;
            }
            var res = new PagedModel<CardOpenedResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "刮刮卡已刮开列表" + CurrentLoginAddress, res));
        }
        /// <summary>
        /// 获取卡类型
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("card_type_list")]
        [ProducesResponseType(typeof(PagedModel<CardTypeResponse>), 200)]
        public async Task<JsonResult> card_type_list([FromBody] GetCardTypeListRequest req)
        {
            var queryEntities = _mySqlMasterDbContext.card_type.Where(a => a.state == 1).AsNoTracking();
            var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking();
            if (req.chain_id != 0)
            {
                queryEntities = queryEntities.Where(p => p.chain_id == req.chain_id);
            }

            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<card_type, CardTypeResponse>(list);

            foreach (var a in viewList)
            {
                var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                a.token_des = tokenView;
            }
            var res = new PagedModel<CardTypeResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "获取卡类型列表" + CurrentLoginAddress, res));
        }
    }
}
