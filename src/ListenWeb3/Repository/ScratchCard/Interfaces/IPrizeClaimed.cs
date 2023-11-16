using System;
namespace ListenWeb3.Repository.ScratchCard.Interfaces
{
	public interface IPrizeClaimed
    {
        Task StartAsync(string nodeUrl, string contractAddress);
    }
}

