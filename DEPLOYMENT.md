# DeMarket 部署指南

本文档描述如何使用 Docker Compose 部署 DeMarket 后端服务，包括 HTTPS 配置。

## 服务架构

- **DeMarket API**: Web API 服务，对外提供 REST API
- **Listen Service**: 区块链监听服务（2个实例）
- **Jobs Service**: 定时任务服务
- **Telegram Service**: Telegram 机器人服务
- **Redis**: 缓存数据库
- **Nginx**: 反向代理和 SSL 终端
- **Certbot**: SSL 证书管理

## 部署步骤

### 1. 首次部署

1. **配置域名**
   - 确保域名 `api.demarket.io` 已正确解析到服务器 IP
   - 在 `scripts/init-letsencrypt.sh` 中修改邮箱地址

2. **启动服务**
   ```bash
   # 给脚本添加执行权限
   chmod +x scripts/*.sh
   
   # 构建并启动服务
   ./scripts/deploy.sh
   ```

3. **初始化 SSL 证书**
   ```bash
   # 获取 Let's Encrypt SSL 证书
   ./scripts/init-letsencrypt.sh
   ```

### 2. 日常操作

- **查看服务状态**
  ```bash
  docker-compose ps
  ```

- **查看日志**
  ```bash
  # 查看所有服务日志
  docker-compose logs
  
  # 查看特定服务日志
  docker-compose logs demarket_api
  docker-compose logs nginx
  ```

- **重启服务**
  ```bash
  # 重启所有服务
  docker-compose restart
  
  # 重启特定服务
  docker-compose restart demarket_api
  ```

- **更新服务**
  ```bash
  # 重新构建并重启
  docker-compose up -d --build
  ```

### 3. SSL 证书管理

- **手动续期证书**
  ```bash
  ./scripts/renew-ssl.sh
  ```

- **自动续期（推荐）**
  将续期脚本添加到 crontab：
  ```bash
  crontab -e
  # 添加以下行（每周检查一次）
  0 3 */7 * * /path/to/your/project/scripts/renew-ssl.sh
  ```

## 配置文件说明

### docker-compose.yml
主要的容器编排配置文件，定义了所有服务及其依赖关系。

### nginx/nginx.conf.initial
初始的 Nginx 配置，用于首次获取 SSL 证书。

### nginx/nginx.conf
包含 SSL 配置的正式 Nginx 配置。

### Dockerfile.demarketapi
DeMarket API 服务的 Docker 镜像构建文件。

## 安全配置

1. **SSL/TLS 配置**
   - 使用 TLS 1.2 和 1.3
   - 强密码套件配置
   - HSTS 安全头
   - 其他安全头（X-Frame-Options, X-Content-Type-Options 等）

2. **访问控制**
   - API 限流配置（10 请求/秒）
   - 仅 DeMarket API 对外暴露
   - 其他服务仅内网访问

## 端口说明

- **80**: HTTP（重定向到 HTTPS）
- **443**: HTTPS（DeMarket API）
- **6379**: Redis（仅内网访问）

## 故障排除

1. **SSL 证书问题**
   ```bash
   # 检查证书状态
   docker-compose run --rm certbot certificates
   
   # 强制重新获取证书
   ./scripts/init-letsencrypt.sh
   ```

2. **Nginx 配置问题**
   ```bash
   # 测试 Nginx 配置
   docker-compose exec nginx nginx -t
   
   # 重新加载配置
   docker-compose exec nginx nginx -s reload
   ```

3. **服务连接问题**
   ```bash
   # 检查网络连接
   docker network ls
   docker network inspect demarket_app-network
   ```

## 监控和日志

- **Nginx 日志**: `./nginx_logs/`
- **应用日志**: `docker-compose logs [service_name]`
- **SSL 证书**: `./certbot/conf/`

## 备份建议

1. **数据备份**
   - Redis 数据: `redis_data` volume
   - SSL 证书: `./certbot/conf/`
   - 应用配置文件

2. **备份脚本**
   ```bash
   # 备份 Redis 数据
   docker-compose exec redis redis-cli BGSAVE
   
   # 复制备份文件
   docker cp redis:/data/dump.rdb ./backup/
   ```

## 环境变量

可以通过创建 `.env` 文件来自定义环境变量：

```env
# Redis 配置
REDIS_HOST=redis
REDIS_PORT=6379

# API 配置
ASPNETCORE_ENVIRONMENT=Production
```
