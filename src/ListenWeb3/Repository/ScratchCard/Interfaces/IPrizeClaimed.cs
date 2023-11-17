using System;
namespace ListenWeb3.Repository.Interfaces
{
	public interface IPrizeClaimed
    {
        Task StartAsync(string nodeUrl, string contractAddress);
    }
}

