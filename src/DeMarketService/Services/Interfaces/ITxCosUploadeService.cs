using System.Threading.Tasks;

namespace deMarketService.Services.Interfaces
{
    public interface ITxCosUploadeService
    {
        Task<string> Upload(byte[] bytes, string cosName);
        string GetCredential();
    }
}
