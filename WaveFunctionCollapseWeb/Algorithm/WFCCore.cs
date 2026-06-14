using WaveFunctionCollapseWeb.Data;
using WaveFunctionCollapseWeb.Models;

namespace WaveFunctionCollapseWeb.Algorithm;

public static class WFCCore
{
    public static string Seed = "00000000";
    public static bool MaxGenerationSpeed = true;
    public static int GenerationSpeed = 10;
    public static bool UseInput = true;
    public static bool Biomes = false;
    public static string Instructions = "Tile by tile";
    public static string Collapse = "Entropy";
    public static int OutputTimeValue = 100;
    private static Dictionary<(int, int), Guid> InputTiles = new();
    private static HashSet<RuleTile> RuleSet = new();
    private static bool inputChanged = false;
    private static Dictionary<(int, int), DisplayTile> DisplayTiles = new();
    private static HashSet<(int, int)> CollapsedTiles = new();
    private static Random random = new();
    private static HashSet<Guid> allPossibleTiles = new();

    public static void ChangeInputTiles(Dictionary<(int, int), Guid> Input)
    {
        InputTiles = Input;
        inputChanged = true;
    }
    public static List<List<Tile>> GenerateTiles(int width, int height)
    {
        return null;
    }

    private static void LoadRules()
    {
        if (!InputTiles.Any())
            return;
        int smallestX = Int32.MaxValue;
        int smallestY = Int32.MaxValue;
        int biggestX = Int32.MinValue;
        int biggestY = Int32.MinValue;
        allPossibleTiles = new();
        foreach (var tile in InputTiles)
        {
            allPossibleTiles.Add(tile.Value);
            var kvp = tile.Key;
            if(kvp.Item1 < smallestX)
                smallestX = kvp.Item1;
            if(kvp.Item1 > biggestX)
                biggestX = kvp.Item1;
            if(kvp.Item2 < smallestY)
                smallestY = kvp.Item2;
            if(kvp.Item2 > biggestY)
                biggestY = kvp.Item2;
        }
        
        FillEntropy(allPossibleTiles);
        
        if (!inputChanged)
            return;
        inputChanged = false;
        RuleSet = new HashSet<RuleTile>();

        if (Instructions == "Tile by tile")
        {
            foreach (var tile in InputTiles)
            {
                RuleTile rule = RuleSet.FirstOrDefault(x => x.ID == tile.Value) ?? new RuleTile(tile.Value);
                int x = tile.Key.Item1;
                int y = tile.Key.Item2;
                if (x - 1 >= smallestX)
                {
                    AddNormalNeighbors(x-1,y,ref rule,allPossibleTiles);
                }
                if (x + 1 <= biggestX)
                {
                    AddNormalNeighbors(x+1,y,ref rule,allPossibleTiles);
                }
                if (y - 1 >= smallestY)
                {
                    AddNormalNeighbors(x,y-1,ref rule,allPossibleTiles);
                }
                if (y + 1 <= biggestY)
                {
                    AddNormalNeighbors(x,y+1,ref rule,allPossibleTiles);
                }
                RuleSet.Add(rule);
            }
        }
    }

    private static void AddNormalNeighbors(int x, int y, ref RuleTile rule, HashSet<Guid> allTiles)
    {
        if (InputTiles.TryGetValue((x, y), out Guid guid))
        {
            rule.AllowedNeighbors.Add(guid);
        }
    }
    private static void FillEntropy(HashSet<Guid> allPossibilities)
    {
        int temp = 20;
        DisplayTiles = new();
        for (int i = 0; i < temp; i++)
        {
            for (int j = 0; j < temp; j++)
            {
                var display = new DisplayTile()
                {
                    Entropy = new HashSet<Guid>(allPossibilities)
                };
                DisplayTiles.Add((i,j), display);
            }
        }
    }

