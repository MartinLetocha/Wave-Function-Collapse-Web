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
        RuleSet = new HashSet<RuleTile>();
        inputChanged = false;
        int smallestX = Int32.MaxValue;
        int smallestY = Int32.MaxValue;
        int biggestX = Int32.MinValue;
        int biggestY = Int32.MinValue;
        HashSet<Guid> allTiles = new();
        foreach (var tile in InputTiles)
        {
            allTiles.Add(tile.Value);
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
        
        FillEntropy(allTiles);
        
        if (!inputChanged)
            return;

        if (Instructions == "Tile by tile")
        {
            foreach (var tile in InputTiles)
            {
                RuleTile rule = RuleSet.FirstOrDefault(x => x.ID == tile.Value) ?? new RuleTile(tile.Value);
                //find input bounds
                //add neighbors
                int x = tile.Key.Item1;
                int y = tile.Key.Item2;
                if (x - 1 >= smallestX)
                {
                    AddNormalNeighbors(x-1,y,ref rule,allTiles);
                }
                if (x + 1 <= biggestX)
                {
                    AddNormalNeighbors(x+1,y,ref rule,allTiles);
                }
                if (y - 1 >= smallestY)
                {
                    AddNormalNeighbors(x,y-1,ref rule,allTiles);
                }
                if (y + 1 <= biggestY)
                {
                    AddNormalNeighbors(x,y+1,ref rule,allTiles);
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
        else
        {
            rule.AllowedNeighbors = allTiles;
        }
    }
    private static void FillEntropy(HashSet<Guid> allPossibilities)
    {
        int temp = 20;
        var display = new DisplayTile() { Entropy = allPossibilities };
        DisplayTiles = new();
        for (int i = 0; i < temp; i++)
        {
            for (int j = 0; j < temp; j++)
            {
                DisplayTiles.Add((i,j), display);
            }
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
        HashSet<Guid> allPossible = new();
        if (isLeftSuccess && !tried.Contains(leftLoc))
        {
            allPossible.UnionWith(leftTile.Entropy);
            queue.Enqueue(new QueueTile(){X = leftLoc.X, Y = leftLoc.Y, Tile = leftTile});
            tried.Add(leftLoc);
        }
        if (isRightSuccess && !tried.Contains(rightLoc))
        {
            allPossible.UnionWith(rightTile.Entropy);
            queue.Enqueue(new QueueTile(){X = rightLoc.X, Y = rightLoc.Y, Tile = rightTile});
            tried.Add(rightLoc);
        }
        if (isTopSuccess && !tried.Contains(topLoc))
        {
            allPossible.UnionWith(topTile.Entropy);
            queue.Enqueue(new QueueTile(){X = topLoc.X, Y = topLoc.Y, Tile = topTile});
            tried.Add(topLoc);
        }
        if (isDownSuccess && !tried.Contains(downLoc))
        {
            allPossible.UnionWith(downTile.Entropy);
            queue.Enqueue(new QueueTile(){X = downLoc.X, Y = downLoc.Y, Tile = downTile});
            tried.Add(downLoc);
        }

        if (!isFirst)
        {
            if (allPossible.Count != 0)
            {
                var old = main[(readied.X, readied.Y)];
                old.Entropy = allPossible;
                main[(readied.X, readied.Y)] = old;
            }
        }

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
            //if they all have the same entropy, then don't pick randomly
            KeyValuePair<(int, int), DisplayTile> chosen = display[random.Next(0, display.Count)];
            
            while (CollapsedTiles.Contains(chosen.Key))
            {
                chosen = display[random.Next(0, display.Count)]; //EXTREMELY INEFFICIENT FOR BIGGER PIECES
            }

            var collapsed = chosen.Value.Entropy.ToList()[random.Next(0, chosen.Value.Entropy.Count)];
            DataManager.AddTile(chosen.Key, collapsed);
            CollapsedTiles.Add(chosen.Key);
            var fullTile = new DisplayTile() { Entropy = new HashSet<Guid>() { collapsed }, Collapsed = true };
            DisplayTiles[chosen.Key] = fullTile;
            //walk through displayTiles, update according to collapsed
            //queue up all neighbors of collapsed tiles and remove their entropy, when removing entropy check those neighbors too, go through queue by the recency of being added
            
            Queue<QueueTile> queue = new();
            HashSet<(int, int)> triedOnes = new();
            queue.Enqueue(new QueueTile() {X = chosen.Key.Item1, Y = chosen.Key.Item2, Tile = fullTile});
            UpdateCollapsedTiles(ref queue, ref DisplayTiles, ref triedOnes, true);
        }

        // if (DataManager.GetTileCount() != 0)
        //     return;
        // Guid greenSquare = DataManager.AddTileToDatabase(new Tile() { Text = "🟩", Name = "Test - Green Square", Category = "Debug", Background = "#445", TextColor = "#fff"});
        // Guid redSquare = DataManager.AddTileToDatabase(new Tile() { Text = "🟥", Name = "Test - Red Square", Category = "Debug", Background = "#445", TextColor = "#fff" });
        // Guid blueSquare = DataManager.AddTileToDatabase(new Tile() { Text = "🟦", Name = "Test - Blue Square", Category = "Debug", Background = "#445", TextColor = "#fff"});
        // for (int y = -50; y < 50; y++)
        // {
        //     for (int x = -50; x < 50; x++)
        //     {
        //         // DataManager.tiles.TryAdd((x, y), new Models.Tile() { X = x, Y = y, Text = $"{x},{y}"});
        //         if (x == 19 || y == 19 || x == 0 || y == 0)
        //         {
        //             DataManager.AddTile((x, y), greenSquare);
        //         }
        //         else if (x % 2 == 0)
        //         {
        //             DataManager.AddTile((x, y), redSquare);
        //         }
        //         else
        //         {
        //             DataManager.AddTile((x, y), blueSquare);
        //         }
        //     }
        // }
    }
    
}