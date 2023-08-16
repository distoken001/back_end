module.exports = {
    apps: [
        {
            name: "deMarketServiceJobs",
            cmd: "dotnet",
            args: ["run", "deMarketService.csproj", "--urls", "http://localhost:5005"],
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
