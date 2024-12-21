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
using System.Diagnostics;

namespace TelegramService
{
    public class TgBotHost : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        public TgBotHost(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _masterDbContext = scope.ServiceProvider.GetRequiredService<MySqlMasterDbContext>();
                    switch (update.Type)
                    {
                        case UpdateType.Unknown: break;
                        case UpdateType.Message:
                            switch (update.Message.Type)
                            {

                                case MessageType.ChatMemberLeft:
                                    break;
                                case MessageType.ChatMembersAdded:
                                    Console.WriteLine("有人进来" + update.Message.Chat.Id.ToString());
                                    if (update.Message.Chat.Id.ToString() == _configuration["GroupChatID"])
                                    {

                                        if (string.IsNullOrEmpty(update.Message.From.Username))
                                        {
                                            var message = await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Hey，" + update.Message.From.FirstName + "，您需要先设置Telegram用户名才能与DeMarket绑定哦");
                                            return;
                                        }
                                        else
                                        {
                                            sb.AppendLine("很高兴遇见你！ @" + update.Message.From.Username.Replace("_", @"\_"));
                                            var obj = new[]
                        {
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "DeMarket", url: "https://demarket.io/"),
                },
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "Twitter", url: "https://twitter.com/demarket_io"),
                },

                       new[]
                {
                    InlineKeyboardButton.WithUrl(text: "进入Debox社群", url: @"https://m.debox.pro/group?id="+_configuration["Debox:Group"])
                  },
                  new[]
                {
                    InlineKeyboardButton.WithUrl(text: "获取绑定验证码", url: @"https://t.me/"+_configuration["BotUserName"])
                }
                                    };
                                            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(obj);
                                            result = await botClient.SendTextMessageAsync(
                                              chatId: new ChatId(update.Message.Chat.Id),
                                              text: sb.ToString(),
                                              parseMode: ParseMode.Markdown,
                                              replyMarkup: inlineKeyboard);

                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(update.Message.From.Username))
                                        {
                                            return;
                                        }
                                        sb.AppendLine("很高兴遇见你！ @" + update.Message.From.Username.Replace("_", @"\_"));
                                        string coin = _configuration[update.Message.Chat.Id.ToString()] == null ? "此币" : _configuration[update.Message.Chat.Id.ToString()];

