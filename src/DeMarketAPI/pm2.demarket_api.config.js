module.exports = {
    apps: [
        {
            name: "demarket_api",
            cmd: "dotnet",
            args: ["DeMarketAPI.dll"],
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