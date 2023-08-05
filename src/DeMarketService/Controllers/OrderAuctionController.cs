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
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderAuctionController : BaseController
    {
        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;

        public OrderAuctionController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
        }
        /// <summary>
        /// 查询拍卖列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("list")]
        [ProducesResponseType(typeof(PagedModel<OrderAuctionResponse>), 200)]
        public async Task<JsonResult> list([FromBody] OrderAuctionRequest req)
        {
            try
            {
                // 获取当前时间
                DateTime currentTime = DateTime.Now;
                // 将时间转换为时间戳（秒）
                long timestamp = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

                var queryEntities = _mySqlMasterDbContext.orders_auction.AsNoTracking().AsQueryable();
                var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking().ToList();
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
                if (req.status != 0)
                {
                    if (req.status == OrderAuctionStatusActual.即将开始)
                    {
                        queryEntities = queryEntities.Where(p => p.start_time > timestamp);
                    }
                    else if (req.status == OrderAuctionStatusActual.进行中)
                    {
                        queryEntities = queryEntities.Where(p => p.start_time <= timestamp && p.end_time > timestamp);
                    }
                    else
                    {
                        queryEntities = queryEntities.Where(p => p.end_time <= timestamp);
                    }
                }

                var totalCount = await queryEntities.CountAsync();
                queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
                var list = await queryEntities.ToListAsync();
                var viewList = AutoMapperHelper.MapDbEntityToDTO<orders_auction, OrderAuctionResponse>(list);
                var sellers = viewList.Select(a => a.seller).ToList();
                var users = _mySqlMasterDbContext.users.AsNoTracking().Where(a => sellers.Contains(a.address)).ToList();
                var user_nfts = _mySqlMasterDbContext.user_nft.AsNoTracking().ToList();
                foreach (var a in viewList)
                {
                    var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token, StringComparison.OrdinalIgnoreCase));
                    var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                    a.token_des = tokenView;
                    a.belong = getBelongUserEnum(a.buyer, a.seller);
                    var user = users.FirstOrDefault(c => c.address.Equals(a.seller, StringComparison.OrdinalIgnoreCase));
                    if (user != null)
                    {
                        a.seller_nick = user.nick_name ?? "匿名商家";
                        a.seller_email = user.email ?? "未预留邮箱";
                        a.seller_nfts = user_nfts.Where(un => un.address.Equals(user.address) && un.status == 1).Select(a => a.nft).ToArray();
                    }
                    a.like_count = _mySqlMasterDbContext.auction_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.status == 1).Count() + new Random().Next(1, 15);
                    if (!string.IsNullOrEmpty(CurrentLoginAddress))
                    {
                        a.is_like = _mySqlMasterDbContext.auction_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.address.Equals(CurrentLoginAddress) && au.status == 1).Count();
                    }
                }

                var res = new PagedModel<OrderAuctionResponse>(totalCount, viewList);
                return Json(new WebApiResult(1, "订单列表" + CurrentLoginAddress, res));
            }
            catch (Exception ex)
            {
                return Json(new WebApiResult(-1, "服务器异常", ex));
            }
        }
        /// <summary>
        /// 猜您喜欢
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("like")]
        [ProducesResponseType(typeof(PagedModel<OrderAuctionResponse>), 200)]
        public async Task<JsonResult> like([FromBody] OrderAuctionRequest req)
        {
            var queryEntities = _mySqlMasterDbContext.orders_auction.AsNoTracking().AsQueryable();
            var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking().ToList();
            queryEntities = queryEntities.Where(p => p.status == OrderAuctionStatus.Initial && (p.buyer.Equals("0x0000000000000000000000000000000000000000", StringComparison.OrdinalIgnoreCase) || p.buyer.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase) || p.seller.Equals(CurrentLoginAddress, StringComparison.OrdinalIgnoreCase)));

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
            int randomNumber = random.Next(0, totalCount - req.pageSize + 1);
            queryEntities = queryEntities.OrderByDescending(p => p.create_time).Skip(randomNumber).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<orders_auction, OrderAuctionResponse>(list);
            var sellers = viewList.Select(a => a.seller).ToList();
            var users = _mySqlMasterDbContext.users.AsNoTracking().Where(a => sellers.Contains(a.address)).ToList();
            var user_nfts = _mySqlMasterDbContext.user_nft.AsNoTracking().ToList();
            foreach (var a in viewList)
            {
                var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token, StringComparison.OrdinalIgnoreCase));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                a.token_des = tokenView;
                a.belong = getBelongUserEnum(a.buyer, a.seller);
                var user = users.FirstOrDefault(c => c.address.Equals(a.seller, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {
                    a.seller_nick = user.nick_name ?? "匿名商家";
                    a.seller_email = user.email ?? "未预留邮箱";
                    a.seller_nfts = user_nfts.Where(un => un.address.Equals(user.address) && un.status == 1).Select(a => a.nft).ToArray();
                }
                a.like_count = _mySqlMasterDbContext.auction_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.status == 1).Count() + random.Next(1, 15);
                if (!string.IsNullOrEmpty(CurrentLoginAddress))
                {
                    a.is_like = _mySqlMasterDbContext.auction_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.address.Equals(CurrentLoginAddress) && au.status == 1).Count();
                }
            }
            var res = new PagedModel<OrderAuctionResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "订单列表", res));
        }
        /// <summary>
        /// 收藏（添加收藏或取消收藏）
        /// </summary>
        /// <param name = "req" ></ param >
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
                var aul = _mySqlMasterDbContext.auction_user_like.Where(a => a.address.Equals(CurrentLoginAddress)).FirstOrDefault();
                if (aul != null)
                {
                    aul.status = aul.status == 0 ? 1 : 0;
                    aul.update_time = DateTime.Now;
                }
                else
                {
                    _mySqlMasterDbContext.auction_user_like.Add(new auction_user_like() { address = CurrentLoginAddress, create_time = DateTime.Now, update_time = DateTime.Now, order_id = request.id, status = 1 });
                }
            }
            _mySqlMasterDbContext.SaveChanges();
            return Json(new WebApiResult(1, "添加成功"));
        }

        /// <summary>
        ///查询拍卖详情
        /// </summary>
        /// <param name="order_id"></param>
        /// <param name="chain_id"></param>
        /// <param name="contract"></param>
        /// <returns></returns>
        [HttpGet("detail")]
        [ProducesResponseType(typeof(OrderAuctionResponse), 200)]
        public async Task<JsonResult> detail([FromQuery] long order_id, [FromQuery] ChainEnum chain_id, [FromQuery] string contract)
        {
            try
            {
                var resList = _mySqlMasterDbContext.orders_auction.Where(p => p.order_id == order_id && p.chain_id == chain_id);
                if (!string.IsNullOrEmpty(contract))
                {
                    resList = resList.Where(p => p.contract.Equals(contract, StringComparison.OrdinalIgnoreCase));
                }
                var res = await resList.FirstOrDefaultAsync();
                var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking().ToList();
                var re = AutoMapperHelper.MapDbEntityToDTO<orders_auction, OrderAuctionResponse>(res);
                re.belong = getBelongUserEnum(res.buyer, res.seller);
                var token = chainTokens.FirstOrDefault(c => c.chain_id == re.chain_id && c.token_address.Equals(re.token, StringComparison.OrdinalIgnoreCase));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                re.token_des = tokenView;
                //return Json(new WebApiResult(1, "CurrentLoginAddress:" + CurrentLoginAddress + ",CurrentLoginChain:"+ CurrentLoginChain, ress));
                return Json(new WebApiResult(1, "查询成功"+CurrentLoginAddress, re));
            }
            catch (Exception ex)
            {
                return Json(new WebApiResult(-1, "服务器异常", ex));
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
                var queryEntities = _mySqlMasterDbContext.auction_user_like.AsNoTracking().AsQueryable();
                var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking().ToList();
                var totalCount = queryEntities.Count();
                queryEntities = queryEntities.Where(a => a.address.Equals(CurrentLoginAddress)).OrderByDescending(p => p.update_time).Skip((request.pageIndex - 1) * request.pageSize).Take(request.pageSize);
                var idList = queryEntities.Select(a => a.order_id).ToList();
                var list = _mySqlMasterDbContext.orders_auction.AsNoTracking().Where(a => idList.Contains(a.id)).ToList();
                var viewList = AutoMapperHelper.MapDbEntityToDTO<orders_auction, OrderAuctionResponse>(list);
                var sellers = viewList.Select(a => a.seller).ToList();
                var users = _mySqlMasterDbContext.users.AsNoTracking().Where(a => sellers.Contains(a.address)).ToList();
                var user_nfts = _mySqlMasterDbContext.user_nft.AsNoTracking().Where(a => a.status == 1).ToList();
                foreach (var a in viewList)
                {
                    var token = chainTokens.FirstOrDefault(c => c.chain_id == a.chain_id && c.token_address.Equals(a.token, StringComparison.OrdinalIgnoreCase));
                    var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                    a.token_des = tokenView;
                    var user = users.FirstOrDefault(c => c.address.Equals(a.seller, StringComparison.OrdinalIgnoreCase));
                    if (user != null)
                    {
                        a.seller_nick = user.nick_name ?? "匿名商家";
                        a.seller_email = user.email ?? "未预留邮箱";
                        a.seller_nfts = user_nfts.Where(un => un.address.Equals(user.address)).Select(a => a.nft).ToArray();
                    }
                    a.like_count = _mySqlMasterDbContext.auction_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.status == 1).Count() + new Random().Next(1, 15);
                    if (!string.IsNullOrEmpty(CurrentLoginAddress))
                    {
                        a.is_like = _mySqlMasterDbContext.auction_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.address.Equals(CurrentLoginAddress) && au.status == 1).Count();
                    }
                }

                var res = new PagedModel<OrderAuctionResponse>(totalCount, viewList);
                return Json(new WebApiResult(1, "查询我的收藏列表成功", res));
            }
        }
    }
}
