module.exports = {
    apps: [
        {
            name: "deMarketService",
            cmd: "dotnet",
            args: "deMarketService.dll",
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