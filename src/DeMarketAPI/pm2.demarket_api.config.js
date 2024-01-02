module.exports = {
    apps: [
        {
            name: "demarket_api",
            cmd: "dotnet",
            args: ["DeMarketAPI.dll", "--urls", "http://localhost:5000"],
            watch: false,
            autorestart: true,
            exec_mode: 'cluster',
            instances:4,
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