#!/bin/bash

# 设置项目名称和发布目录
project_name="listen_service"
publish_directory="./publish_listen_service"

# 检查并删除旧的发布目录（如果存在）
echo "Checking for existing publish directory..."
if [ -d "$publish_directory" ]; then
    echo "Publish directory exists. Removing..."
    rm -rf $publish_directory
fi

# 构建项目
echo "Building the project..."
dotnet build --configuration Release ./src/ListenService/ListenService.csproj

# 发布项目
echo "Publishing the project..."
dotnet publish --configuration Release ./src/ListenService/ListenService.csproj --output $publish_directory

# 复制 PM2 配置文件到发布目录
echo "Copying PM2 config file to the publish directory..."
cp  ./src/ListenService/pm2.listen.config.js $publish_directory

# 进入发布目录
cd $publish_directory

# 使用 PM2 运行项目
echo "Starting the application with PM2..."
pm2 start pm2.listen.config.js --env development