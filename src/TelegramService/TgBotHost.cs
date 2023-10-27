using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramService.Service;
using CommonLibrary.DbContext;
using CommonLibrary.Model.DataEntityModel;
using Microsoft.Extensions.Primitives;

namespace TelegramService
{
    public class TgBotHost : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public TgBotHost(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var botClient = new TelegramBotClient(_configuration["BotToken"]); // 使用申请的 Token 创建机器人
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 消息处理方法
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                Message result;
                var sb = new StringBuilder();
                telegram_user_chat telegramUserChat;
                switch (update.Type)
                {
                    case UpdateType.Unknown:
                        break;
                    case UpdateType.Message:

                        if (update.Message.Type == MessageType.ChatMembersAdded || update.Message.Text.Equals("绑定") || update.Message.Text.Equals("Bind", StringComparison.OrdinalIgnoreCase) || update.Message.Text.Equals("@" + _configuration["BotUserName"]) || update.Message.Chat.Id > 0)
                        {
                            sb.AppendLine("很高兴遇见你！ "+update.Message.From.Username.Replace("_", @"\_"));
                            var obj = new[]
                            {
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "DeMarket德玛市场", url: "https://demarket.io/"),
                },
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "Twitter推特", url: "https://twitter.com/demarket_io"),
                } };
                            if (update.Message.Chat.Id > 0)
                            {
                                sb.AppendLine("不要删除我哦，我是DeMarket通知机器人，您的相关订单动态我会第一时间通知您！");
                                obj = obj.Concat(new[]{new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "获取绑定验证码", callbackData: "Bind") }
                }).ToArray();
                            }
                            else
                            {
                                obj = obj.Concat(new[]{new[]
                {
                    InlineKeyboardButton.WithUrl(text: "获取绑定验证码", url: @"https://t.me/"+_configuration["BotUserName"]) }
                }).ToArray();
                                                           }                        
                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(obj);
                        telegramUserChat = _masterDbContext.telegram_user_chat.Where(a => a.chat_id == update.Message.Chat.Id).FirstOrDefault();
                        if (telegramUserChat == null)
                        {
                            _masterDbContext.telegram_user_chat.Add(
                                new telegram_user_chat()
                                {
                                    user_name = update.Message.From.Username,
                                    user_id = update.Message.From.Id,
                                    chat_id = update.Message.Chat.Id,
                                    create_time = DateTime.Now,
                                    update_time = DateTime.Now,
                                    verify_code = ""
                                });
                        }
                        else
                        {
                            telegramUserChat.user_name = update.Message.From.Username;
                            telegramUserChat.user_id = update.Message.From.Id;
                        }
                        _masterDbContext.SaveChanges();
                        result = await botClient.SendTextMessageAsync(
                              chatId: new ChatId(update.Message.Chat.Id),
                              text: sb.ToString(),
                              parseMode: ParseMode.Markdown,
                              replyMarkup: inlineKeyboard);
                }
            
                        break;
                    case UpdateType.InlineQuery:
                        break;
                    case UpdateType.ChosenInlineResult:
                        break;
                    case UpdateType.CallbackQuery:
                        if (update.CallbackQuery.Data == "Bind")
                        {
                            string modifiedString = update.CallbackQuery.From.Username.Replace("_", @"\_");
                            sb.Append(" 您的验证码是： ");
                            sb.Append("*");
                            Random random = new Random();
                            int randomNumber = random.Next(10000000, 99999999); // 生成8位随机数字
                            sb.Append(randomNumber.ToString());
                            sb.Append("*");
                            sb.Append("  有效期五分钟");
                            telegramUserChat = _masterDbContext.telegram_user_chat.Where(a => a.chat_id == update.CallbackQuery.Message.Chat.Id).FirstOrDefault();
                            if (telegramUserChat == null)
                            {
                                return;
                            }
                            else
                            {
                                telegramUserChat.user_name = update.CallbackQuery.Message.Chat.Username;
                                telegramUserChat.update_time = DateTime.Now;
                                telegramUserChat.verify_code = randomNumber.ToString();
                                telegramUserChat.count = 0;
                            }
                            _masterDbContext.SaveChanges();
                            if (_configuration["GroupChatID"] == update.CallbackQuery.Message.Chat.Id.ToString())
                            {
                                await botClient.SendTextMessageAsync(
                            chatId: update.CallbackQuery.Message.Chat.Id,
                            text: "@" + modifiedString + " *私聊我立即获取绑定验证码*",
                            parseMode: ParseMode.MarkdownV2);

                            }

                            await botClient.SendTextMessageAsync(
                                  chatId: telegramUserChat.user_id,
                                  text: sb.ToString(),
                                  parseMode: ParseMode.MarkdownV2);

                        }
                        break;
                    case UpdateType.EditedMessage:
                        break;
                    case UpdateType.ChannelPost:
                        break;
                    case UpdateType.EditedChannelPost:
                        break;
                    case UpdateType.ShippingQuery:
                        break;
                    case UpdateType.PreCheckoutQuery:
                        break;
                    case UpdateType.Poll:
                        break;
                    case UpdateType.PollAnswer:
                        break;
                    case UpdateType.MyChatMember:
                        break;
                    case UpdateType.ChatMember:
                        break;
                    case UpdateType.ChatJoinRequest:
                        break;
                    default:
                        break;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;

            }
        }
        /// 异常处理方法            /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
