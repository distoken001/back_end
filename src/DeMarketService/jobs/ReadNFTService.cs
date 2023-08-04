using deMarketService.Common.Model.DataEntityModel;
using deMarketService.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts;
using Nethereum.RPC;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.jobs
{
    /// <summary>
    /// 每日凌晨产生前一天的邀请人返佣明细
    /// </summary>
    [DisallowConcurrentExecution]
    public class ReadNFTService : BaseJob
    {
        private readonly ILogger<ReadNFTService> _logger;
        private readonly MySqlMasterDbContext _masterDbContext;
        public ReadNFTService(ILogger<ReadNFTService> logger, MySqlMasterDbContext masterDbContext)
        {
            _logger = logger;
            _masterDbContext = masterDbContext;
        }

        public override Task ExecuteMethod(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {
                try
                {
                    _logger.LogDebug($"ReadNFTService task start {DateTime.Now}");

                    // 连接到以太坊区块链网络
                    var web3 = new Web3(@"https://dry-restless-dawn.bsc.discover.quiknode.pro/f73e235a8e136c7cbee870e54518e164a3823300/");

                    // 合约地址和ABI定义
                    var contractAddress = "0xF30141271104656AEE4D25F785c5E567Af455669"; // 合约地址

                    // 读取JSON文件内容
                    string jsonFilePath = "DeMarketAvatarNFT.json"; // 替换为正确的JSON文件路径

                    string jsonString = File.ReadAllText(jsonFilePath);

                    // 解析JSON
                    JObject jsonObject = JObject.Parse(jsonString);

                    // 获取abi节点的值
                    string abi = jsonObject["abi"]?.ToString(); // 使用?表示如果

                    var contractABI = abi; // 合约ABI定义

                    // 构造合约对象
                    var contract = new Contract(new EthApiService(web3.Client), contractABI, contractAddress);

                    // 定义查询函数的输入参数（如果有的话）
                    var functionABI = contract.GetFunction("ownerOf");
                    for (int i = 0; i < 100; i++)
                    {
                        var queryInput = new { i };
                        // 执行合约查询
                        //var queryHandler = web3.Eth.GetContractQueryHandler<string>();
                        //var queryResult = await queryHandler.QueryAsync<string>(contractAddress, functionABI, queryInput);
                        //Console.WriteLine($"Query Result: {queryResult}");
                    }

                    _logger.LogDebug($"ReadNFTService task end {DateTime.Now}");
                }
                catch (Exception ex)
                {

                    _logger.LogDebug($"ReadNFTService task 异常 {ex.InnerException.GetBaseException().Message}");
                }
            });
        }
    }
}
