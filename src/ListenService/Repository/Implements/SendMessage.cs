using Nethereum.Contracts;
using CommonLibrary.DbContext;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Newtonsoft.Json;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Nethereum.ABI.Model;
using ListenService.Model;
using Nethereum.JsonRpc.Client;
using CommonLibrary.Model.DataEntityModel;
using CommonLibrary.Common.Common;
using ListenService.Repository.Interfaces;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using Nethereum.Web3;
using Nethereum.RPC;
using Org.BouncyCastle.Asn1.X509;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Telegram.Bot.Requests.Abstractions;

namespace ListenService.Repository.Implements
{
    public class SendMessage : ISendMessage
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        public SendMessage(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public async Task SendMessageEbay(long order_id, ChainEnum chain_id, string contract)
        {

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                    List<long> ls = new List<long>();
                    bool isSame = false;
                    var order = await _masterDbContext.orders.FirstOrDefaultAsync(p => p.order_id == order_id && p.chain_id == chain_id && p.contract == contract);
                    // var order_pre = await _masterDbContext.orders.FirstOrDefaultAsync(p => p.chain_id == chain_id && p.contract == contract && p.seller.Equals(order.seller) && (p.status == OrderStatus.Initial || p.status == OrderStatus.Ordered) && p.order_id < order.order_id&&p.token.Equals(order.token)&&p.img.Equals(order.img));
                    //var order_pre = await _masterDbContext.orders.FirstOrDefaultAsync(p => p.order_id < order_id && p.img.Equals(order.img)&&p.status==OrderStatus.Initial);
                    var order_pres = _masterDbContext.orders.Where(p => p.img.Equals(order.img)).ToList();
                    if (order_pres != null && order_pres.Count > 1)
                    {
                        isSame = true;
                    }
                    OrderStatus status = order.status;
                    var seller = await _masterDbContext.users.FirstOrDefaultAsync(u => u.address == order.seller);
                    var buyer = await _masterDbContext.users.FirstOrDefaultAsync(u => u.address == order.buyer);
                    var token = _masterDbContext.chain_tokens.AsNoTracking().FirstOrDefault(c => c.chain_id == order.chain_id && c.token_address.Equals(order.token, StringComparison.OrdinalIgnoreCase));
                    var botClient = new TelegramBotClient(_configuration["BotToken"]);
                    string mailMessageSeller = "";
                    string mailMessageBuyer = "";
                    if (status == OrderStatus.Initial)
                    {
                        //if (seller?.telegram_id != null)
                        //{
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上发布了订单：{order.name}。";
                        //var yajin = "单押";
                        //if (order.seller_pledge > 0)
                        //{
                        //    yajin = "双押";
                        //}
                        var chatMessage = $"卖家订单：币友 @{seller?.nick_name} 在{order.chain_id.ToString()}链上发布了新订单：{order.name}，单价：{order.price.ToString("0.000000000000000000").TrimEnd('0').TrimEnd('.')} {token.token_name}, 库存：{order.amount}，订单链接:{_configuration["Domain"]}/market/detail/{order.contract}/{(int)order.chain_id}/{order.order_id}";
                        if (!isSame)
                        {
                            await botClient.SendTextMessageAsync(_configuration["GroupChatID"], chatMessage);
                        }
                        var chatIDs = _configuration["GroupChatIDs"].Split(',');
                        foreach (var chatID in chatIDs)
                        {
                            if (chatID == _configuration["GroupChatID"])
                            {
                                continue;
                            }
                            if (_configuration[chatID] == token.token_name || token.token_name == "USDT")
                            {
                                if (!isSame)
                                {
                                    var message = await botClient.SendTextMessageAsync(long.Parse(chatID), chatMessage);
                                }
                            }
                        }

                        //}
                        if (buyer?.telegram_id != null)
                        {
                            mailMessageBuyer = $"卖家(@{seller?.nick_name})在{order.chain_id.ToString()}链上发布的订单({order.name})指定您为唯一购买人。";
                        }
                    }
                    else if (status == OrderStatus.SellerCancelWithoutDuty)
                    {
                        if (seller?.telegram_id != null)
                        {
                            mailMessageSeller = $"您在{order.chain_id.ToString()}链上发布的订单({order.name})已取消。";
                        }
                        if (buyer?.telegram_id != null)
                        {
                            mailMessageBuyer = $"卖家(@{seller?.nick_name})在{order.chain_id.ToString()}链上发布的订单({order.name})已取消，该订单曾指定您为唯一购买人。";
                        }
                    }
                    else if (status == OrderStatus.Completed)
                    {
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上的发布的订单({order.name})交易已完成。";
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上购买的订单({order.name})交易已完成。";
                    }
                    else if (status == OrderStatus.ConsultCancelCompleted)
                    {
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上的发布的订单({order.name})协商取消已完成。";
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上购买的订单({order.name})协商取消已完成。";
                    }
                    else
                    {
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上发布的订单（{order.name}）有新动态，请及时查看。\n对方Telegram：@" + buyer?.nick_name;
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上购买的订单（{order.name}）有新动态，请及时查看。\n对方Telegram： @" + seller?.nick_name;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in SendMessageEbay(): {0}", ex.ToString());

            }
        }
        public async Task SendMessagePost(long order_id, ChainEnum chain_id, string contract)
        {

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                    List<long> ls = new List<long>();
                    bool isSame = false;
                    var order = await _masterDbContext.orders.FirstOrDefaultAsync(p => p.order_id == order_id && p.chain_id == chain_id && p.contract == contract);
                    // var order_pre = await _masterDbContext.orders.FirstOrDefaultAsync(p => p.chain_id == chain_id && p.contract == contract && p.seller.Equals(order.seller) && (p.status == OrderStatus.Initial || p.status == OrderStatus.Ordered) && p.order_id < order.order_id&&p.token.Equals(order.token)&&p.img.Equals(order.img));
                    //var order_pre = await _masterDbContext.orders.FirstOrDefaultAsync(p => p.order_id < order_id && p.img.Equals(order.img)&&p.status==OrderStatus.Initial);
                    var order_pres = _masterDbContext.orders.Where(p => p.img.Equals(order.img)).ToList();
                    if (order_pres != null && order_pres.Count > 1)
                    {
                        isSame = true;
                    }
                    OrderStatus status = order.status;
                    var seller = await _masterDbContext.users.FirstOrDefaultAsync(u => u.address == order.seller);
                    var buyer = await _masterDbContext.users.FirstOrDefaultAsync(u => u.address == order.buyer);
                    var token = _masterDbContext.chain_tokens.AsNoTracking().FirstOrDefault(c => c.chain_id == order.chain_id && c.token_address.Equals(order.token, StringComparison.OrdinalIgnoreCase));
                    var botClient = new TelegramBotClient(_configuration["BotToken"]);
                    string mailMessageSeller = "";
                    string mailMessageBuyer = "";
                    if (status == OrderStatus.Initial)
                    {
                        //if (seller?.telegram_id != null)
                        //{
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上发布了新的求购：{order.name}。";

                        var chatMessage = $"买家订单：币友在{order.chain_id.ToString()}链上发布了新的求购：{order.name}，单价：{order.price.ToString("0.000000000000000000").TrimEnd('0').TrimEnd('.')} {token.token_name}, 数量：{order.amount}，买家已经提前预付了款项，订单链接:{_configuration["Domain"]}/market/post/{order.contract}/{(int)order.chain_id}/{order.order_id}";
                        if (!isSame)
                        {
                            await botClient.SendTextMessageAsync(_configuration["GroupChatID"], chatMessage);
                        }
                        var chatIDs = _configuration["GroupChatIDs"].Split(',');
                        foreach (var chatID in chatIDs)
                        {
                            if (chatID == _configuration["GroupChatID"])
                            {
                                continue;
                            }
                            if (_configuration[chatID] == token.token_name || token.token_name == "USDT")
                            {
                                if (!isSame)
                                {
                                    var message = await botClient.SendTextMessageAsync(long.Parse(chatID), chatMessage);
                                }
                            }
                        }

                        //}
                        if (buyer?.telegram_id != null)
                        {
                            mailMessageSeller = $"买家(@{seller?.nick_name})在{order.chain_id.ToString()}链上发布的求购订单({order.name})指定您为唯一出售人。";
                        }
                    }
                    else if (status == OrderStatus.SellerCancelWithoutDuty)
                    {
                        if (buyer?.telegram_id != null)
                        {
                            mailMessageBuyer = $"您在{order.chain_id.ToString()}链上发布的求购订单({order.name})已取消。";
                        }
                        if (seller?.telegram_id != null)
                        {
                            mailMessageSeller = $"买家(@{seller?.nick_name})在{order.chain_id.ToString()}链上发布的求购订单({order.name})已取消，该商品曾指定您为唯一出售人。";
                        }
                    }
                    else if (status == OrderStatus.Completed)
                    {
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上的发布的求购订单({order.name})交易已完成。";
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上出售的订单({order.name})交易已完成。";
                    }
                    else if (status == OrderStatus.ConsultCancelCompleted)
                    {
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上的发布的求购订单({order.name})协商取消已完成。";
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上出售的订单({order.name})协商取消已完成。";
                    }
                    else
                    {
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上发布的求购订单（{order.name}）有新动态，请及时查看。\n对方Telegram：@" + seller?.nick_name;
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上出售的订单（{order.name}）有新动态，请及时查看。\n对方Telegram： @" + buyer?.nick_name;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in SendMessagePost(): {0}", ex.ToString());

            }
        }
        public async Task SendMessageAuction(long order_id, ChainEnum chain_id, string contract)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                    List<long> ls = new List<long>();
                    var order = await _masterDbContext.orders_auction.FirstOrDefaultAsync(p => p.order_id == order_id && p.chain_id == chain_id && p.contract == contract);
                    OrderAuctionStatus status = order.status;
                    var seller = await _masterDbContext.users.FirstOrDefaultAsync(u => u.address == order.seller);
                    var buyer = await _masterDbContext.users.FirstOrDefaultAsync(u => u.address == order.buyer);

                    var token = _masterDbContext.chain_tokens.AsNoTracking().FirstOrDefault(c => c.chain_id == order.chain_id && c.token_address.Equals(order.token, StringComparison.OrdinalIgnoreCase));

                    var botClient = new TelegramBotClient(_configuration["BotToken"]);
                    string mailMessageSeller = "";
                    string mailMessageBuyer = "";
                    if (status == OrderAuctionStatus.Initial)
                    {
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上发布了拍卖商品：{order.name}。";
                        var chatMessage = $"拍卖订单：用户 @{seller?.nick_name} 在{order.chain_id.ToString()}链上发布了新商品：{order.name}，起拍单价：{order.price.ToString("0.000000000000000000").TrimEnd('0').TrimEnd('.')} {token.token_name}, 数量：{order.amount}，订单链接:{_configuration["Domain"]}/auction/detail/{order.contract}/{(int)order.chain_id}/{order.order_id}"; ;
                        await botClient.SendTextMessageAsync(_configuration["GroupChatID"], chatMessage);

                        var chatIDs = _configuration["GroupChatIDs"].Split(',');

                        foreach (var chatID in chatIDs)
                        {
                            if (chatID == _configuration["GroupChatID"])
                            {
                                continue;
                            }
                            if (_configuration[chatID] == token.token_name || token.token_name == "USDT")
                            {
                                var message = await botClient.SendTextMessageAsync(long.Parse(chatID), chatMessage);
                            }
                        }
                    }
                    else if (status == OrderAuctionStatus.SellerCancelWithoutDuty)
                    {
                        if (seller?.telegram_id != null)
                        {
                            mailMessageSeller = $"您在{order.chain_id.ToString()}链上发布的拍卖商品({order.name})已取消。";
                        }

                    }
                    else if (status == OrderAuctionStatus.Completed)
                    {
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上的发布的拍卖商品({order.name})交易已完成。";
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上拍下的商品({order.name})交易已完成。";
                    }
                    else if (status == OrderAuctionStatus.ConsultCancelCompleted)
                    {
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上的发布的拍卖商品({order.name})协商取消已完成。";
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上拍下的商品({order.name})协商取消已完成。";
                    }
                    else
                    {
                        mailMessageSeller = $"您在{order.chain_id.ToString()}链上发布的拍卖商品（{order.name}）有新动态，请及时查看。\n对方Telegram：@" + buyer?.nick_name;
                        mailMessageBuyer = $"您在{order.chain_id.ToString()}链上拍下的商品（{order.name}）有新动态，请及时查看。\n对方Telegram： @" + seller?.nick_name;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in sendBotAuction(): {0}", ex.ToString());
            }


        }
    }
}

