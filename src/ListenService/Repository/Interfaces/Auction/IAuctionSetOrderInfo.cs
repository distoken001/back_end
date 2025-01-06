using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
    public interface IAuctionSetOrderInfo
    {
        Task StartAsync(string nodeWss, string nodeHttps, string contractAddress, ChainEnum chain_id);
    }
}