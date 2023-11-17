using CommonLibrary.Common.Common;
using deMarketService.Common.Model;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using CommonLibrary.DbContext;
using deMarketService.Services.Interfaces;
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

namespace deMarketService.Controllers
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
        [HttpPost("not_open_list")]
        [ProducesResponseType(typeof(PagedModel<NotOpenCardResponse>), 200)]
        public async Task<JsonResult> not_open_list([FromBody] GetNotOpenCardListRequest req)
        {
            var queryEntities = _mySqlMasterDbContext.card_not_opened.AsNoTracking().AsQueryable();
            var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking();
            queryEntities = queryEntities.Where(p => p.buyer.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase));


            var totalCount = await queryEntities.CountAsync();
            queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<card_not_opened, NotOpenCardResponse>(list);
           
            foreach (var a in viewList)
            {
                var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                a.token_des = tokenView;
            }
            var res = new PagedModel<NotOpenCardResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "刮刮卡未刮开列表" + CurrentLoginAddress, res));
        }
        /// <summary>
        /// 猜您喜欢
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("like")]
        [ProducesResponseType(typeof(PagedModel<OrderResponse>), 200)]
        public async Task<JsonResult> like([FromBody] GetOrderListRequest req)
        {
            var queryEntities = _mySqlMasterDbContext.orders.AsNoTracking().AsQueryable();
            var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking().ToList();
            queryEntities = queryEntities.Where(p => p.status == OrderStatus.Initial && (p.buyer.Equals("0x0000000000000000000000000000000000000000", StringComparison.OrdinalIgnoreCase) || p.buyer.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase) || p.seller.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase)));

            if (!string.IsNullOrEmpty(req.name))
            {
                queryEntities = queryEntities.Where(p => p.name.Contains(req.name, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(req.description))
            {
                queryEntities = queryEntities.Where(p => p.description.Contains(req.description, StringComparison.OrdinalIgnoreCase));
            }


            if (req.chain_id != 0)
            {
                queryEntities = queryEntities.Where(p => p.chain_id == req.chain_id);
            }
            var totalCount = await queryEntities.CountAsync();
            Random random = new Random();
            int randomNumber = random.Next(0, (totalCount - req.pageSize + 1) <= 0 ? 1 : (totalCount - req.pageSize + 1));
            queryEntities = queryEntities.OrderBy(p => p.weight).ThenByDescending(p => p.create_time).Skip(randomNumber).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<orders, OrderResponse>(list);
            var sellers = viewList.Select(a => a.seller).ToList();
            var users = _mySqlMasterDbContext.users.AsNoTracking().Where(a => sellers.Contains(a.address)).ToList();
            var user_nfts = _mySqlMasterDbContext.user_nft.AsNoTracking().Where(a => a.status == 1);
            foreach (var a in viewList)
            {
                var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token, StringComparison.OrdinalIgnoreCase));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                a.token_des = tokenView;
                a.belong = Tool.getBelongUserEnum(CurrentLoginAddress, a.buyer, a.seller);
                var user = users.FirstOrDefault(c => c.address.Equals(a.seller, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {
                    a.seller_nick = user.nick_name ?? "";
                    a.seller_email = user.email ?? "";
                    a.seller_nfts = user_nfts.Where(un => un.address.Equals(user.address)).Select(a => a.nft).ToArray();
                }
                a.like_count = _mySqlMasterDbContext.ebay_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.status == 1).Count() + random.Next(1, 15);
                if (!string.IsNullOrEmpty(CurrentLoginAddress))
                {
                    a.is_like = _mySqlMasterDbContext.ebay_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.address.Equals(CurrentLoginAddress) && au.status == 1).Count();
                }
            }
            var res = new PagedModel<OrderResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "猜您喜欢" + CurrentLoginAddress, res));
        }
        /// <summary>
        /// 收藏（添加或取消）
        /// </summary>
        /// <param name = "request" ></ param >
        /// < returns ></ returns >
        [HttpPost("switch_like")]
        public JsonResult switch_like([FromBody] SwitchLikeRequest request)
        {
            if (string.IsNullOrEmpty(CurrentLoginAddress))
            {
                return Json(new WebApiResult(-1, "您未登录"));
            }
            else
            {
                var aul = _mySqlMasterDbContext.ebay_user_like.Where(a => a.address.Equals(CurrentLoginAddress)).FirstOrDefault();
                if (aul != null)
                {
                    aul.status = aul.status == 0 ? 1 : 0;
                    aul.update_time = DateTime.Now;
                }
                else
                {
                    _mySqlMasterDbContext.ebay_user_like.Add(new ebay_user_like() { address = CurrentLoginAddress, create_time = DateTime.Now, update_time = DateTime.Now, order_id = request.id, status = 1 });
                }
            }
            _mySqlMasterDbContext.SaveChanges();
            return Json(new WebApiResult(1, "添加成功" + CurrentLoginAddress));
        }

        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name="order_id"></param>
        /// <param name="chain_id"></param>
        /// <param name="contract"></param>
        /// <returns></returns>
        [HttpGet("detail")]
        [ProducesResponseType(typeof(orders), 200)]
        public async Task<JsonResult> detail([FromQuery] long order_id, [FromQuery] ChainEnum chain_id, [FromQuery] string contract)
        {
            try
            {
                var resList = _mySqlMasterDbContext.orders.Where(p => p.order_id == order_id && p.chain_id == chain_id);
                if (!string.IsNullOrEmpty(contract))
                {
                    resList = resList.Where(p => p.contract.Equals(contract, StringComparison.OrdinalIgnoreCase));
                }
                var res = await resList.FirstOrDefaultAsync();
                var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking().ToList();
                var re = AutoMapperHelper.MapDbEntityToDTO<orders, OrderResponse>(res);
                re.belong = Tool.getBelongUserEnum(CurrentLoginAddress, res.buyer, res.seller);
                re.seller_nfts = _mySqlMasterDbContext.user_nft.AsNoTracking().Where(a => a.status == 1 && a.address.Equals(re.seller)).Select(a => a.nft).ToArray();
                var user = _mySqlMasterDbContext.users.AsNoTracking().FirstOrDefault(c => c.address.Equals(re.seller, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {
                    re.seller_nick = user.nick_name ?? "";
                    re.seller_email = user.email ?? "";
                }
                if (CurrentLoginAddress.Equals(re.buyer, StringComparison.OrdinalIgnoreCase) || CurrentLoginAddress.Equals(re.seller, StringComparison.OrdinalIgnoreCase))
                {
                    var userBuyer = _mySqlMasterDbContext.users.AsNoTracking().FirstOrDefault(c => c.address.Equals(re.buyer, StringComparison.OrdinalIgnoreCase));
                    if (userBuyer != null)
                    {
                        re.buyer_nick = userBuyer.nick_name ?? "";
                        re.buyer_email = userBuyer.email ?? "";
                    }
                }
                var token = chainTokens.FirstOrDefault(c => c.chain_id == re.chain_id && c.token_address.Equals(re.token, StringComparison.OrdinalIgnoreCase));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                re.token_des = tokenView;
                //return Json(new WebApiResult(1, "CurrentLoginAddress:" + CurrentLoginAddress + ",CurrentLoginChain:"+ CurrentLoginChain, ress));
                return Json(new WebApiResult(1, "查询成功" + CurrentLoginAddress, re));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送错误：{ex.Message}");
                return Json(new WebApiResult(-1, "查询订单详情失败" + ex.Message));
            }
        }
        /// <summary>
        /// 我的收藏
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("my_like")]
        public JsonResult my_like([FromBody] MyLikeRequest request)
        {
            if (string.IsNullOrEmpty(CurrentLoginAddress))
            {
                return Json(new WebApiResult(-1, "您未登录"));
            }
            else
            {
                var queryEntities = _mySqlMasterDbContext.ebay_user_like.AsNoTracking().AsQueryable();
                var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking().ToList();
                var totalCount = queryEntities.Count();
                queryEntities = queryEntities.Where(a => a.address.Equals(CurrentLoginAddress)).OrderByDescending(p => p.update_time).Skip((request.pageIndex - 1) * request.pageSize).Take(request.pageSize);
                var idList = queryEntities.Select(a => a.order_id).ToList();
                var list = _mySqlMasterDbContext.orders.AsNoTracking().Where(a => idList.Contains(a.id)).ToList();
                var viewList = AutoMapperHelper.MapDbEntityToDTO<orders, OrderResponse>(list);
                var sellers = viewList.Select(a => a.seller).ToList();
                var users = _mySqlMasterDbContext.users.AsNoTracking().Where(a => sellers.Contains(a.address)).ToList();
                var user_nfts = _mySqlMasterDbContext.user_nft.AsNoTracking().ToList();
                foreach (var a in viewList)
                {
                    var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token, StringComparison.OrdinalIgnoreCase));
                    var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                    a.token_des = tokenView;
                    a.belong = Tool.getBelongUserEnum(CurrentLoginAddress, a.buyer, a.seller);
                    var user = users.FirstOrDefault(c => c.address.Equals(a.seller, StringComparison.OrdinalIgnoreCase));
                    if (user != null)
                    {
                        a.seller_nick = user.nick_name ?? "匿名用户";
                        a.seller_email = user.email ?? "未预留邮箱";
                        a.seller_nfts = user_nfts.Where(un => un.address.Equals(user.address) && un.status == 1).Select(a => a.nft).ToArray();
                    }
                    a.like_count = _mySqlMasterDbContext.ebay_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.status == 1).Count() + new Random().Next(1, 15);
                    if (!string.IsNullOrEmpty(CurrentLoginAddress))
                    {
                        a.is_like = _mySqlMasterDbContext.ebay_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.address.Equals(CurrentLoginAddress) && au.status == 1).Count();
                    }
                }

                var res = new PagedModel<OrderResponse>(totalCount, viewList);
                return Json(new WebApiResult(1, "查询我的收藏列表成功" + CurrentLoginAddress, res));
            }
        }
    }
}
