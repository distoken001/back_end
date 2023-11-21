module.exports = {
    apps: [
        {
            name: "listen",
            cmd: "dotnet",
            args: ["ListenService.dll", "--urls", "http://localhost:5007"],
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