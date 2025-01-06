using System.Threading.Tasks;

namespace DeMarketAPI.Services.Interfaces
{
    public interface ITxCosUploadeService
    {
        Task<string> Upload(byte[] bytes, string cosName);

        string GetCredential();
    }
}