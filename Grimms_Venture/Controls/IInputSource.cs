namespace GrimmsVenture.Controls;

public interface IInputSource
{
    string Name { get; }
    bool IsAvailable { get; }
    PlayerInput Poll();
}
