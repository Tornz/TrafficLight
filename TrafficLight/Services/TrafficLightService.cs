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
    private string _currentLightState;
    
    private DateTime _lastChangeTime;
    

    public TrafficLightService(ILogger<TrafficLightService> logger, IHubContext<TrafficLightHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
        _currentLightState = "NSGreen"; // Starting state        
        _lastChangeTime = DateTime.Now;
        
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
        var greenDuration = isPeakTime ? 40 : 10;
        var eastWestGreenDuration = isPeakTime ? 10 : 10;

        switch (_currentLightState)
        {
            case "NSGreen":
                if ((now - _lastChangeTime).TotalSeconds >= greenDuration)
                {
                    _currentLightState = "NSYellow";
                    _lastChangeTime = now;
                }
                break;
            case "NSYellow":
                if ((now - _lastChangeTime).TotalSeconds >= 5)
                {
                    _currentLightState = "NSRed";
                    _lastChangeTime = now;
                }
                break;
            case "NSRed":
                if ((now - _lastChangeTime).TotalSeconds >= 4)
                {                   
                    _currentLightState = "EWGreen";
                    _lastChangeTime = now;
                }
                break;
            case "EWGreen":
                if ((now - _lastChangeTime).TotalSeconds >= eastWestGreenDuration)
                {
                    _currentLightState = "EWYellow";
                    _lastChangeTime = now;
                }
                break;

            case "EWYellow":
                if ((now - _lastChangeTime).TotalSeconds >= 5)
                {
                    _currentLightState = "EWRed";
                    _lastChangeTime = now;
                }
                break;

            case "EWRed":
                if ((now - _lastChangeTime).TotalSeconds >= 4)
                {
                    _currentLightState = "NSGreenRightTurn";
                    _lastChangeTime = now;
                }
                break;

            case "NSGreenRightTurn":
                if ((now - _lastChangeTime).TotalSeconds >= 10)
                {
                    _currentLightState = "NSGreenRightTurnYellow";
                    _lastChangeTime = now;
                }
                break;

            case "NSGreenRightTurnYellow":
                if ((now - _lastChangeTime).TotalSeconds >= 5)
                {
                    _currentLightState = "NSGreenRightTurnRed";
                    _lastChangeTime = now;
                }
                break;
            case "NSGreenRightTurnRed":
                if ((now - _lastChangeTime).TotalSeconds >= 4)
                {
                    _currentLightState = "NSGreen";
                    _lastChangeTime = now;
                }
                break;
        }
        await _hubContext.Clients.All.SendAsync("UpdateLightState", _currentLightState);
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
