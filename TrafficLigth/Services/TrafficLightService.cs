using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TrafficLightService : BackgroundService
{
    private readonly ILogger<TrafficLightService> _logger;
    private readonly IHubContext<TrafficLightHub> _hubContext;
    private Timer _timer;
    private string _currentLightStateNS;
    private string _currentLightStateEW;
    private DateTime _lastChangeTimeNS;
    private DateTime _lastChangeTimeEW;

    public TrafficLightService(ILogger<TrafficLightService> logger, IHubContext<TrafficLightHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
        _currentLightStateNS = "NSGreen"; // Starting state
        _currentLightStateEW = "EWGreen";
        _lastChangeTimeNS = DateTime.Now;
        _lastChangeTimeEW = DateTime.Now;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        var now = DateTime.Now;
        var isPeakTime = (now.Hour >= 8 && now.Hour < 10) || (now.Hour >= 17 && now.Hour < 19);
        var greenDuration = isPeakTime ? 40 : 20;
        var eastWestGreenDuration = isPeakTime ? 10 : 20;

        switch (_currentLightStateNS)
        {
            case "NSGreen":
                if ((now - _lastChangeTimeNS).TotalSeconds >= greenDuration)
                {
                    _currentLightStateNS = "NSYellow";
                    _lastChangeTimeNS = now;
                }
                break;
            case "NSYellow":
                if ((now - _lastChangeTimeNS).TotalSeconds >= 5)
                {
                    _currentLightStateNS = "AllRed";
                    _lastChangeTimeNS = now;
                }
                break;
            case "AllRed":
                if ((now - _lastChangeTimeNS).TotalSeconds >= 4)
                {
                    if (isPeakTime)
                    {
                        _currentLightStateNS = "NSGreenRightTurn";
                    }
                    _currentLightStateNS = "NSGreen";
                    _lastChangeTimeNS = now;
                }
                break;
            case "NSGreenRightTurn":
                if ((now - _lastChangeTimeNS).TotalSeconds >= 10)
                {
                    _currentLightStateNS = "NSGreen";
                    _lastChangeTimeNS = now;
                }

                break;
        }

        switch (_currentLightStateEW)
        {
            case "EWGreen":
                if ((now - _lastChangeTimeEW).TotalSeconds >= eastWestGreenDuration)
                {
                    _currentLightStateEW = "EWYellow";
                    _lastChangeTimeEW = now;
                }
                break;
            case "EWYellow":
                if ((now - _lastChangeTimeEW).TotalSeconds >= 5)
                {
                    _currentLightStateEW = "AllRed";
                    _lastChangeTimeEW = now;
                }
                break;
            case "AllRed":
                if ((now - _lastChangeTimeEW).TotalSeconds >= 4)
                {
                    _currentLightStateEW = "EWGreen";
                    _lastChangeTimeEW = now;
                }
                break;
        }

        await _hubContext.Clients.All.SendAsync("UpdateLightStateEW", _currentLightStateEW);

        await _hubContext.Clients.All.SendAsync("UpdateLightState", _currentLightStateNS);
    }


   

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
