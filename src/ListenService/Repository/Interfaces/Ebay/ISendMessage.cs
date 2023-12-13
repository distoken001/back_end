using System;
using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
	public interface ISendMessage
    {
        Task SendMessageEbay(long order_id, ChainEnum chain_id, string contract);
    }
}

