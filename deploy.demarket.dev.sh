#!/bin/bash

# 设置项目名称和发布目录
project_name="demarket_api"
publish_directory="./publish_demarket_api"

# 构建项目
echo "Building the project..."
dotnet build --configuration Release ./src/DeMarketService/deMarketService.csproj

# 发布项目
echo "Publishing the project..."
dotnet publish --configuration Release ./src/DeMarketService/deMarketService.csproj --output $publish_directory

# 复制 PM2 配置文件到发布目录
echo "Copying PM2 config file to the publish directory..."
cp  ./src/DeMarketService/pm2.demarket.config.js $publish_directory

# 进入发布目录
cd $publish_directory

# 使用 PM2 运行项目
echo "Starting the application with PM2..."
pm2 start pm2.demarket.config.js --env development