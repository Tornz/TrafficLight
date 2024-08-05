

using TrafficLight.Interfaces;

public class TrafficLightService : ITrafficLight
{
    private string _currentState;

    public void SetGreen()
    {
        _currentState = "Green";
    }

    public void SetYellow()
    {
        _currentState = "Yellow";
    }

    public void SetRed()
    {
        _currentState = "Red";
    }

    public string GetCurrentState()
    {
        return _currentState;
    }
}