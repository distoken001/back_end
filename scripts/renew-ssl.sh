#!/bin/bash

# SSL 证书续期脚本
# 建议添加到 crontab 中定期执行：
# 0 3 */7 * * /path/to/your/project/scripts/renew-ssl.sh

echo "### Checking for SSL certificate renewal..."

# 尝试续期证书
docker-compose run --rm certbot renew

# 重新加载 nginx 配置
if [ $? -eq 0 ]; then
    echo "### Certificate renewed successfully, reloading nginx..."
    docker-compose exec nginx nginx -s reload
    echo "### SSL certificate renewal completed!"
else
    echo "### No certificate renewal needed or renewal failed."
fi
