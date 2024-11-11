using Microsoft.AspNetCore.Mvc;
using Nethereum.JsonRpc.WebSocketStreamingClient;

namespace ListenService.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly StreamingWebSocketClient _client;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, StreamingWebSocketClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<string> Get()
    {
        Summaries.Select<string,string>(a =>  a);
        await _client.StopAsync();
        return _client.IsStarted.ToString()+_client.WebSocketState;

    }
}

