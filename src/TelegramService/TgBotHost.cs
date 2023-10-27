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

namespace TelegramService
{
    public class TgBotHost : IHostedService
    {
        private readonly IConfiguration _configuration;

        public TgBotHost(IConfiguration configuration)
        {
            _configuration = configuration;
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
            // 消息的类型有多种，最常见的是文本型 UpdateType.Message
            switch (update.Type)
            {
                case UpdateType.Unknown:
                    break;
                case UpdateType.Message:
                    Console.WriteLine(update?.Message?.Text); // 将受到的文本消息输出到控制台
                    if (string.IsNullOrEmpty(update?.Message?.Text))
                        return;
                    //组织开始回复消息模板
                    var sb = new StringBuilder();
                    sb.AppendLine("➡️ 请选择您想要完成的操作 ");
                    //sb.AppendLine("");
                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                // first row
                new []
                {
                    InlineKeyboardButton.WithUrl(text: "DeMarket德玛市场", url: "https://demarket.io/"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "获取验证码绑定DeMarket", callbackData: "Bind")
                },

                });

                    var reMsg = await botClient.SendTextMessageAsync(
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

        /// <summary>
        /// 异常处理方法
        /// </summary>
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
