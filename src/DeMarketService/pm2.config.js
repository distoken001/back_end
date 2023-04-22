module.exports = {
    apps: [
        {
            name: "back_end",
            cmd: "dotnet",
            args: "deMarketService.dll",
            watch: false,
            autorestart: true,
            env_development: {
                EnvironmentName: "Development",
            },
            // env_testing: {
            // ASPNETCORE_ENVIRONMENT: "Testing",
            //   },
            env_production: {
                EnvironmentName: "Production",
            },
        },
    ],
};