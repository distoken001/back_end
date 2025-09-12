using ListenService.Chains;

namespace ListenService
{
    public class ClientManage
    {
        private readonly object _lock = new();
        private WebSocketClientBsc _client;

        public ClientManage(string nodeUrl)
        {
            _client = new WebSocketClientBsc(nodeUrl);
            WebSocketClientBsc.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
            WebSocketClientBsc.ConnectionTimeout = Timeout.InfiniteTimeSpan;
            var client = new WebSocketClientBsc(nodeUrl);
        }

        public WebSocketClientBsc GetClient()
        {
            lock (_lock)
            {
                return _client;
            }
        }

        public void ReplaceClient(WebSocketClientBsc newClient)
        {
            lock (_lock)
            {
                _client.Dispose();
                _client = newClient;
            }
        }
    }
}