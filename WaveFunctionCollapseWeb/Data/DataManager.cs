using WaveFunctionCollapseWeb.Models;

namespace WaveFunctionCollapseWeb.Data;

public static class DataManager
{
    public static int testCounter = 0;
    public static Tile paintingTile = new Tile() { Text = "", Background = "#42f5a1", Name = "Test - Painting" };
    public static Dictionary<(int, int), Guid> tiles = new();
    public static Dictionary<Guid, Tile> tileDatabase = new();
    private static readonly object _lock = new();
    //input tileMap
    public static int width = 0;
    public static int height = 0;
    public static int offsetX = 0;
    public static int offsetY = 0;
    public static Dictionary<(int, int), Guid> viewTiles = new();
    public static void AddTile((int, int) index, Guid id)
    {
        lock (_lock)
        {
            tiles.TryAdd(index, id);
        }
    }

    public static Tile GetTileFromDb(Guid id)
    {
        lock (_lock)
        {
            return tileDatabase[id];
        }
    }
    public static Guid AddTileToDatabase(Tile tile)
    {
        lock (_lock)
        {
            var tileInDb = tileDatabase.FirstOrDefault(x =>
                x.Value.Background == tile.Background && x.Value.Text == tile.Text && tile.Image == x.Value.Image);

            if (tileInDb.Value != null)
            {
                return tileInDb.Key;
            }

            Guid id = Guid.NewGuid();
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