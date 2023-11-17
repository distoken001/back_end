using System;
namespace ListenWeb3.Repository.Interfaces
{
	public interface ICardTypeAdded
    {
        Task StartAsync(string nodeUrl, string contractAddress);
    }
}

