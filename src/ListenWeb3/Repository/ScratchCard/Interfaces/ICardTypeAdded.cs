using System;
namespace ListenWeb3.Repository.ScratchCard.Interfaces
{
	public interface ICardTypeAdded
    {
        Task StartAsync(string nodeUrl, string contractAddress);
    }
}

