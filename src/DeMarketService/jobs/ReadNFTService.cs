using deMarketService.Common.Model;
using deMarketService.Common.Model.DataEntityModel;
using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC721.ContractDefinition;
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
    /// 读取NFT
    /// </summary>
    [DisallowConcurrentExecution]
    public class ReadNFTService : BaseJob
    {
        private readonly ILogger<ReadNFTService> _logger;
        private readonly MySqlMasterDbContext _masterDbContext;
        private IConfiguration _config;
        public ReadNFTService(ILogger<ReadNFTService> logger, MySqlMasterDbContext masterDbContext, IConfiguration configuration)
        {
            _logger = logger;
            _masterDbContext = masterDbContext;
            _config = configuration;
        }

        public override Task ExecuteMethod(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {
                try
                {
                    _logger.LogDebug($"ReadNFTService task start {DateTime.Now}");
                    // 连接到以太坊区块链网络
                    var web3 = new Web3(_config["BSCRPC"]);

                    // 合约地址和ABI定义
                    var contractAddress = _config["NFTContract"]; // 合约地址

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
                    var function = contract.GetFunction("ownerOf");
                    for (int i = 0; i < 100; i++)
                    {
                        var owner = await function.CallAsync<string>(i);

                        var user_nfts = _masterDbContext.user_nft.Where(a => a.nft == i && a.status == 1&&a.contract.Equals(contractAddress)).ToList();
                        if (user_nfts.Count > 1)
                        {
                            user_nfts.ForEach(a => a.status = 0);
                        }
                        else
                        {
                            var user_nft = user_nfts.FirstOrDefault();
                            if (user_nft == null)
                            {
                                var model = new user_nft()
                                {
                                    address = owner,
                                    create_time = DateTime.Now,
                                    nft = i,
                                    status = 1,
                                    update_time = DateTime.Now,
                                    contract= contractAddress,
                                    chain_id= ChainEnum.Bsc
                                };
                                _masterDbContext.Add(model);
                            }
                            if (user_nft != null)
                            {
                                user_nft.address = owner;
                                user_nft.update_time = DateTime.Now;
                            }
                        }
                    }
                    _masterDbContext.SaveChanges();
                    _logger.LogDebug($"ReadNFTService task end {DateTime.Now}");
                }
                catch (Exception ex)
                {
                    _masterDbContext.SaveChanges();
                    _logger.LogDebug($"ReadNFTService task 异常 {ex.InnerException.GetBaseException().Message}");
                }
            });
        }
    }
}
