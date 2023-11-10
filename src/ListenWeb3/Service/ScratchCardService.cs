using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using CommonLibrary.DbContext;
using Newtonsoft.Json.Linq;
using Nethereum.ABI.Model;
using ListenWeb3.Model;
namespace ListenWeb3.Service
{
    public class ScratchCardService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlMasterDbContext _masterDbContext;
        public ScratchCardService(IConfiguration configuration, MySqlMasterDbContext mySqlMasterDbContext)
        {
            _configuration = configuration;
            _masterDbContext = mySqlMasterDbContext;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {

            try
            {


                // Infura 提供的以太坊节点 WebSocket 地址
                string nodeUrl = _configuration["OP:WSS_URL"];

                // 你的以太坊智能合约地址
                string contractAddress = _configuration["OP:Contract_ScratchCard"];

                // 读取JSON文件内容
                string jsonFilePath = "ScratchCard.json"; // 替换为正确的JSON文件路径

                string jsonString = System.IO.File.ReadAllText(jsonFilePath);

                // 解析JSON
                JObject jsonObject = JObject.Parse(jsonString);

                // 获取abi节点的值
                string abi = jsonObject["abi"]?.ToString();

                // 创建 Web3 实例
                var web3 = new Web3WebSocket(nodeUrl);

                // 创建智能合约实例
                var contract = web3.Eth.GetContract(abi, contractAddress);

                // 事件名
                string eventName = "CardTypeAdded";

                // 订阅事件
                var subscription = contract.GetEvent(eventName).CreateFilterInput().Subscribe(log =>
                {
                    // 处理实时事件
                    Console.WriteLine($"Real-time Event {eventName} received: {log.TransactionHash}");
                });

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                // 取消订阅
                await subscription;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
