module.exports = {
    apps: [
        {
            name: "listen_service",
            cmd: "dotnet",
            args: ["ListenService.dll", "--urls", "http://localhost:5007"],
            watch: false,
            autorestart: true,
            //cron_restart: '0 0 * * *', // 每天午夜重启一次
            max_memory_restart: '1024M',
            env_development: {
                ASPNETCORE_ENVIRONMENT: "Development",
            },
            // env_testing: {
            // ASPNETCORE_ENVIRONMENT: "Testing",
            //   },
            env_production: {
                ASPNETCORE_ENVIRONMENT: "Production",
            },
        },
    ],
};