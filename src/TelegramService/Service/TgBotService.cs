
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramService.Service
{
    public class TgBotService 
    {

        //        private readonly IRedisService redisService;
        //        private readonly IIdProvideService idProvideService;
        //        private readonly TrendingContext trendingContext;
        //        private readonly ILogRecorder<BuyTrendingBotService> logger;
        //        private readonly TgBotConfigOption tgBotConfigOption;
        //        private readonly Web3ConfigOption web3ConfigOption;



        //        public BuyTrendingBotService(
        //            IRedisService redisService,
        //            IIdProvideService idProvideService,
        //            TrendingContext trendingContext,
        //            IOptions<TgBotConfigOption> options,
        //            IOptions<Web3ConfigOption> web3Options,
        //             ILogRecorder<BuyTrendingBotService> logger)
        //        {
        //            this.redisService = redisService;
        //            this.idProvideService = idProvideService;
        //            this.trendingContext = trendingContext;
        //            this.logger = logger;
        //            this.tgBotConfigOption = options.Value;
        //            this.web3ConfigOption = web3Options.Value;
        //        }

        //        public async Task ReceiveProcessSetEmojiCallBack(CallbackQuery callbackQuery)
        //        {
        //            var stepEntity = await trendingContext.CqBuyTrendingBotStep.FirstOrDefaultAsync(p => p.ChartId == callbackQuery.Message.Chat.Id);
        //            if (stepEntity != null && stepEntity.LastStep == (int)BuyTrendingBotStep.提交付款hash)
        //            {
        //                var sb = new StringBuilder();
        //                sb.AppendLine($"➡️ Please Click Replay new emoji ");

        //                await trendingContext.SaveChangesAsync();
        //                var botClient = new TelegramBotClient(tgBotConfigOption.Token);
        //                var reMsg = await botClient.SendTextMessageAsync(
        //                      chatId: new ChatId(callbackQuery.Message.Chat.Id),
        //                      text: sb.ToString(),
        //                      parseMode: ParseMode.Markdown);
        //            }
        //        }

        //        public async Task<TextCommandViewModel> ReceiveProcessTextCommand(Message message)
        //        {
        //            var res = new TextCommandViewModel();
        //            var stepEntity = await trendingContext.CqBuyTrendingBotStep.FirstOrDefaultAsync(p => p.ChartId == message.Chat.Id);
        //            if (stepEntity != null)
        //            {
        //                //处理输入token地址
        //                if ((BuyTrendingBotStep)stepEntity.LastStep == BuyTrendingBotStep.选择链)
        //                    return await PrivateProcessConfirmTokenText(message, stepEntity);

        //                //处理输入电报群组地址
        //                if ((BuyTrendingBotStep)stepEntity.LastStep == BuyTrendingBotStep.输入Token)
        //                    return await PrivateProcessConfirmTgUrlText(message, stepEntity);

        //                //处理输入交易hash
        //                if ((BuyTrendingBotStep)stepEntity.LastStep == BuyTrendingBotStep.选择趋势时长)
        //                    return await PrivateProcessConfirmPaymentTxText(message, stepEntity);
        //                //处理设置表情
        //                if ((BuyTrendingBotStep)stepEntity.LastStep == BuyTrendingBotStep.提交付款hash)
        //                    return await PrivateProcessSetEmojiText(message, stepEntity.TokenAddress);

        //            }
        //            else
        //            {
        //                return res;
        //            }
        //            return res;
        //        }



        //        public async Task ReceiveProcessBackToOptionCallBack(CallbackQuery callbackQuery)
        //        {
        //            var stepEntity = await trendingContext.CqBuyTrendingBotStep.FirstOrDefaultAsync(p => p.ChartId == callbackQuery.Message.Chat.Id);
        //            if (stepEntity != null)
        //            {
        //                var optionFomate = callbackQuery.Data.Split("-")[1];
        //                if (optionFomate == "EnterToken")
        //                {
        //                    //返回到输入token
        //                    var chainTypeFomate = "";
        //                    if (stepEntity.ChainType == 1)
        //                        chainTypeFomate = "ETH";
        //                    if (stepEntity.ChainType == 2)
        //                        chainTypeFomate = "BSC";
        //                    stepEntity.LastStep = (int)BuyTrendingBotStep.选择链;
        //                    stepEntity.UpdateTime = DateTime.Now;

        //                    //组织开始回复消息模板
        //                    var sb = new StringBuilder();
        //                    sb.AppendLine($"➡️ Please Enter [{chainTypeFomate}] Token Address ");

        //                    var botClient = new TelegramBotClient(tgBotConfigOption.Token);

        //                    var reMsg = await botClient.SendTextMessageAsync(
        //                          chatId: new ChatId(callbackQuery.Message.Chat.Id),
        //                          text: sb.ToString(),
        //                          parseMode: ParseMode.Markdown);
        //                    await trendingContext.SaveChangesAsync();

        //                }

        //                if (optionFomate == "Start")
        //                {
        //                    //返回到重新开始
        //                    await ReceiveProcessStartCommand(callbackQuery.Message);
        //                }
        //            }
        //        }

        //        public async Task ReceiveProcessSelectChainCallBack(CallbackQuery callbackQuery)
        //        {
        //            var stepEntity = await trendingContext.CqBuyTrendingBotStep.FirstOrDefaultAsync(p => p.ChartId == callbackQuery.Message.Chat.Id);
        //            if (stepEntity != null)
        //            {
        //                var selectedChain = callbackQuery.Data.Split("-")[1];
        //                var chainType = 0;
        //                if (selectedChain == "ETH")
        //                    chainType = 1;
        //                if (selectedChain == "BSC")
        //                    chainType = 2;
        //                stepEntity.ChainType = chainType;
        //                stepEntity.LastStep = (int)BuyTrendingBotStep.选择链;
        //                stepEntity.UpdateTime = DateTime.Now;

        //                //组织开始回复消息模板
        //                //如果是BSC，增加提示，必须是博饼v2、wbnb做为主交易对
        //                var tip = "";
        //                if (selectedChain == "BSC")
        //                    tip = ",❕ Dear Please make sure your pair from PancakeSwapV2 and Dominated by wbnb.";

        //                var sb = new StringBuilder();
        //                sb.AppendLine($"➡️ Please Enter [{selectedChain}] Token Address {tip}");

        //                var botClient = new TelegramBotClient(tgBotConfigOption.Token);
        //                //InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        //                //{
        //                //// first row
        //                //new []
        //                //{
        //                //    InlineKeyboardButton.WithCallbackData(text: "⬅️ Return to the previous operation", callbackData: "ReturnBack-SelectChian"),
        //                //}

        //                //});

        //                var reMsg = await botClient.SendTextMessageAsync(
        //                      chatId: new ChatId(callbackQuery.Message.Chat.Id),
        //                      text: sb.ToString(),
        //                      parseMode: ParseMode.Markdown);
        //                await trendingContext.SaveChangesAsync();

        //            }
        //        }

        //        public async Task ReceiveProcessStartCallBack(CallbackQuery callbackQuery)
        //        {
        //            var stepEntity = await trendingContext.CqBuyTrendingBotStep.FirstOrDefaultAsync(p => p.ChartId == callbackQuery.Message.Chat.Id);
        //            if (stepEntity != null)
        //            {
        //                //组织开始回复消息模板
        //                var sb = new StringBuilder();
        //                sb.AppendLine("➡️ Select a chain option to continue ");

        //                var botClient = new TelegramBotClient(tgBotConfigOption.Token);
        //                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        //                {
        //                // first row
        //                new []
        //                {
        //                    InlineKeyboardButton.WithCallbackData(text: "Ethereum(ETH)", callbackData: "SelectChian-ETH"),
        //                },
        //                new []
        //                {
        //                    InlineKeyboardButton.WithCallbackData(text: "Binance Smart Chain(BSC)", callbackData: "SelectChian-BSC")
        //                },

        //                });

        //                var reMsg = await botClient.SendTextMessageAsync(
        //                      chatId: new ChatId(callbackQuery.Message.Chat.Id),
        //                      text: sb.ToString(),
        //                      parseMode: ParseMode.Markdown,
        //                      replyMarkup: inlineKeyboard);
        //            }

        //        }

        //        public async Task ReceiveProcessStartCommand(Message message)
        //        {
        //            var stepEntity = await trendingContext.CqBuyTrendingBotStep.FirstOrDefaultAsync(p => p.ChartId == message.Chat.Id);
        //            if (stepEntity == null)
        //            {
        //                stepEntity = new CqBuyTrendingBotStep()
        //                {
        //                    LastStep = (int)BuyTrendingBotStep.开始,
        //                    ChartId = message.Chat.Id,
        //                    CreateTime = DateTime.Now,
        //                    UpdateTime = DateTime.Now,
        //                    TgFromName = $"{message.From?.FirstName} {message.From.LastName}",
        //                };
        //                await trendingContext.CqBuyTrendingBotStep.AddAsync(stepEntity);
        //            }
        //            else
        //            {
        //                stepEntity.UpdateTime = DateTime.Now;
        //                stepEntity.LastStep = (int)BuyTrendingBotStep.开始;
        //            }

        //            //组织开始回复消息模板
        //            var sb = new StringBuilder();
        //            sb.AppendLine("🥇 Trending Fast Track ");
        //            sb.AppendLine("");
        //            sb.AppendLine("-Start by chose chain and sending token address ");
        //            sb.AppendLine("-We don't need to put our buyTechBot in your token's group chat");
        //            sb.AppendLine("");
        //            sb.AppendLine("Fast Tracking does NOT guarantee immediate listing, it only guarantees listing the next available spot");

        //            var botClient = new TelegramBotClient(tgBotConfigOption.Token);
        //            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        //            {
        //                // first row
        //                new []
        //                {
        //                    InlineKeyboardButton.WithCallbackData(text: "➡️CLICK TO START", callbackData: "toChoseChain"),
        //                },

        //            });

        //            var reMsg = await botClient.SendTextMessageAsync(
        //                  chatId: new ChatId(message.Chat.Id),
        //                  text: sb.ToString(),
        //                  parseMode: ParseMode.Markdown,
        //                  replyMarkup: inlineKeyboard);

        //            await trendingContext.SaveChangesAsync();
        //        }

        //        public async Task ReceiveProcessChoseChatCallBack(CallbackQuery callbackQuery)
        //        {
        //            var stepEntity = await trendingContext.CqBuyTrendingBotStep.FirstOrDefaultAsync(p => p.ChartId == callbackQuery.Message.Chat.Id);
        //            if (stepEntity != null)
        //            {
        //                stepEntity.LastStep = (int)BuyTrendingBotStep.设置tg名称;
        //                stepEntity.UpdateTime = DateTime.Now;

        //                var menus = await trendingContext.CqAmountMenus.Where(p => p.ChainType == stepEntity.ChainType).ToListAsync();
        //                //组织消息模板
        //                var chainTypeFomate = "";
        //                if (stepEntity.ChainType == 1)
        //                    chainTypeFomate = "ETH";
        //                if (stepEntity.ChainType == 2)
        //                    chainTypeFomate = "BSC";
        //                var sb = new StringBuilder();
        //                sb.AppendLine($"✅ Token:{stepEntity.TokenName} ({chainTypeFomate})");
        //                sb.AppendLine($"✅ Group:{stepEntity.ToTgName}");
        //                sb.AppendLine($"🔥 Auto Top Trending Ranking(1-18)  ");
        //                sb.AppendLine("");

        //                sb.AppendLine("🆓 __4 Hours=15% Discount__");
        //                sb.AppendLine("🆓 __6 Hours=20% Discount__");
        //                sb.AppendLine("🆓 __12 Hours=30% Discount__");
        //                sb.AppendLine("🆓 __24 Hours=30% Discount__");
        //                sb.AppendLine("");
        //                sb.AppendLine("➡️ Select promotion length. ");

        //                var buttons = new List<List<InlineKeyboardButton>>();
        //                foreach (var item in menus)
        //                {
        //                    buttons.Add(new List<InlineKeyboardButton> {
        //                        InlineKeyboardButton.WithCallbackData(text: $"{item.Hour} Hours : {item.Amount} {item.Unit}", callbackData: $"ChoseHour-{item.Hour}")
        //                    });
        //                }
        //                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

        //                var botClient = new TelegramBotClient(tgBotConfigOption.Token);
        //                await trendingContext.SaveChangesAsync();
        //                var reMsg = await botClient.SendTextMessageAsync(
        //                     chatId: new ChatId(callbackQuery.Message.Chat.Id),
        //                     text: sb.ToString(),
        //                     parseMode: ParseMode.Markdown,
        //                     replyMarkup: inlineKeyboard);
        //            }
        //        }


        //        public async Task ReceiveProcessChoseHoursCallBack(CallbackQuery callbackQuery)
        //        {
        //            var stepEntity = await trendingContext.CqBuyTrendingBotStep.FirstOrDefaultAsync(p => p.ChartId == callbackQuery.Message.Chat.Id);
        //            if (stepEntity != null)
        //            {
        //                //获取选择时长
        //                var hours = Convert.ToInt32(callbackQuery.Data.Split("-")[1]);
        //                //获取价格类目模型
        //                var menu = await trendingContext.CqAmountMenus.FirstOrDefaultAsync(p => p.ChainType == stepEntity.ChainType && p.Hour == hours);
        //                if (menu != null)
        //                {
        //                    stepEntity.UpdateTime = DateTime.Now;
        //                    stepEntity.LastStep = (int)BuyTrendingBotStep.选择趋势时长;
        //                    stepEntity.TrendingHours = hours;

        //                    //组织消息模板
        //                    var chainTypeFomate = "";
        //                    var paymentAddress = "";
        //                    if (stepEntity.ChainType == 1)
        //                    {
        //                        chainTypeFomate = "ETH";
        //                        paymentAddress = "0x86620EB5C1feb9b8c80411911f99863Fa5bF34D6";
        //                    }

        //                    if (stepEntity.ChainType == 2)
        //                    {
        //                        chainTypeFomate = "BSC";
        //                        paymentAddress = "0x0cD29E8161194BB034B958635FFFd4cf39B8901d";
        //                    }

        //                    var sb = new StringBuilder();
        //                    sb.AppendLine("🥇 Trending Fast Track");
        //                    sb.AppendLine("");
        //                    sb.AppendLine($"✅ Token:{stepEntity.TokenName} ({chainTypeFomate})");
        //                    sb.AppendLine($"✅ Group:{stepEntity.ToTgName}");
        //                    sb.AppendLine($"🔥 Auto Top Trending Ranking(1-18)  ");
        //                    sb.AppendLine($"⏱ Lentgh:{hours} Hours ");
        //                    sb.AppendLine($"💰 Price:{menu.Amount} {menu.Unit}");
        //                    sb.AppendLine("");
        //                    sb.AppendLine($"⬇️ Your Payment Wallet (to copy)");
        //                    sb.AppendLine($"{paymentAddress}");//回头动态
        //                    sb.AppendLine("");
        //                    sb.AppendLine($"➡️ Send {menu.Amount} {menu.Unit} to the payment wallet above. Then click SEND PAYMENT TX and send transaction hash.");
        //                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        //                        {
        //                         new []
        //                            {
        //                                InlineKeyboardButton.WithCallbackData(text: $"➡️SEND PAYMENT TX ({menu.Amount} {menu.Unit})", callbackData: "TodSendTx"),
        //                            },
        //                            new []
        //                            {
        //                                InlineKeyboardButton.WithCallbackData(text: "🔙Go Back To Length Options", callbackData: "ToSelectLength"),
        //                            }

        //                        });

        //                    await trendingContext.SaveChangesAsync();

        //                    var botClient = new TelegramBotClient(tgBotConfigOption.Token);
        //                    var reMsg = await botClient.SendTextMessageAsync(
        //                        chatId: new ChatId(callbackQuery.Message.Chat.Id),
        //                        text: sb.ToString(),
        //                        parseMode: ParseMode.Markdown,
        //                        replyMarkup: inlineKeyboard);
        //                }


        //            }
        //        }

        //        public async Task ReceiveProcessBackToChoseHourCallBack(CallbackQuery callbackQuery)
        //        {
        //            await this.ReceiveProcessChoseChatCallBack(callbackQuery);
        //        }

        //        public async Task ReceiveProcessSendPaymentTxCallBack(CallbackQuery callbackQuery)
        //        {
        //            var stepEntity = await trendingContext.CqBuyTrendingBotStep.FirstOrDefaultAsync(p => p.ChartId == callbackQuery.Message.Chat.Id);
        //            if (stepEntity != null && stepEntity.LastStep == (int)BuyTrendingBotStep.选择趋势时长)
        //            {
        //                var sb = new StringBuilder();
        //                sb.AppendLine("➡️ Send transaction hash ");
        //                var botClient = new TelegramBotClient(tgBotConfigOption.Token);
        //                var reMsg = await botClient.SendTextMessageAsync(
        //                    chatId: new ChatId(callbackQuery.Message.Chat.Id),
        //                    text: sb.ToString(),
        //                    parseMode: ParseMode.Markdown);
        //            }

        //        }


        //        private async Task<TextCommandViewModel> PrivateProcessConfirmTgUrlText(Message message, CqBuyTrendingBotStep stepEntity)
        //        {
        //            var chainTypeFomate = "";
        //            if (stepEntity.ChainType == 1)
        //                chainTypeFomate = "ETH";
        //            if (stepEntity.ChainType == 2)
        //                chainTypeFomate = "BSC";
        //            var res = new TextCommandViewModel();
        //            var groupUserName = Web3Util.ExtractGroupUsernameFromUrl(message.Text);
        //            if (string.IsNullOrEmpty(groupUserName))
        //            {
        //                res.Text = $"❌ Telegram Group Link not valid ({message.Text}). Try again";
        //                return res;
        //            }
        //            var botClient = new TelegramBotClient(tgBotConfigOption.Token);
        //            if (groupUserName.Contains("@"))
        //                groupUserName.Replace("@", "");
        //            groupUserName = groupUserName.Replace("%20", "");
        //            try
        //            {
        //                var groupInfo = await botClient.GetChatAsync(new ChatId($"@{groupUserName}"));
        //                stepEntity.LastStep = (int)BuyTrendingBotStep.设置tg名称;
        //                stepEntity.UpdateTime = DateTime.Now;
        //                //stepEntity.ToTgName = groupInfo.Title;
        //                stepEntity.ToTgUrl = message.Text;
        //                await trendingContext.SaveChangesAsync();

        //                //这一步利用N''，不然表情乱码
        //                await trendingContext.Database.ExecuteSqlCommandAsync("update [Cq_BuyTrendingBotStep] set ToTgName=N'" + groupInfo.Title + "' where chartId=" + message.Chat.Id);

        //                //组织模板
        //                var sb = new StringBuilder();
        //                sb.AppendLine($"✅ Token:{stepEntity.TokenName} ({chainTypeFomate})");
        //                sb.AppendLine("");
        //                sb.AppendLine("➡️ Confirm And Select The chat to continue. ");
        //                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        //                {
        //                                new []
        //                                {
        //                                    InlineKeyboardButton.WithCallbackData(text: $"{groupInfo.Title}", callbackData: "ChoseChat"),
        //                                }

        //                        });
        //                res.Text = sb.ToString();
        //                res.KeyboardMarkup = inlineKeyboard;
        //                return res;
        //            }
        //            catch (Exception ex)
        //            {
        //                if (ex.Message.Contains("chat not found"))
        //                {
        //                    res.Text = $"❌ Telegram Group Not Found ({message.Text}). Try again";
        //                    return res;
        //                }
        //                else
        //                {
        //                    throw;
        //                }

        //            }
        //        }


        //        private async Task<TextCommandViewModel> PrivateProcessConfirmTokenText(Message message, CqBuyTrendingBotStep stepEntity)
        //        {
        //            //根据地址，获得合约名称,供应量
        //            var abi = "";
        //            var tokenName = "";
        //            var chainTypeFomate = "";
        //            if (stepEntity.ChainType == 1)
        //                chainTypeFomate = "ETH";
        //            if (stepEntity.ChainType == 2)
        //                chainTypeFomate = "BSC";
        //            var res = new TextCommandViewModel();
        //            if (stepEntity.ChainType == 1)
        //            {
        //                //检测合约是否合法
        //                if (!Web3Util.IsTrueEthAddress(message.Text))
        //                {
        //                    res.Text = $"❌ Token address not valid ({message.Text}). Try again";
        //                    return res;
        //                }


        //                abi = web3ConfigOption.EthAbi;
        //                try
        //                {
        //                    var web3 = new Web3(web3ConfigOption.InfuraUrl);
        //                    var contract = web3.Eth.GetContract(abi, message.Text);

        //                    var contractNameFunction = contract.GetFunction("name");
        //                    tokenName = await contractNameFunction.CallAsync<string>();

        //                    var totalSupplyFunction = contract.GetFunction("totalSupply");
        //                    var totalSupply = await totalSupplyFunction.CallAsync<BigInteger>();

        //                    var decimalsFunction = contract.GetFunction("decimals");
        //                    var decimals = await decimalsFunction.CallAsync<int>();
        //                    //获取真实的供应量，abi中的totalSupply除以abi中的10的decimals次方
        //                    var realSupply = decimal.Parse((totalSupply / BigInteger.Pow(10, decimals)).ToString()); // 除以10的decimals次方

        //                    //获取该TOKEN的交易对合约
        //                    // 创建工厂合约实例
        //                    var factoryContract = web3.Eth.GetContract(web3ConfigOption.UniswapV2FactoryAbi, web3ConfigOption.UniswapV2FactoryAddress);
        //                    // 调用getPair方法获取交易对地址
        //                    var pairAddress = await factoryContract.GetFunction("getPair").CallAsync<string>("0xc02aaa39b223fe8d0a0e5c4f27ead9083c756cc2", message.Text); // weth/代币

        //                    stepEntity.TokenName = tokenName;
        //                    stepEntity.TokenAddress = message.Text;
        //                    stepEntity.TokenDecimals = decimals;
        //                    stepEntity.TotalSupply = realSupply;
        //                    stepEntity.PairAddress = pairAddress;
        //                    stepEntity.LastStep = (int)BuyTrendingBotStep.输入Token;

        //                }
        //                catch (Exception ex)
        //                {
        //                    logger.Fatal(new EventData()
        //                    {
        //                        Type = $"根据ETH合约地址:{message.Text}获取合约数据异常",
        //                        Message = $"{ex.Message}-{ex.StackTrace}"
        //                    });
        //                    res.Text = $"❌ Failed to obtain information based on the contract address({message.Text}). Try again";
        //                    return res;

        //                }

        //            }

        //            if (stepEntity.ChainType == 2)
        //            {
        //                if (!Web3Util.IsTrueBscAddress(message.Text))
        //                {
        //                    res.Text = $"❌ Token address not valid ({message.Text}). Try again";
        //                    return res;
        //                }


        //                abi = web3ConfigOption.BscAbi;

        //                try
        //                {
        //                    var web3 = new Web3(web3ConfigOption.QuickNodeUrl);
        //                    var contract = web3.Eth.GetContract(abi, message.Text);

        //                    var contractNameFunction = contract.GetFunction("name");
        //                    tokenName = await contractNameFunction.CallAsync<string>();

        //                    var totalSupplyFunction = contract.GetFunction("totalSupply");
        //                    var totalSupply = await totalSupplyFunction.CallAsync<BigInteger>();

        //                    var decimalsFunction = contract.GetFunction("decimals");
        //                    var decimals = await decimalsFunction.CallAsync<int>();
        //                    //获取真实的供应量，abi中的totalSupply除以abi中的10的decimals次方
        //                    var realSupply = decimal.Parse((totalSupply / BigInteger.Pow(10, decimals)).ToString()); // 除以10的decimals次方

        //                    //获取该TOKEN的交易对合约
        //                    // 创建工厂合约实例
        //                    var factoryContract = web3.Eth.GetContract(web3ConfigOption.BscCakeV2FactoryAbi, web3ConfigOption.BscCakeV2FactoryAddress);
        //                    // 调用getPair方法获取交易对地址
        //                    var pairAddress = await factoryContract.GetFunction("getPair").CallAsync<string>("0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c", message.Text); // wbnb/代币

        //                    //目前支支持cakev2并且合约对为wbnb否则不好使

        //                    stepEntity.TokenName = tokenName;
        //                    stepEntity.TokenAddress = message.Text;
        //                    stepEntity.TokenDecimals = decimals;
        //                    stepEntity.TotalSupply = realSupply;
        //                    stepEntity.PairAddress = pairAddress;
        //                    stepEntity.LastStep = (int)BuyTrendingBotStep.输入Token;

        //                }
        //                catch (Exception ex)
        //                {
        //                    logger.Fatal(new EventData()
        //                    {
        //                        Type = $"根据BSC合约地址:{message.Text}获取合约数据异常",
        //                        Message = $"{ex.Message}-{ex.StackTrace}"
        //                    });
        //                    res.Text = $"❌ Failed to obtain information based on the contract address({message.Text}). Try again";
        //                    return res;

        //                }

        //            }

        //            stepEntity.UpdateTime = DateTime.Now;
        //            await trendingContext.SaveChangesAsync();
        //            //组织开始回复token添加成功消息模板
        //            var sb = new StringBuilder();
        //            sb.AppendLine($"✅ Token:{tokenName} ({chainTypeFomate})");
        //            sb.AppendLine("");
        //            sb.AppendLine("➡️ Enter Your“Telegram Group Link”To Guys Join;");
        //            sb.AppendLine("The Format Is “https://t.me/xxxxxxxxxxx”");

        //            //带上返回重新输入token按钮
        //            //InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        //            //{
        //            //        new []
        //            //        {
        //            //            InlineKeyboardButton.WithCallbackData(text: $"Back to Enter [{chainTypeFomate}] Token Address", callbackData: "BackToPre-EnterToken"),
        //            //        },
        //            //        new []
        //            //        {
        //            //            InlineKeyboardButton.WithCallbackData(text: "Back to Restart", callbackData: "BackToPre-Start")
        //            //        },

        //            //});
        //            res.Text = sb.ToString();
        //            //res.KeyboardMarkup = inlineKeyboard;
        //            return res;
        //        }

        //        private async Task<TextCommandViewModel> PrivateProcessSetEmojiText(Message message, string address)
        //        {
        //            var res = new TextCommandViewModel();
        //            var trendingEntity = await trendingContext.CqTrendingTokens.FirstOrDefaultAsync(p => p.TokenAdress == address && p.IsTrending == 1);
        //            if (trendingContext != null)
        //            {
        //                trendingEntity.Emojy = message.Text;
        //                await trendingContext.SaveChangesAsync();
        //                var sb = new StringBuilder();
        //                sb.AppendLine("✅ Successful setting");
        //                res.Text = sb.ToString();
        //                res.IsReplay = true;
        //                return res;
        //            }
        //            return res;
        //        }

        //        private async Task<TextCommandViewModel> PrivateProcessConfirmPaymentTxText(Message message, CqBuyTrendingBotStep stepEntity)
        //        {
        //            var res = new TextCommandViewModel();
        //            CqAmountMenus menu = null;
        //            //验证格式
        //            if (!Web3Util.IsValidBlockchainTransactionHash(message.Text))
        //            {
        //                res.Text = "❌ Transaction Hash is not valid.";
        //                return res;
        //            }
        //            //看是否redis存在,防止重复
        //            if (await redisService.IsExit($"CQ:HASH_{message.Text}"))
        //            {
        //                res.Text = "❌ The Transaction is being confirmed.";
        //                return res;
        //            }

        //            //数据库验证是否有成功的hash
        //            if (await trendingContext.CqSuccessOrder.AnyAsync(p => p.TransactionHash == message.Text))
        //            {
        //                res.Text = "❌ Duplicate transaction";
        //                return res;
        //            }

        //            //开始验证hash交易状态
        //            await redisService.Set<string>($"CQ:HASH_{message.Text}", "abc", DateTime.Now.AddMinutes(1));
        //            var status = false;
        //            try
        //            {
        //                if (stepEntity.ChainType == 1)
        //                {
        //                    //eth链验证
        //                    var web3 = new Web3(web3ConfigOption.InfuraUrl);
        //                    var transaction = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(message.Text);

        //                    // 如果交易不为null，表示交易存在
        //                    if (transaction != null)
        //                    {
        //                        //判断收款方是不是我们的钱包地址
        //                        if (transaction.To.ToLower() != "0x86620EB5C1feb9b8c80411911f99863Fa5bF34D6".ToLower())
        //                        {
        //                            res.Text = $"❌ the payment wallet is not valid. ";
        //                            return res;
        //                        }
        //                        //判断是否是所需支付的金额
        //                        //获取交易的转账金额（Wei单位）
        //                        var value = transaction.Value;
        //                        decimal ethAmount = Web3.Convert.FromWei(value);
        //                        //获取价格类目模型
        //                        menu = await trendingContext.CqAmountMenus.FirstOrDefaultAsync(p => p.ChainType == stepEntity.ChainType && p.Hour == stepEntity.TrendingHours);

        //                        //涉及到用户的gas,我们默认给他加10%哎
        //                        ethAmount = ethAmount * 1.1m;

        //                        if (ethAmount < menu.Amount)
        //                        {
        //                            res.Text = $"❌ amount is not valid. soure {menu.Amount}{menu.Unit}?";
        //                            return res;
        //                        }



        //                        // 获取交易状态
        //                        var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(message.Text);

        //                        if (receipt != null && receipt.Status.Value == 1)
        //                        {
        //                            status = true;
        //                            //交易成功
        //                        }
        //                        else
        //                        {
        //                            res.Text = $"❌ Transaction is not completed. Try again";
        //                            return res;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        res.Text = $"❌ Transaction Hash is not exist.";
        //                        return res;
        //                    }
        //                }

        //                if (stepEntity.ChainType == 2)
        //                {
        //                    //bsc链验证
        //                    var web3 = new Web3(web3ConfigOption.QuickNodeUrl);
        //                    var transaction = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(message.Text);

        //                    // 如果交易不为null，表示交易存在
        //                    if (transaction != null)
        //                    {
        //                        //判断收款方是不是我们的钱包地址
        //                        if (transaction.To.ToLower() != "0x0cD29E8161194BB034B958635FFFd4cf39B8901d".ToLower())
        //                        {
        //                            res.Text = $"❌ the payment wallet is not valid. ";
        //                            return res;
        //                        }
        //                        //判断是否是所需支付的金额
        //                        //获取交易的转账金额（Wei单位）
        //                        var value = transaction.Value;
        //                        decimal ethAmount = Web3.Convert.FromWei(value);
        //                        //获取价格类目模型
        //                        menu = await trendingContext.CqAmountMenus.FirstOrDefaultAsync(p => p.ChainType == stepEntity.ChainType && p.Hour == stepEntity.TrendingHours);
        //                        ethAmount = ethAmount * 1.1m;
        //                        if (ethAmount < menu.Amount)
        //                        {
        //                            res.Text = $"❌ amount is not valid. soure {menu.Amount}{menu.Unit}?";
        //                            return res;
        //                        }



        //                        // 获取交易状态
        //                        var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(message.Text);

        //                        if (receipt != null && receipt.Status.Value == 1)
        //                        {
        //                            status = true;
        //                            //交易成功
        //                        }
        //                        else
        //                        {
        //                            res.Text = $"❌ Transaction is not completed. Try again";
        //                            return res;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        res.Text = $"❌ Transaction Hash is not exist.";
        //                        return res;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.Fatal(new EventData()
        //                {
        //                    Type = $"验证交易HASH地址:{message.Text}异常",
        //                    Message = $"{ex.Message}-{ex.StackTrace}"
        //                });
        //                res.Text = $"❌ Failed to obtain information based on the transaction hash({message.Text}). Try again";
        //                return res;

        //            }
        //            finally
        //            {
        //                await redisService.Remove($"CQ:HASH_{message.Text}");
        //            }

        //            if (status)
        //            {
        //                stepEntity.LastStep = (int)BuyTrendingBotStep.提交付款hash;
        //                stepEntity.UpdateTime = DateTime.Now;
        //                var _id = 0L;
        //                //入库成功交易
        //                var order = new CqSuccessOrder()
        //                {
        //                    TransactionHash = message.Text,
        //                    ChainType = stepEntity.ChainType.Value,
        //                    Hours = stepEntity.TrendingHours.Value,
        //                    Amount = menu.Amount,
        //                    ChatId = message.Chat.Id,
        //                    ChatUserName = stepEntity.TgFromName,
        //                    CreateTime = DateTime.Now,
        //                    ToTgUrl = stepEntity.ToTgUrl,
        //                    Unit = menu.Unit
        //                };
        //                await trendingContext.CqSuccessOrder.AddAsync(order);

        //                //入库趋势定时表
        //                var trendingTokenEntity = await trendingContext.CqTrendingTokens.FirstOrDefaultAsync(p => p.TokenAdress == stepEntity.TokenAddress && p.IsTrending == 1 && p.IsTrue == 1 && p.ChainType == stepEntity.ChainType);
        //                if (trendingTokenEntity != null)
        //                {
        //                    _id = trendingTokenEntity.Id;
        //                    //增加趋势时间
        //                    trendingTokenEntity.TgUrl = stepEntity.ToTgUrl;
        //                    trendingTokenEntity.TgName = stepEntity.ToTgName;
        //                    trendingTokenEntity.ExpireTime = trendingTokenEntity.ExpireTime.AddHours(stepEntity.TrendingHours.Value);
        //                }
        //                else
        //                {
        //                    //新增该链趋势
        //                    //因为有假的，如果在趋势中的，满员18，自动去掉一个假的趋势
        //                    if (await trendingContext.CqTrendingTokens.CountAsync(p => p.ChainType == stepEntity.ChainType && p.IsTrending == 1) >= 18)
        //                    {
        //                        var fakeTrendingTokenEntity = await trendingContext.CqTrendingTokens.Where(p => p.ChainType == stepEntity.ChainType && p.IsTrending == 1 && p.IsTrue == 0).OrderBy(p => p.CreateTime).FirstOrDefaultAsync();
        //                        if (fakeTrendingTokenEntity != null)
        //                            fakeTrendingTokenEntity.IsTrending = 0;
        //                    }

        //                    var newTrendingTokenEntity = new CqTrendingTokens()
        //                    {
        //                        Id = await idProvideService.GetID(),
        //                        ChainType = stepEntity.ChainType.Value,
        //                        CoinName = stepEntity.TokenName,
        //                        CreateTime = DateTime.Now,
        //                        ExpireTime = DateTime.Now.AddHours(stepEntity.TrendingHours.Value),
        //                        IsTrending = 1,
        //                        TokenDecimals = stepEntity.TokenDecimals,
        //                        IsTrue = 1,
        //                        TgName = stepEntity.ToTgName,
        //                        TgUrl = stepEntity.ToTgUrl,
        //                        PairAddress = stepEntity.PairAddress,
        //                        TokenAdress = stepEntity.TokenAddress,
        //                        TotalSupply = stepEntity.TotalSupply.Value
        //                    };
        //                    await trendingContext.CqTrendingTokens.AddAsync(newTrendingTokenEntity);
        //                    _id = newTrendingTokenEntity.Id;
        //                }

        //                var _trendingUserName = "";
        //                var _trendingUrl = "";
        //                if (stepEntity.ChainType == 1)
        //                {
        //                    _trendingUserName = "FastEthTrending(Live)";
        //                    _trendingUrl = "https://t.me/fastEthTrending";
        //                }

        //                if (stepEntity.ChainType == 2)
        //                {
        //                    _trendingUserName = "FastBscTrending(Live)";
        //                    _trendingUrl = "https://t.me/fastBscTrending";
        //                }

        //                //组织回复消息模板
        //                var sb = new StringBuilder();
        //                sb.AppendLine($"[Forwarded from Buy Fast Trending Bot](https://t.me/BuyFastTrendingBot)");
        //                sb.AppendLine($"✅ Payment confirmed. Ticket:");
        //                sb.AppendLine($"{_id}");
        //                sb.AppendLine("");

        //                sb.AppendLine($"Token: [{stepEntity.TokenName}]({stepEntity.ToTgUrl})");
        //                sb.AppendLine($"Group: [{stepEntity.ToTgName}]({stepEntity.ToTgUrl})");
        //                sb.AppendLine($"Ca: {stepEntity.TokenAddress}");
        //                sb.AppendLine("");
        //                sb.AppendLine($"[{_trendingUserName}]({_trendingUrl}) boost will begin shortly.");
        //                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        //                        {
        //                         new []
        //                            {
        //                                InlineKeyboardButton.WithCallbackData(text: $"Click Setting Emoji 🟢", callbackData: "ToSettingEmoji"),
        //                            }

        //                        });

        //                res.Text = sb.ToString();
        //                res.IsReplay = false;
        //                res.KeyboardMarkup = inlineKeyboard;
        //                await trendingContext.SaveChangesAsync();
        //                return res;
        //            }

        //            return res;
        //        }
    }
}
