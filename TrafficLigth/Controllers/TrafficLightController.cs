using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficLight.Interfaces;

public class TrafficLightController : Controller
{
    private readonly ITrafficLight _northLight;
    private readonly ITrafficLight _southLight;
    private readonly ITrafficLight _eastLight;
    private readonly ITrafficLight _westLight;
    private readonly ITrafficLight _northRightTurnLight;

    public TrafficLightController(ITrafficLight northLight, ITrafficLight southLight, ITrafficLight eastLight, ITrafficLight westLight, ITrafficLight northRightTurnLight)
    {
        _northLight = northLight;
        _southLight = southLight;
        _eastLight = eastLight;
        _westLight = westLight;
        _northRightTurnLight = northRightTurnLight;
    }

    public async Task<IActionResult> Index()
    {             
        return View();
    }



    [HttpPost]
    public async Task LoadLight()
    {
        await manage();        
    }

    public async Task manage()
    {
        while (true)
        {
            await ManageTrafficLights();
            UpdateViewBag();
        }
    }


    public async Task<IActionResult> GetLatestColor()
    {
        UpdateViewBag();
        return View("Index");
    }



    private void UpdateViewBag()
    {
        ViewBag.NorthLight = _northLight.GetCurrentState();
        ViewBag.SouthLight = _southLight.GetCurrentState();
        ViewBag.EastLight = _eastLight.GetCurrentState();
        ViewBag.WestLight = _westLight.GetCurrentState();
        ViewBag.NorthRightTurnLight = _northRightTurnLight.GetCurrentState();        
    }

    private async Task ManageTrafficLights()
    {
        TimeSpan now = DateTime.Now.TimeOfDay;
        bool isPeakHour = (now >= new TimeSpan(8, 0, 0) && now <= new TimeSpan(10, 0, 0)) || (now >= new TimeSpan(17, 0, 0) && now <= new TimeSpan(19, 0, 0));

        int greenDurationNorthSouth = isPeakHour ? 40 : 20;
        int greenDurationEastWest = isPeakHour ? 10 : 20;

        await SetLightSequence(greenDurationNorthSouth, greenDurationEastWest);
    }

    private async Task SetLightSequence(int northSouthGreenDuration, int eastWestGreenDuration)
    {
        // North and South Green
        _northLight.SetGreen();
        _southLight.SetGreen();
        await Task.Delay(TimeSpan.FromSeconds(northSouthGreenDuration - 5));

        _northLight.SetYellow();
        _southLight.SetYellow();
        await Task.Delay(TimeSpan.FromSeconds(5));

        _northLight.SetRed();
        _southLight.SetRed();
        await Task.Delay(TimeSpan.FromSeconds(4));

        // North Right Turn Green
        _northRightTurnLight.SetGreen();
        _northLight.SetGreen();
        await Task.Delay(TimeSpan.FromSeconds(10));

        _northRightTurnLight.SetRed();
        _northLight.SetRed();
        await Task.Delay(TimeSpan.FromSeconds(4));

        // East and West Green
        _eastLight.SetGreen();
        _westLight.SetGreen();
        await Task.Delay(TimeSpan.FromSeconds(eastWestGreenDuration - 5));

        _eastLight.SetYellow();
        _westLight.SetYellow();
        await Task.Delay(TimeSpan.FromSeconds(5));

        _eastLight.SetRed();
        _westLight.SetRed();
        await Task.Delay(TimeSpan.FromSeconds(4));
    }
}