    private static void HandleQueueAddition(bool isSuccess, (int X, int Y) location, DisplayTile tile, ref HashSet<Guid> centerEntropy, ref Queue<QueueTile> queue, ref HashSet<(int, int)> tried, Dictionary<(int, int), DisplayTile> main)
    {
        if (isSuccess && !tried.Contains(location))
        {
            HashSet<Guid> finished = null;
            foreach (var entropyGuid in centerEntropy)
            {
                if (finished == null)
                {
                    finished = new HashSet<Guid>(RuleSet.ToList().First(x=>x.ID == entropyGuid).AllowedNeighbors);
                }
                else
                {
                    finished.UnionWith(RuleSet.ToList().First(x=>x.ID == entropyGuid).AllowedNeighbors);
                    //finished.IntersectWith(RuleSet.ToList().First(x=>x.ID == entropyGuid).AllowedNeighbors);
                }
            }
            
            (int X,int Y) leftLoc = (location.X - 1, location.Y);
            (int X,int Y) rightLoc = (location.X + 1, location.Y);
            (int X,int Y) topLoc = (location.X, location.Y + 1);
            (int X,int Y) downLoc = (location.X, location.Y - 1);
            bool isLeftSuccess = main.TryGetValue(leftLoc, out var leftTile);
            bool isRightSuccess = main.TryGetValue(rightLoc, out var rightTile);
            bool isTopSuccess = main.TryGetValue(topLoc, out var topTile);
            bool isDownSuccess = main.TryGetValue(downLoc, out var downTile);
            
            //intersect finished with all four neighbors

            if (isLeftSuccess)
            {
                HashSet<Guid> mini = new HashSet<Guid>();
                foreach (var entropy in leftTile.Entropy)
                {
                    mini.UnionWith(RuleSet.ToList().First(x=>x.ID == entropy).AllowedNeighbors);
                }
                finished.IntersectWith(mini);
            }

            if (isRightSuccess)
            {
                HashSet<Guid> mini = new HashSet<Guid>();
                foreach (var entropy in rightTile.Entropy)
                {
                    mini.UnionWith(RuleSet.ToList().First(x=>x.ID == entropy).AllowedNeighbors);
                }
                finished.IntersectWith(mini);
            }

            if (isTopSuccess)
            {
                HashSet<Guid> mini = new HashSet<Guid>();
                foreach (var entropy in topTile.Entropy)
                {
                    mini.UnionWith(RuleSet.ToList().First(x=>x.ID == entropy).AllowedNeighbors);
                }
                finished.IntersectWith(mini);
            }

            if (isDownSuccess)
            {
                HashSet<Guid> mini = new HashSet<Guid>();
                foreach (var entropy in downTile.Entropy)
                {
                    mini.UnionWith(RuleSet.ToList().First(x=>x.ID == entropy).AllowedNeighbors);
                }
                finished.IntersectWith(mini);
            }

            if (!finished.SetEquals(tile.Entropy) && finished.Count != 0)
            {
                //OR tile.Entropy = new HashSet<Guid>(finished);
                tile.Entropy.IntersectWith(finished);
                main[(location.X, location.Y)] = tile;
                
                
                if(isLeftSuccess)
                    queue.Enqueue(new QueueTile(){X = leftLoc.X, Y = leftLoc.Y, Tile = leftTile});
                if(isRightSuccess)
                    queue.Enqueue(new QueueTile(){X = rightLoc.X, Y = rightLoc.Y, Tile = rightTile});
                if(isTopSuccess)
                    queue.Enqueue(new QueueTile(){X = topLoc.X, Y = topLoc.Y, Tile = topTile});
                if(isDownSuccess)
                    queue.Enqueue(new QueueTile(){X = downLoc.X, Y = downLoc.Y, Tile = downTile});
            }

            tried.Add(location);
        }
    }

    private static void UpdateCollapsedTiles(ref Queue<QueueTile> queue, ref Dictionary<(int, int), DisplayTile> main, ref HashSet<(int, int)> tried, bool isFirst = false)
    {
        if (queue.Count == 0)
            return;
        var readied = queue.Dequeue();
        (int X,int Y) leftLoc = (readied.X - 1, readied.Y);
        (int X,int Y) rightLoc = (readied.X + 1, readied.Y);
        (int X,int Y) topLoc = (readied.X, readied.Y + 1);
        (int X,int Y) downLoc = (readied.X, readied.Y - 1);
        bool isLeftSuccess = main.TryGetValue(leftLoc, out var leftTile);
        bool isRightSuccess = main.TryGetValue(rightLoc, out var rightTile);
        bool isTopSuccess = main.TryGetValue(topLoc, out var topTile);
        bool isDownSuccess = main.TryGetValue(downLoc, out var downTile);
        HashSet<Guid> centerEntropy = new HashSet<Guid>(readied.Tile.Entropy);
        
        HandleQueueAddition(isLeftSuccess, leftLoc, leftTile, ref centerEntropy, ref queue, ref tried, main);
        HandleQueueAddition(isRightSuccess, rightLoc, rightTile, ref centerEntropy, ref queue, ref tried, main);
        HandleQueueAddition(isTopSuccess, topLoc, topTile, ref centerEntropy, ref queue, ref tried, main);
        HandleQueueAddition(isDownSuccess, downLoc, downTile, ref centerEntropy, ref queue, ref tried, main);
        
        UpdateCollapsedTiles(ref queue, ref main, ref tried);
    }
    
    public static void Generate()
    {
        random = new Random(Seed.GetHashCode());
        CollapsedTiles.Clear();
        LoadRules();
        if (DisplayTiles.Count == 0)
            return;
        while(CollapsedTiles.Count < 20 * 20) //CHANGE THIS TO BE DYNAMIC
        {
            var display = DisplayTiles.Where(x=>!x.Value.Collapsed).OrderBy(x => x.Value.Entropy.Count()).ToList();
            bool success = false;
            KeyValuePair<(int, int), DisplayTile> chosen = display.First();
            int entropyCount = chosen.Value.Entropy.Count();

            foreach (var keyValuePair in display)
            {
                if (entropyCount != keyValuePair.Value.Entropy.Count())
                {
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                chosen = display[random.Next(0, display.Count)];
            }
            
            
            while (CollapsedTiles.Contains(chosen.Key))
            {
                chosen = display[random.Next(0, display.Count)]; //EXTREMELY INEFFICIENT FOR BIGGER PIECES
            }

            var collapsed = chosen.Value.Entropy.ToList()[random.Next(0, chosen.Value.Entropy.Count)];
            DataManager.AddTile(chosen.Key, collapsed);
            CollapsedTiles.Add(chosen.Key);
            var fullTile = new DisplayTile() { Entropy = new HashSet<Guid>() { collapsed }, Collapsed = true };
            DisplayTiles[chosen.Key] = fullTile;
            
            Queue<QueueTile> queue = new();
            HashSet<(int, int)> triedOnes = new();
            queue.Enqueue(new QueueTile() {X = chosen.Key.Item1, Y = chosen.Key.Item2, Tile = fullTile});
            UpdateCollapsedTiles(ref queue, ref DisplayTiles, ref triedOnes, true);
        }
    }
}