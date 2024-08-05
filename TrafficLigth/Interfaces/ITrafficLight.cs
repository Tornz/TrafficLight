namespace TrafficLight.Interfaces
{
    public interface ITrafficLight
    {
        void SetGreen();
        void SetYellow();
        void SetRed();
        string GetCurrentState();
    }
}
