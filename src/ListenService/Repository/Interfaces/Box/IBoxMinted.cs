using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
    public interface IBoxMinted
    {
        Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id);
    }
}