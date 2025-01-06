using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
    public interface IPostAddOrder
    {
        Task StartAsync(string nodeWss, string nodeHttps, string contractAddress, ChainEnum chain_id);
    }
}