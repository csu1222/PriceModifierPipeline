namespace PriceModifierPipeline.Debugging
{
    public interface IPriceArchitectureDebugCommand
    {
        void Preview();
        void Commit();
        void NextSeason();
        void ToggleDistance();
        void ToggleEvent();
        void NextWeather();
        void ToggleItemType();
        void Reset();
    }
}
