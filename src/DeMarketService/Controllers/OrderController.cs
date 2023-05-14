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
        [ProducesResponseType(typeof(PagedModel<OrdersResponse>), 200)]
        public async Task<JsonResult> list([FromBody] ReqOrdersVo req)
        {
            var queryEntities = _mySqlMasterDbContext.orders.AsNoTracking().AsQueryable();
            if (req.searchType == 0)
                queryEntities = queryEntities.Where(p => p.status == 0);
            var currentLoginAddress = this.CurrentLoginAddress;
            if (req.searchType == 1)
            {
                queryEntities = queryEntities.Where(p => p.buyer.ToLower().Equals(currentLoginAddress.ToLower()) || p.seller.ToLower().Equals(currentLoginAddress.ToLower()));
            }

            if (!string.IsNullOrEmpty(req.name) )
            {
                queryEntities = queryEntities.Where(p => p.name.ToLower().Contains(req.name.ToLower()));
            }
            if (!string.IsNullOrEmpty(req.description) )
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
            return Json(new WebApiResult(1, "订单列表", res));
        }


        /// <summary>
        /// 订单详情
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpGet("detail")]
        [ProducesResponseType(typeof(orders), 200)]
        public async Task<JsonResult> detail([FromQuery] long order_id, [FromQuery] ChainEnum chain_id)
        {
            var res = await _mySqlMasterDbContext.orders.FirstOrDefaultAsync(p => p.order_id == order_id && p.chain_id == chain_id);
            var  ress = AutoMapperHelper.MapDbEntityToDTO<orders, OrdersResponse>(res);
            //return Json(new WebApiResult(1, "CurrentLoginAddress:" + CurrentLoginAddress + ",CurrentLoginChain:"+ CurrentLoginChain, ress));
            return Json(new WebApiResult(1, "查询成功" , ress));
        }


    }
}
