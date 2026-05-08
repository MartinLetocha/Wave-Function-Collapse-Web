namespace WaveFunctionCollapseWeb.Models;

public class DisplayTile
{
    public HashSet<Guid> Entropy { get; set; }
    public bool Collapsed { get; set; } = false;
}