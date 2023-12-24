using System;
using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
	public interface IBoxGifted
    {
        Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id);
    }
}

