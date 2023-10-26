module.exports = {
    apps: [
        {
            name: "jobs",
            cmd: "dotnet",
            args: ["Jobs.dll", "--urls", "http://localhost:5005"],
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