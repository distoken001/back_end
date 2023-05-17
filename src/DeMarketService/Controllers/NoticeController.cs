using System;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using System.IO;
using System.Threading.Tasks;
using deMarketService.DbContext;
using deMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using System.Net;
using System.Net.Mail;
using deMarketService.Common.Common;
using deMarketService.Common.Model.DataEntityModel;
using Microsoft.EntityFrameworkCore;
using deMarketService.Common.Model;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeController : BaseController
    {

        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;

        public NoticeController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name = "req" ></ param >
        /// < returns ></ returns >
        [HttpPost("sendemail")]
        public async Task<JsonResult> SendEmail([FromBody] SendEmailRequest request)
        {
            string senderEmail = "demarkethub@gmail.com";
            string senderPassword = "Gg__1208";


            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };
            var order = await _mySqlMasterDbContext.orders.FirstOrDefaultAsync(p => p.order_id == request.order_id && p.chain_id == request.chain_id&&p.contract==request.contract);
            OrderStatus status = order.status;
            var seller= await  _mySqlMasterDbContext.users.FirstOrDefaultAsync(u => u.address == order.seller);
            var buyer = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(u => u.address == order.buyer);
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(senderEmail);
            if (!string.IsNullOrEmpty(seller?.email))
            {
                mailMessage.To.Add(seller.email);
            }
            if (!string.IsNullOrEmpty(buyer?.email))
            {
                mailMessage.To.Add(buyer.email);
            }

            mailMessage.Subject = "德玛De-MArket通知";
            if (status == OrderStatus.Initial)
            {
                mailMessage.Body = $"您发布的商品（订单id={request.order_id}）已经上架，如果您的商品有新动态，我们将邮件通知您！";
            }
            if (status != OrderStatus.Ordered)
            {
                mailMessage.Body = $"您的商品（订单id={request.order_id}）有新动态，请注意查看！------此封邮件收件人为买卖双方预留的邮箱联系方式，当然您也可以在商品详情页查看对方的联系方式！";
            }

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in SendEmail(): {0}", ex.ToString());
                return Json(new WebApiResult(1, "发送失败"));
            }
            return Json(new WebApiResult(1, "发送成功"));
        }
    }
}
