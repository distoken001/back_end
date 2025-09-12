using System.Net.WebSockets;

namespace ListenService
{
    public class ReconnectionManager
    {
        private readonly ClientManage _clientManage;
        private readonly string _serviceName;
        private int _reconnectAttempts = 0;
        private readonly int _maxReconnectAttempts = 5;
        private readonly int _baseDelay = 2000;
        private readonly object _lock = new();

        public ReconnectionManager(ClientManage clientManage, string serviceName)
        {
            _clientManage = clientManage;
            _serviceName = serviceName;
        }

        public async Task<bool> WaitForConnectionAsync(CancellationToken cancellationToken = default)
        {
            var maxWaitTime = TimeSpan.FromMinutes(5); // 最多等待5分钟
            var startTime = DateTime.Now;
            
            while (!cancellationToken.IsCancellationRequested && 
                   (DateTime.Now - startTime) < maxWaitTime)
            {
                if (_clientManage.IsConnectionHealthy())
                {
                    return true;
                }
                
                await Task.Delay(500, cancellationToken);
            }
            
            return false;
        }

        public async Task<bool> HandleReconnectionAsync(Func<Task> reconnectAction, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                _reconnectAttempts++;
            }
            
            if (_reconnectAttempts > _maxReconnectAttempts)
            {
                // 重置计数，使用指数退避
                _reconnectAttempts = 0;
                var delay = Math.Min(_baseDelay * Math.Pow(2, 3), 30000); // 最大延迟30秒
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {_serviceName} 重连次数过多，延迟 {delay}ms 后继续重连");
                await Task.Delay((int)delay, cancellationToken);
            }
            else
            {
                var delay = Math.Min(_baseDelay * Math.Pow(2, _reconnectAttempts - 1), 10000); // 最大延迟10秒
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {_serviceName} 连接错误，{delay}ms 后重连 (第{_reconnectAttempts}次)");
                await Task.Delay((int)delay, cancellationToken);
            }
            
            try
            {
                await reconnectAction();
                lock (_lock)
                {
                    _reconnectAttempts = 0; // 重连成功，重置计数
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {_serviceName} 重连失败: {ex.Message}");
                return false;
            }
        }

        public void ResetAttempts()
        {
            lock (_lock)
            {
                _reconnectAttempts = 0;
            }
        }
    }
}
