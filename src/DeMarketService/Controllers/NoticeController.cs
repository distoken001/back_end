using System;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using System.IO;
using System.Threading.Tasks;
using CommonLibrary.DbContext;
using deMarketService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using deMarketService.Common.Model.HttpApiModel.RequestModel;
using CommonLibrary.Common.Common;
using Microsoft.EntityFrameworkCore;
using deMarketService.Proxies;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using CommonLibrary.Model.DataEntityModel;
using System.Net.Mail;
using System.Linq;
using Telegram.Bot.Types;

namespace deMarketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeController : BaseController
    {

        MySqlMasterDbContext _mySqlMasterDbContext;
        private readonly ITxCosUploadeService txCosUploadeService;
        private readonly EmailProxy _mailKitEmail;
        private readonly IConfiguration _configuration;
        public NoticeController(MySqlMasterDbContext mySqlMasterDbContext, ITxCosUploadeService txCosUploadeService, EmailProxy mailKitEmail, IConfiguration configuration)
        {
            _mySqlMasterDbContext = mySqlMasterDbContext;
            this.txCosUploadeService = txCosUploadeService;
            _mailKitEmail = mailKitEmail;
            _configuration = configuration;
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

                string subject = "DeMarket通知";
                string mailMessage = "";
                if (status == OrderStatus.Initial)
                {
                    if (ls.Count == 2)
                    {
                        mailMessage = $"指定交易商品({order.name})在{order.chain_id.ToString()}链上已成功上架，特此通知！";
                    }
                    else if (ls.Contains(seller.email))
                    {
                        mailMessage = $"您在{order.chain_id.ToString()}链上发布的商品({order.name})已成功上架，如果您的商品有新动态，我们将邮件通知您！";
                    }
                    else if (ls.Contains(buyer.email))
                    {
                        mailMessage = $"一位商家在{order.chain_id.ToString()}链上发布的商品({order.name})指定您为唯一购买人！";
                    }
                }
                else if (status == OrderStatus.SellerCancelWithoutDuty)
                {
                    if (ls.Count == 2)
                    {
                        mailMessage = $"指定交易商品({order.name})在{order.chain_id.ToString()}链上已取消，特此通知！";
                    }
                    else if (ls.Contains(seller.email))
                    {
                        mailMessage = $"您在{order.chain_id.ToString()}链上发布的商品({order.name})已取消，特此通知！";
                    }
                    else if (ls.Contains(buyer.email))
                    {
                        mailMessage = $"商家在{order.chain_id.ToString()}链上发布的商品({order.name})已取消，该商品曾指定您为唯一购买人。";
                    }


                }
                else if (status == OrderStatus.Completed)
                {
                    mailMessage = $"您在{order.chain_id.ToString()}链上的商品({order.name})交易已完成，特此通知！";
                }
                else if (status == OrderStatus.ConsultCancelCompleted)
                {
                    mailMessage = $"您在{order.chain_id.ToString()}链上的商品({order.name})已经与对方协商取消交易，特此通知！";
                }
                else
                {
                    mailMessage = $"您在{order.chain_id.ToString()}链上的商品（{order.name}）有新动态，请注意查看！<br/><br/><br/>---此邮件收件人为买卖双方预留的邮箱联系方式<br/><br/><br/>---您也可以在商品详情页查看对方的其他联系方式！";
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
        [HttpPost("sendBot")]
        public async Task<JsonResult> sendBot([FromBody] SendEmailRequest request)
        {

            try
            {
                List<long> ls = new List<long>();
                var order = await _mySqlMasterDbContext.orders.FirstOrDefaultAsync(p => p.order_id == request.order_id && p.chain_id == request.chain_id && p.contract == request.contract);
                OrderStatus status = order.status;
                var seller = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(u => u.address == order.seller);
                var buyer = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(u => u.address == order.buyer);
                var orderDto = AutoMapperHelper.MapDbEntityToDTO<orders, OrderResponse>(order);
                var token = _mySqlMasterDbContext.chain_tokens.AsNoTracking().FirstOrDefault(c => c.chain_id == orderDto.chain_id && c.token_address.Equals(orderDto.token, StringComparison.OrdinalIgnoreCase));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                orderDto.token_des = tokenView;
                var botClient = new TelegramBotClient(_configuration["BotToken"]);
                string mailMessageSeller = "";
                string mailMessageBuyer = "";
                if (status == OrderStatus.Initial)
                {
                    if (seller?.telegram_id != null)
                    {
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上发布了商品：{order.name}。";
                        var chatMessage = $"市场订单：用户 @{seller?.nick_name} 在{order.chain_id.ToString()}链上发布了新商品：{orderDto.name}，单价：{orderDto.price_actual} {orderDto.token_des.token_name}, 数量：{order.amount}，订单链接:{_configuration["Domain"]}/market/detail/{order.contract}/{(int)order.chain_id}/{order.order_id}";

                        var chatIDs = _configuration["GroupChatIDs"].Split(',');
                        foreach(var chatID in chatIDs)
                        {
                            if (_configuration[chatID] == orderDto.token_des.token_name)
                            {
                                var message = await botClient.SendTextMessageAsync(long.Parse(chatID), chatMessage);
                            }
                        }
                    }
                    if (buyer?.telegram_id != null)
                    {
                        mailMessageBuyer = $"卖家(@{seller?.nick_name})在{order.chain_id.ToString()}链上发布的商品({order.name})指定您为唯一购买人。";
                    }
                }
                else if (status == OrderStatus.SellerCancelWithoutDuty)
                {
                    if (seller?.telegram_id != null)
                    {
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上发布的商品({order.name})已取消。";
                    }
                    if (buyer?.telegram_id != null)
                    {
                        mailMessageBuyer = $"卖家(@{seller?.nick_name})在{order.chain_id.ToString()}链上发布的商品({order.name})已取消，该商品曾指定您为唯一购买人。";
                    }
                }
                else if (status == OrderStatus.Completed)
                {
                    mailMessageSeller = $"您在{order.chain_id.ToString()}链上的发布的商品({order.name})交易已完成。";
                    mailMessageBuyer = $"您在{order.chain_id.ToString()}链上购买的商品({order.name})交易已完成。";
                }
                else if (status == OrderStatus.ConsultCancelCompleted)
                {
                    mailMessageSeller = $"您在{order.chain_id.ToString()}链上的发布的商品({order.name})协商取消已完成。";
                    mailMessageBuyer = $"您在{order.chain_id.ToString()}链上购买的商品({order.name})协商取消已完成。";
                }
                else
                {
                    mailMessageSeller = $"您在{order.chain_id.ToString()}链上发布的商品（{order.name}）有新动态，请及时查看。";
                    mailMessageBuyer = $"您在{order.chain_id.ToString()}链上购买的商品（{order.name}）有新动态，请及时查看。";
                }

                if (!string.IsNullOrEmpty(mailMessageSeller))
                {

                    var chatId = seller?.telegram_id; // 替换为您要发送消息的聊天ID
                    if (chatId != null)
                    {
                        var message = await botClient.SendTextMessageAsync(chatId, mailMessageSeller);
                    }
                }
                if (!string.IsNullOrEmpty(mailMessageBuyer))
                {
                    var chatId = buyer?.telegram_id; // 替换为您要发送消息的聊天ID
                    if (chatId != null)
                    {
                        var message = await botClient.SendTextMessageAsync(chatId, mailMessageBuyer);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in sendBot(): {0}", ex.ToString());
                return Json(new WebApiResult(1, "发送失败"));
            }
            return Json(new WebApiResult(1, "发送成功"));
        }

        [HttpPost("sendBotAuction")]
        public async Task<JsonResult> sendBotAuction([FromBody] SendEmailRequest request)
        {

            try
            {
                List<long> ls = new List<long>();
                var order = await _mySqlMasterDbContext.orders_auction.FirstOrDefaultAsync(p => p.order_id == request.order_id && p.chain_id == request.chain_id && p.contract == request.contract);
                OrderAuctionStatus status = order.status;
                var seller = await _mySqlMasterDbContext.users.FirstOrDefaultAsync(u => u.address == order.seller);

                var orderDto = AutoMapperHelper.MapDbEntityToDTO<orders_auction, OrderAuctionResponse>(order);
                var token = _mySqlMasterDbContext.chain_tokens.AsNoTracking().FirstOrDefault(c => c.chain_id == orderDto.chain_id && c.token_address.Equals(orderDto.token, StringComparison.OrdinalIgnoreCase));
                var tokenView = AutoMapperHelper.MapDbEntityToDTO<chain_tokens, ChainTokenViewModel>(token);
                orderDto.token_des = tokenView;
                var botClient = new TelegramBotClient(_configuration["BotToken"]);

                if (status == OrderAuctionStatus.Initial)
                {

                    var chatMessage = $"拍卖订单：用户 @{seller?.nick_name} 在{order.chain_id.ToString()}链上发布了新商品：{orderDto.name}，起拍单价：{orderDto.price_actual} {orderDto.token_des.token_name}, 数量：{order.amount}，订单链接:{_configuration["Domain"]}/auction/detail/{order.contract}/{(int)order.chain_id}/{order.order_id}";;
                    var chatIDs = _configuration["GroupChatIDs"].Split(',');
                    foreach (var chatID in chatIDs)
                    {
                        var message = await botClient.SendTextMessageAsync(long.Parse(chatID), chatMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in sendBotAuction(): {0}", ex.ToString());
                return Json(new WebApiResult(1, "发送失败"));
            }
            return Json(new WebApiResult(1, "发送成功"));
        }
        [HttpPost("cooperate")]
        public async Task<JsonResult> Cooperate([FromBody] CooperateRequest request)
        {

            try
            {
                var co = new cooperator
                {
                    status = 1,
                    create_time = DateTime.Now,
                    update_time = DateTime.Now,
                    creator = "system",
                    updater = null,
                    contact = request.contact,
                    name = request.name,
                    ip = ClientIP
                };
                await _mySqlMasterDbContext.cooperator.AddAsync(co);
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
