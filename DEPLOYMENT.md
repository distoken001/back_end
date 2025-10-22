# DeMarket Docker 部署指南

本文档描述了如何使用 Docker Compose 部署完整的 DeMarket 应用，包括 API、前端、Redis、后台服务以及 HTTPS 配置。

## 架构概览

- **demarket.io** - 前端应用 (Next.js)
- **api.demarket.io** - API 服务 (.NET 6)
- **Nginx** - 反向代理和 SSL 终端
- **Redis** - 缓存和消息队列
- **Listen Service** - 区块链监听服务
- **Jobs** - 定时任务服务
- **Telegram** - Telegram Bot 服务

## 前置要求

1. Docker 和 Docker Compose
2. 域名解析配置：
   - `demarket.io` 和 `www.demarket.io` 指向服务器IP
   - `api.demarket.io` 指向服务器IP

## 部署步骤

### 1. 初始化 SSL 证书

首先为域名生成 Let's Encrypt SSL 证书：

```bash
cd back_end
chmod +x nginx/init-letsencrypt.sh
./nginx/init-letsencrypt.sh
```

**注意：** 在运行脚本前，请确保：
- 域名已正确解析到服务器
- 编辑 `nginx/init-letsencrypt.sh` 中的 `email` 变量为你的邮箱

### 2. 启动所有服务

```bash
docker-compose up -d
```

### 3. 检查服务状态

```bash
docker-compose ps
```

所有服务应该显示为 "Up" 状态。

### 4. 查看日志

```bash
# 查看所有服务日志
docker-compose logs

# 查看特定服务日志
docker-compose logs nginx
docker-compose logs demarket_api
docker-compose logs demarket_frontend
```

## 服务配置

### API 服务 (DeMarketAPI)
- **容器名**: `demarket_api`
- **内部端口**: 80
- **外部访问**: `https://api.demarket.io`
- **环境**: Production

### 前端服务 (Frontend)
- **容器名**: `demarket_frontend`
- **内部端口**: 3000
- **外部访问**: `https://demarket.io`
- **环境**: Production

### Nginx 代理
- **容器名**: `nginx_proxy`
- **端口**: 80 (HTTP) 和 443 (HTTPS)
- **配置文件**: `nginx/conf.d/demarket.conf`
- **功能**:
  - HTTP 到 HTTPS 重定向
  - SSL 终端
  - 反向代理
  - 静态资源缓存
  - 安全头设置
  - 速率限制

### SSL 证书管理
- **容器名**: `certbot`
- **功能**: 自动续期 Let's Encrypt 证书
- **续期频率**: 每12小时检查一次

## 安全配置

### SSL/TLS
- 支持 TLS 1.2 和 1.3
- 强加密套件
- HSTS 头部
- 完美前向保密

### 安全头部
- X-Frame-Options: DENY
- X-Content-Type-Options: nosniff
- X-XSS-Protection: 1; mode=block
- Strict-Transport-Security

### 速率限制
- API: 10 请求/秒，突发 20
- 前端: 20 请求/秒，突发 50

## 维护操作

### 更新应用

```bash
# 重新构建并启动服务
docker-compose up -d --build

# 更新特定服务
docker-compose up -d --build demarket_api
docker-compose up -d --build demarket_frontend
```

### SSL 证书手动续期

```bash
docker-compose run --rm certbot renew
docker-compose exec nginx nginx -s reload
```

### 备份数据

```bash
# 备份 Redis 数据
docker-compose exec redis redis-cli BGSAVE
docker cp redis:/data/dump.rdb ./backup/

# 备份 SSL 证书
cp -r nginx/certbot/conf ./backup/ssl-backup/
```

### 查看实时日志

```bash
# 所有服务
docker-compose logs -f

# 特定服务
docker-compose logs -f nginx
docker-compose logs -f demarket_api
```

## 故障排除

### 1. SSL 证书获取失败
- 确认域名解析正确
- 检查防火墙设置 (80, 443 端口)
- 查看 certbot 日志：`docker-compose logs certbot`

### 2. 服务无法启动
- 检查 Docker 镜像构建：`docker-compose build`
- 查看具体错误：`docker-compose logs [service_name]`

### 3. 域名无法访问
- 检查 nginx 配置：`docker-compose exec nginx nginx -t`
- 重启 nginx：`docker-compose restart nginx`

### 4. API 连接问题
- 确认 API 服务运行正常：`docker-compose logs demarket_api`
- 检查数据库连接配置

## 监控

### 健康检查端点
- Frontend: `https://demarket.io/health`
- API: `https://api.demarket.io/health`

### 日志监控
日志文件位置：
- Nginx 访问日志: `nginx/logs/access.log`
- Nginx 错误日志: `nginx/logs/error.log`

## 文件结构

```
back_end/
├── docker-compose.yml          # 主要的 Docker Compose 配置
├── Dockerfile.api              # API 服务 Dockerfile
├── Dockerfile.jobs             # Jobs 服务 Dockerfile
├── Dockerfile.listenservice    # Listen 服务 Dockerfile
├── Dockerfile.telegram         # Telegram 服务 Dockerfile
├── nginx/
│   ├── conf.d/
│   │   └── demarket.conf       # Nginx 配置
│   ├── certbot/                # SSL 证书存储
│   ├── logs/                   # Nginx 日志
│   └── init-letsencrypt.sh     # SSL 初始化脚本
└── src/                        # 源代码

front_end/
├── Dockerfile                  # 前端 Dockerfile
├── next.config.js              # Next.js 配置 (已添加 standalone 输出)
└── ...                         # 其他前端文件
```

## 环境变量

可以通过创建 `.env` 文件来自定义环境变量：

```bash
# .env 文件示例
REDIS_HOST=redis
REDIS_PORT=6379
NODE_ENV=production
ASPNETCORE_ENVIRONMENT=Production
```

---

如需更多帮助，请查看各服务的具体日志或联系技术支持团队。
