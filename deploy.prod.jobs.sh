#!/bin/bash

# 设置项目名称和发布目录
project_name="deMarketServiceJobs"

# 构建项目
echo "Building the project..."
dotnet build --configuration Release

# 使用 PM2 运行项目
echo "Starting the application with PM2..."

cd  ./src 
cd  ./DeMarketService
pm2 start pm2.jobs.config.js --env production