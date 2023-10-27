module.exports = {
    apps: [
        {
            name: "telegram_service",
            cmd: "dotnet",
            args: ["TelegramService.dll", "--urls", "http://localhost:5006"],
            watch: false,
            autorestart: true,
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