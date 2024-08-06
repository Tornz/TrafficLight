using Microsoft.AspNetCore.Mvc;

public class TrafficLightController : Controller
{
    public TrafficLightController()
    {
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }




}