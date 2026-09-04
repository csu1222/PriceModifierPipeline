namespace PriceModifierPipeline.Debugging
{
    public interface IPriceArchitectureDebugSource
    {
        PriceDebugSnapshot CaptureSnapshot();
    }
}