                                        var obj = new[]
                                        {
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "用"+coin+"交易商品", url: "https://demarket.io/"),
                },
                  new[]
                {
                    InlineKeyboardButton.WithUrl(text: "获取绑定验证码", url: @"https://t.me/"+_configuration["BotUserName"])
                }
                            };

                                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(obj);
                                        result = await botClient.SendTextMessageAsync(
                                              chatId: new ChatId(update.Message.Chat.Id),
                                              text: sb.ToString(),
                                              parseMode: ParseMode.Markdown,
                                              replyMarkup: inlineKeyboard);
                                    }
                                    break;
                                case MessageType.Text:
                                    if (update.Message.Chat.Id < 0)
                                    {
                                        if (update.Message.Text.Contains(_configuration["BotUserName"]) || update.Message.Text.Contains("验证码") || update.Message.Text.Contains("绑定"))
                                        {
                                            if (string.IsNullOrEmpty(update.Message.From.Username))
                                            {
                                                if (update.Message.Chat.Id.ToString() == _configuration["GroupChatID"])
                                                {
                                                    var message = await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Hey，" + update.Message.From.FirstName + "，您需要先设置用户名才能与DeMarket绑定哦");
                                                    return;
                                                }
                                                else
                                                {
                                                    var message = await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Hey，" + update.Message.From.FirstName + "，您没有设置用户名，不能与我沟通哦～");
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                if (update.Message.Chat.Id.ToString() == _configuration["GroupChatID"])
                                                {

                                                    sb.AppendLine("Hey！靓仔！@" + update.Message.From.Username.Replace("_", @"\_"));
                                                    var obj = new[]
                                          {                new []
                {                    InlineKeyboardButton.WithUrl(text: "DeMarket", url: "https://demarket.io/"),
                },
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "Twitter", url: "https://twitter.com/demarket_io"),
                },
                       new[]
                {
                    InlineKeyboardButton.WithUrl(text: "进入Debox社群", url: @"https://m.debox.pro/group?id="+_configuration["Debox:Group"])
                  },

                //  new[]
                //{
                //    InlineKeyboardButton.WithUrl(text: "Telegram电报", url: @"https://t.me/"+_configuration["ChatGroup"])
                //}
                        };

                                                    obj = obj.Concat(new[]{new[]
                {
                    InlineKeyboardButton.WithUrl(text: "获取绑定验证码", url: @"https://t.me/"+_configuration["BotUserName"]) }
                }).ToArray();

                                                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(obj);
                                                    result = await botClient.SendTextMessageAsync(
                                                          chatId: new ChatId(update.Message.Chat.Id),
                                                          text: sb.ToString(),
                                                          parseMode: ParseMode.Markdown,
                                                          replyMarkup: inlineKeyboard);

                                                }
                                                else
                                                {
                                                    sb.AppendLine("很高兴遇见你！ @" + update.Message.From.Username.Replace("_", @"\_"));
                                                    string coin = _configuration[update.Message.Chat.Id.ToString()] == null ? "此币" : _configuration[update.Message.Chat.Id.ToString()];
                                                    var obj = new[]
                                                    {
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "用"+coin+"交易商品", url: "https://demarket.io/"),
                },
                  new[]
                {
                    InlineKeyboardButton.WithUrl(text: "获取绑定验证码", url: @"https://t.me/"+_configuration["BotUserName"])
                }
                            };

                                                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(obj);
                                                    result = await botClient.SendTextMessageAsync(
                                                          chatId: new ChatId(update.Message.Chat.Id),
                                                          text: sb.ToString(),
                                                          parseMode: ParseMode.Markdown,
                                                          replyMarkup: inlineKeyboard);
                                                }
                                            }
                                        }
                                        return;
                                    }

                                    //else  (update.Message.Text.Equals("绑定") || update.Message.Text.Equals("Bind", StringComparison.OrdinalIgnoreCase) || update.Message.Text.Equals("DeMarket", StringComparison.OrdinalIgnoreCase) || update.Message.Text.Equals("德玛", StringComparison.OrdinalIgnoreCase) || update.Message.Text.Equals("@" + _configuration["BotUserName"]) || update.Message.Chat.Id > 0 || !string.IsNullOrEmpty(update.Message.ReplyToMessage.Text))
                                    else
                                    {
                                        if (string.IsNullOrEmpty(update.Message.From.Username))
                                        {

                                            var message = await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Hey，" + update.Message.From.FirstName + "，您需要先设置Telegram用户名才能与DeMarket绑定哦");
                                            return;
                                        }
                                        sb.AppendLine("Hey！靓仔！@" + update.Message.From.Username.Replace("_", @"\_"));
                                        sb.AppendLine("与您相关的订单动态我会第一时间通知您，如果您修改了Telegram用户名，务必去DeMarket个人中心重新绑定！");
                                        sb.AppendLine("");
                                        var obj = new[]
                                        {                new []
                {                    InlineKeyboardButton.WithUrl(text: "DeMarket", url: "https://demarket.io/"),
                },
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "Twitter", url: "https://twitter.com/demarket_io"),
                },

                  new[]
                {
                    InlineKeyboardButton.WithUrl(text: "进入Telegram社群", url: @"https://t.me/"+_configuration["ChatGroup"])
                  },
                       new[]
                {
                    InlineKeyboardButton.WithUrl(text: "进入Debox社群", url: @"https://m.debox.pro/group?id="+_configuration["Debox:Group"])
                  },

                        };

                                        obj = obj.Concat(new[]{new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: "获取绑定验证码", callbackData: "Bind") }
                }).ToArray();

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
                                        await _masterDbContext.SaveChangesAsync();
                                        result = await botClient.SendTextMessageAsync(
                                              chatId: new ChatId(update.Message.Chat.Id),
                                              text: sb.ToString(),
                                              parseMode: ParseMode.Markdown,
                                              replyMarkup: inlineKeyboard);
                                    }
                                    break;
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
                                sb.Append("  有效期五分钟，");
                                sb.Append("验证码属于个人隐私，为了防止被其他人冒用，切记不要泄露。如若您修改了Telegram用户名，务必重新绑定！");
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
                                    telegramUserChat.state = 1;
                                }
                                await _masterDbContext.SaveChangesAsync();
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
                            if (update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Member)
                            {
                                telegramUserChat = _masterDbContext.telegram_user_chat.Where(a => a.chat_id == update.MyChatMember.From.Id).FirstOrDefault();
                                if (telegramUserChat == null)
                                {
                                    _masterDbContext.telegram_user_chat.Add(
                                        new telegram_user_chat()
                                        {
                                            user_name = update.MyChatMember.From.Username,
                                            user_id = update.MyChatMember.From.Id,
                                            chat_id = update.MyChatMember.Chat.Id,
                                            create_time = DateTime.Now,
                                            update_time = DateTime.Now,
                                            verify_code = ""
                                        });
                                }
                                else
                                {
                                    telegramUserChat.user_name = update.MyChatMember.From.Username;
                                    telegramUserChat.user_id = update.MyChatMember.From.Id;
                                }
                            }
                            else if (update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Kicked)
                            {
                                var list = _masterDbContext.users.Where(a => a.telegram_id == update.MyChatMember.From.Id).ToList();
                                list.ForEach(a => { a.nick_name = null; a.telegram_id = null; });
                            }

                            await _masterDbContext.SaveChangesAsync();
                            break;
                        case UpdateType.ChatMember:
                            break;
                        case UpdateType.ChatJoinRequest:
                            break;
                        default:
                            break;
                    }
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
