namespace WaveFunctionCollapseWeb.Models;

public class RuleTile
{
    public Guid ID { get; set; }
    public HashSet<Guid> AllowedNeighbors { get; set; } = new();
    public HashSet<(Guid, float)> AllowedNeighborsWeighted { get; set; } = new();
    public HashSet<Guid>[] AllowedNeighborsDirectional { get; set; } = new HashSet<Guid>[4];
    public RuleTile(Guid self)
    {
        ID = self;
    }
}