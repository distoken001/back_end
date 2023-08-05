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
    public class OrderController : BaseController
    {
        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;

        public OrderController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
        }

        /// <summary>
        /// 获取腾讯COS相关参数
        /// </summary>
        /// <returns></returns>
        [HttpGet("get/cosParms")]
        [ProducesResponseType(typeof(GetCredentialResponse), 200)]
        public JsonResult GetCosParms()
        {
            var parms = txCosUploadeService.GetCredential();
            var res = JsonConvert.DeserializeObject<GetCredentialResponse>(parms);
            return Json(new WebApiResult(1, "", res));
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("upload")]
        public async Task<JsonResult> upload([FromForm] IFormCollection formCollection)
        {
            if ((formCollection == null || formCollection.Files.Count == 0))
            {
                return Json(new WebApiResult(-1, "没有可上传的文件"));
            }
            var file = formCollection.Files[0];

            var cosName = string.Format("{0}_{1}_cp{2}", DateTime.Now.ToString("yyyyMMddhhmmss"), new Random().Next(10000), Path.GetExtension(file.FileName));

            using (var stream = file.OpenReadStream())
            {
                var bytes = ToByteArray(stream);
                var res = await txCosUploadeService.Upload(bytes, cosName);
                return Json(new WebApiResult(1, "上传图片", res));
            }

        }


        private byte[] ToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        /// <summary>
        /// 订单列表
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("list")]
        [ProducesResponseType(typeof(PagedModel<OrderResponse>), 200)]
        public async Task<JsonResult> list([FromBody] GetOrderListRequest req)
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
            queryEntities = queryEntities.OrderBy(p => p.weight).ThenByDescending(p => p.create_time).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<orders, OrderResponse>(list);
            var sellers = viewList.Select(a => a.seller).ToList();
            var users = _mySqlMasterDbContext.users.AsNoTracking().Where(a => sellers.Contains(a.address)).ToList();

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
                }
                a.like_count = _mySqlMasterDbContext.ebay_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.status == 1).Count() + new Random().Next(1, 15);
                if (!string.IsNullOrEmpty(CurrentLoginAddress))
                {
                    a.is_like = _mySqlMasterDbContext.ebay_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.address.Equals(CurrentLoginAddress) && au.status == 1).Count();
                }
            }
            var res = new PagedModel<OrderResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "订单列表" + CurrentLoginAddress, res));
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
            int randomNumber = random.Next(0, totalCount - req.pageSize + 1);
            queryEntities = queryEntities.OrderBy(p => p.weight).ThenByDescending(p => p.create_time).Skip(randomNumber).Take(req.pageSize);
            var list = await queryEntities.ToListAsync();
            var viewList = AutoMapperHelper.MapDbEntityToDTO<orders, OrderResponse>(list);
            var sellers = viewList.Select(a => a.seller).ToList();
            var users = _mySqlMasterDbContext.users.AsNoTracking().Where(a => sellers.Contains(a.address)).ToList();

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
                }
                a.like_count = _mySqlMasterDbContext.ebay_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.status == 1).Count() + random.Next(1, 15);
                if (!string.IsNullOrEmpty(CurrentLoginAddress))
                {
                    a.is_like = _mySqlMasterDbContext.ebay_user_like.AsNoTracking().Where(au => au.order_id == a.id && au.address.Equals(CurrentLoginAddress) && au.status == 1).Count();
                }
            }
            var res = new PagedModel<OrderResponse>(totalCount, viewList);
            return Json(new WebApiResult(1, "订单列表", res));
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
            return Json(new WebApiResult(1, "添加成功"));
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
            var resList = _mySqlMasterDbContext.orders.Where(p => p.order_id == order_id && p.chain_id == chain_id);
            if (!string.IsNullOrEmpty(contract))
            {
                resList = resList.Where(p => p.contract.Equals(contract, StringComparison.OrdinalIgnoreCase));
            }
            var res = await resList.FirstOrDefaultAsync();
            var chainTokens = _mySqlMasterDbContext.chain_tokens.AsNoTracking().ToList();
            var re = AutoMapperHelper.MapDbEntityToDTO<orders, OrderResponse>(res);
            var token = chainTokens.FirstOrDefault(c => c.chain_id == re.chain_id && c.token_address.Equals(re.token, StringComparison.OrdinalIgnoreCase));
            var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
            re.token_des = tokenView;
            //return Json(new WebApiResult(1, "CurrentLoginAddress:" + CurrentLoginAddress + ",CurrentLoginChain:"+ CurrentLoginChain, ress));
            return Json(new WebApiResult(1, "查询成功", re));
        }


    }
}
