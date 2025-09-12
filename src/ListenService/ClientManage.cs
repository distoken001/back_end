using ListenService.Chains;
using System.Net.WebSockets;

namespace ListenService
{
    public class ClientManage
    {
        private readonly object _lock = new();
        private WebSocketClientBsc _client;
        private readonly string _nodeUrl;
        private volatile bool _isReplacing = false;

        public ClientManage(string nodeUrl)
        {
            _nodeUrl = nodeUrl;
            _client = new WebSocketClientBsc(nodeUrl);
            WebSocketClientBsc.ForceCompleteReadTotalMilliseconds = Timeout.Infinite;
            WebSocketClientBsc.ConnectionTimeout = Timeout.InfiniteTimeSpan;
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
                if (_isReplacing)
                {
                    return; // 避免重复替换
                }
                
                _isReplacing = true;
                try
                {
                    if (_client != null)
                    {
                        try
                        {
                            _client.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Dispose旧客户端时发生错误: {ex.Message}");
                        }
                    }
                    _client = newClient;
                }
                finally
                {
                    _isReplacing = false;
                }
            }
        }

        public bool IsConnectionHealthy()
        {
            lock (_lock)
            {
                if (_client == null || _isReplacing)
                    return false;
                    
                var state = _client.WebSocketState;
                return state == WebSocketState.Open || state == WebSocketState.Connecting;
            }
        }

        public WebSocketState GetConnectionState()
        {
            lock (_lock)
            {
                return _client?.WebSocketState ?? WebSocketState.Closed;
            }
        }
    }
}