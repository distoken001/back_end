using System;
namespace ListenWeb3.Repository.ScratchCard.Interfaces
{
	public interface ICardPurchased
    {
        Task StartAsync(string nodeUrl, string contractAddress);
    }
}

