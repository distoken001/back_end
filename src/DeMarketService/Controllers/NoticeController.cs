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
using deMarketService.Common.Common;
using deMarketService.Common.Model.DataEntityModel;
using Microsoft.EntityFrameworkCore;
using deMarketService.Common.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using MimeKit;
using MailKit;
using MailKit.Security;
using Google.Apis.Util.Store;
using System.Threading;
using System.Net.Mail;
using Newtonsoft.Json;
using deMarketService.Proxies;
using System.Collections.Generic;
using Org.BouncyCastle.Ocsp;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeController : BaseController
    {

        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;
        private readonly EmailProxy _mailKitEmail;

        public NoticeController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService, EmailProxy mailKitEmail)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
            _mailKitEmail = mailKitEmail;
        }

        [HttpPost("sendemail")]
        public async Task<JsonResult> SendEmail([FromBody] SendEmailRequest request)
        {

            try
            {
                List<string> ls = new List<string>();
                var order = await _mySqlMasterDbContext.orders.FirstOrDefaultAsync(p => p.order_id == request.order_id && p.chain_id == request.chain_id && p.contract == request.contract);
                OrderStatus status = order.status;
                var seller = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(u => u.address == order.seller);
                var buyer = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(u => u.address == order.buyer);
                if (!string.IsNullOrEmpty(seller?.email))
                {
                    ls.Add(seller.email);
                }
                if (!string.IsNullOrEmpty(buyer?.email))
                {
                    ls.Add(buyer.email);
                }

                string subject = "德玛商城通知";
                string mailMessage = "";
                if (status == OrderStatus.Initial)
                {
                    mailMessage = $"您在{order.chain_id.ToString()}网络发布的商品({order.name})已成功上架，如果您的商品有新动态，我们将邮件通知您！";
                }
                else if (status == OrderStatus.SellerCancelWithoutDuty)
                {
                    mailMessage = $"您在{order.chain_id.ToString()}网络发布的商品({order.name})已取消，特此通知！";
                }
                else if (status == OrderStatus.Completed)
                {
                    mailMessage = $"您在{order.chain_id.ToString()}网络的商品({order.name})交易已完成，特此通知！";
                }
                else if (status == OrderStatus.ConsultCancelCompleted)
                {
                    mailMessage = $"您在{order.chain_id.ToString()}网络的商品({order.name})已经与对方协商取消交易，特此通知！";
                }
                else
                {
                    mailMessage = $"您在{order.chain_id.ToString()}网络的商品（{order.name}）有新动态，请注意查看！<br/><br/><br/>---此邮件收件人为买卖双方预留的邮箱联系方式<br/><br/><br/>---您也可以在商品详情页查看对方的其他联系方式！";
                }
                var a = _mailKitEmail.SendMailAsync(subject, mailMessage, ls).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in SendEmail(): {0}", ex.ToString());
                return Json(new WebApiResult(1, "发送失败"));
            }
            return Json(new WebApiResult(1, "发送成功"));
        }
        [HttpPost("cooperate")]
        public async Task<JsonResult> Cooperate([FromBody] CooperateRequest request)
        {

            try
            {
                var cooperator = new cooperator
                {
                    status = 1,
                    create_time = DateTime.Now,
                    update_time = DateTime.Now,
                    creator = "system",
                    updater = null,
                    contact = request.contact,
                    name = request.name
                };
                await _mySqlMasterDbContext.cooperator.AddAsync(cooperator);
                await _mySqlMasterDbContext.SaveChangesAsync();
                List<string> ls = new List<string>();
                ls.Add("songnian.liu@outlook.com");
                string subject = "德玛商城通知";
                string mailMessage = "有新的意向合作客户，他的联系方式:" + request.contact;
                var a = _mailKitEmail.SendMailAsync(subject, mailMessage, ls).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in Cooperate(): {0}", ex.ToString());
                return Json(new WebApiResult(1, "发送失败"));
            }
            return Json(new WebApiResult(1, "发送成功"));
        }
    }
}
