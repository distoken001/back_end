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
                telegram_user telegramUser;
                switch (update.Type)
                {
                    case UpdateType.Unknown:
                        break;
                    case UpdateType.Message:
                        sb.AppendLine("➡️ 请选择您想要完成的操作 ");
                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "DeMarket德玛市场", url: "https://demarket.io/"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "获取验证码绑定DeMarket", callbackData: "Bind")
                },

                });
                         telegramUser= _masterDbContext.telegram_user.Where(a => a.user_name == update.Message.Chat.Username).FirstOrDefault();
                        if (telegramUser == null)
                        {
                            _masterDbContext.telegram_user.Add(
                                new telegram_user()
                                {
                                    user_name = update.Message.Chat.Username,
                                    chat_id = update.Message.Chat.Id,
                                    create_time = DateTime.Now,
                                    update_time = DateTime.Now,
                                    verify_code = ""
                                });
                        }
                        else
                        {
                            telegramUser.chat_id = update.Message.Chat.Id;
                        }
                        _masterDbContext.SaveChanges();
                        result = await botClient.SendTextMessageAsync(
                              chatId: new ChatId(update.Message.Chat.Id),
                              text: sb.ToString(),
                              parseMode: ParseMode.Markdown,
                              replyMarkup: inlineKeyboard);
                        break;
                    case UpdateType.InlineQuery:
                        break;
                    case UpdateType.ChosenInlineResult:
                        break;
                    case UpdateType.CallbackQuery:
                        if (update.CallbackQuery.Data == "Bind")
                        {
                            sb.Append("您的验证码是： ");
                            Random random = new Random();
                            int randomNumber = random.Next(100000, 999999); // 生成6位随机数字
                            sb.Append(randomNumber.ToString());

                            telegramUser = _masterDbContext.telegram_user.Where(a => a.chat_id == update.CallbackQuery.From.Id).FirstOrDefault();
                            if (telegramUser == null)
                            {
                                _masterDbContext.telegram_user.Add(
                                    new telegram_user()
                                    {
                                        user_name = update.CallbackQuery.From.Username,
                                        chat_id = update.CallbackQuery.From.Id,
                                        create_time = DateTime.Now,
                                        update_time = DateTime.Now,
                                        verify_code = "",
                                        count = 0
                                    });
                            }
                            else
                            {
                                telegramUser.update_time = DateTime.Now;
                                telegramUser.verify_code = randomNumber.ToString();
                                telegramUser.count = 0;
                            }
                            _masterDbContext.SaveChanges();
                            result = await botClient.SendTextMessageAsync(
                                  chatId: update.CallbackQuery.Message.Chat.Id,
                                  text: sb.ToString(),
                                  parseMode: ParseMode.MarkdownV2);
                            Console.WriteLine(result.ToString());
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
