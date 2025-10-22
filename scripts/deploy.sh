#!/bin/bash

# 部署脚本
# 使用方法: ./scripts/deploy.sh

echo "### Building and starting DeMarket services..."

# 构建并启动所有服务
docker-compose up -d --build

echo "### Waiting for services to start..."
sleep 10

# 检查服务状态
echo "### Checking service status..."
docker-compose ps

echo ""
echo "### Services started successfully!"
echo ""
echo "Available services:"
echo "- Redis: localhost:6379"
echo "- DeMarket API: https://api.demarket.io (through nginx)"
echo "- Listen Services: Internal only"
echo "- Jobs Service: Internal only"
echo "- Telegram Service: Internal only"
echo ""
echo "Note: If this is the first deployment, run './scripts/init-letsencrypt.sh' to setup SSL certificates."
