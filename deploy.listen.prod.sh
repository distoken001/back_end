#!/bin/bash

# 设置项目名称和发布目录
project_name="listen_service"
publish_directory="./publish_listen_service"

# 构建项目
echo "Building the project..."
dotnet build --configuration Release ./src/ListenWeb3/ListenWeb3.csproj

# 发布项目
echo "Publishing the project..."
dotnet publish --configuration Release ./src/ListenWeb3/ListenWeb3.csproj --output $publish_directory

# 复制 PM2 配置文件到发布目录
echo "Copying PM2 config file to the publish directory..."
cp  ./src/ListenWeb3/pm2.listen.config.js $publish_directory

# 进入发布目录
cd $publish_directory

# 使用 PM2 运行项目
echo "Starting the application with PM2..."
pm2 start pm2.listen.config.js --env production