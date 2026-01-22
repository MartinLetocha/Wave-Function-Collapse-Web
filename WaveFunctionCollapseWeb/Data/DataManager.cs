using Microsoft.AspNetCore.Components.Forms;
using WaveFunctionCollapseWeb.Models;

namespace WaveFunctionCollapseWeb.Data;

public static class DataManager
{
    //meta
    private static readonly object _lock = new();
    public static int testCounter = 0;
    private static bool once = false;
    //persistent
    public static Guid selectedTileOverview = Guid.Empty;
    public static Guid[] tilePalette = [Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty];
    public static Tile paintingTile = new Tile() { Text = "", Background = "#42f5a1", Name = "Test - Painting", ID = Guid.Empty, Category = "Debug"};
    public static Dictionary<(int, int), Guid> tiles = new();
    public static Dictionary<Guid, Tile> tileDatabase = new();
    //input tileMap
    public static int width = 0;
    public static int height = 0;
    public static int offsetX = 0;
    public static int offsetY = 0;
    public static Dictionary<(int, int), Guid> viewTiles = new();
    //output tileMap
    public static int widthOutput = 0;
    public static int heightOutput = 0;
    public static int offsetXOutput = 0;
    public static int offsetYOutput = 0;

    public static void Start()
    {
        if(once) return;
        once = true;
        string rootpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        AddTileToDatabase(new Tile() { Category = "Basic", Name = "Grass", TextColor = "#fff", Background = "#445", Image = new Image(){Data=File.ReadAllBytes(Path.Combine(rootpath, "pics/grassTile.png")), DataType = "image/png"}});
        AddTileToDatabase(new Tile() { Category = "Basic", Name = "Rock", TextColor = "#fff", Background = "#445", Image = new Image(){Data=File.ReadAllBytes(Path.Combine(rootpath, "pics/rockTile.png")), DataType = "image/png"}});
        AddTileToDatabase(new Tile() { Category = "Basic", Name = "Sand", TextColor = "#fff", Background = "#445", Image = new Image(){Data=File.ReadAllBytes(Path.Combine(rootpath, "pics/sandTile.png")), DataType = "image/png"}});
        AddTileToDatabase(new Tile() { Category = "Basic", Name = "Water", TextColor = "#fff", Background = "#445", Image = new Image(){Data=File.ReadAllBytes(Path.Combine(rootpath, "pics/seaTile.png")), DataType = "image/png"}});
    }
    
    public static void AddTile((int, int) index, Guid id)
    {
        lock (_lock)
        {
            tiles.TryAdd(index, id);
        }
    }

    public static void SetPalette(int number, Guid id)
    {
        tilePalette[number] = id;
    }

    public static Tile GetTileFromDb(Guid id)
    {
        //remove this after
        if(id == Guid.Empty)
            return new Tile() { Text = "", Background = "#42f5a1", Name = "Test - Painting", ID = Guid.Empty, Category = "Debug" };
        //TODO: yea
        lock (_lock)
        {
            return tileDatabase[id];
        }
    }

    public static bool EditTileInDatabase(Guid id, Tile tile)
    {
        lock (_lock)
        {
            if (tileDatabase.ContainsKey(id))
            {
                tileDatabase[id] = tile;
            }
            else
            {
                return false;
            }
        }
        return true;
    }
    public static Guid AddTileToDatabase(Tile tile, Guid? withId = null)
    {
        lock (_lock)
        {
            var tileInDb = tileDatabase.FirstOrDefault(x =>
                x.Value.Name == tile.Name);

            if (tileInDb.Value != null)
            {
                return tileInDb.Key;
            }

            var id = withId == null ? Guid.NewGuid() : withId.Value;
            tile.ID = id;
            tileDatabase.TryAdd(id, tile);
            return id;
        }
    }

    public static int GetTileCount()
    {
        lock (_lock)
        {
            int count = tiles.Count;
            return count;
        }
    }
}