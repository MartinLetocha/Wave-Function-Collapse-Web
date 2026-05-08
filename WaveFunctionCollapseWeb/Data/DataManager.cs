using Microsoft.AspNetCore.Components.Forms;
using WaveFunctionCollapseWeb.Algorithm;
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
    //options
    public delegate void SettingChanged<T>(int id, T value);
    public static Dictionary<int, object> settings = new();
    public static event SettingChanged<int> OnSettingChangeInt;
    public static event SettingChanged<string> OnSettingChangeString;
    public static event SettingChanged<bool> OnSettingChangeBool;
    public static event SettingChanged<double> OnSettingChangeDouble;
    //refresh
    public delegate void Refresh();
    public static event Refresh OnDisplayChange;

    public static void Start()
    {
        if(once) return;
        once = true;
        
        OnSettingChangeInt += OnSettingChange;
        OnSettingChangeString += OnSettingChange;
        OnSettingChangeBool += OnSettingChange;
        OnSettingChangeDouble += OnSettingChange;
        string rootpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        Guid first = AddTileToDatabase(new Tile() { Category = "Basic", Name = "Grass", TextColor = "#fff", Background = "#445", Image = new Image(){Data=File.ReadAllBytes(Path.Combine(rootpath, "pics/grassTile.png")), DataType = "image/png"}});
        AddTileToDatabase(new Tile() { Category = "Basic", Name = "Rock", TextColor = "#fff", Background = "#445", Image = new Image(){Data=File.ReadAllBytes(Path.Combine(rootpath, "pics/rockTile.png")), DataType = "image/png"}});
        AddTileToDatabase(new Tile() { Category = "Basic", Name = "Sand", TextColor = "#fff", Background = "#445", Image = new Image(){Data=File.ReadAllBytes(Path.Combine(rootpath, "pics/sandTile.png")), DataType = "image/png"}});
        AddTileToDatabase(new Tile() { Category = "Basic", Name = "Water", TextColor = "#fff", Background = "#445", Image = new Image(){Data=File.ReadAllBytes(Path.Combine(rootpath, "pics/seaTile.png")), DataType = "image/png"}});
        AddTileToDatabase(new Tile() { Category = "Basic", Name = "Tree", TextColor = "#fff", Background = "#445", Image = new Image(){Data=File.ReadAllBytes(Path.Combine(rootpath, "pics/treeTile.png")), DataType = "image/png"}});
        var tileList = GetTilesFromDbByCategory("Basic");
        for (int i = 0; i < tilePalette.Length; i++)
        {
            tilePalette[i] = tileList[i].ID;
        }
        paintingTile = tileList[0];
        selectedTileOverview = first;
        AddDefaultSettings();
    }

    private static void OnSettingChange<T>(int id, T value)
    {
        switch (id)
        {
            case 2:
                //send to input with template
                break;
            case 3:
                //just if WFC should use input or direct tile instructions (send to WFC)
                WFCCore.UseInput = Convert.ToBoolean(value);
                break;
            case 4: //biomes (send to WFC)
                WFCCore.Biomes = Convert.ToBoolean(value);
                break;
            case 5: //instructions (send to WFC) INPUT CAN BE OFF BUT ALL PICKED TILES HAVE TO HAVE INSTRUCTIONS FOR CHOSEN TYPE
                WFCCore.Instructions = value.ToString();
                break;
            case 6: //collapse, send to WFC
                WFCCore.Collapse = value.ToString();
                break;
            case 7: //7 and 8 generation speed, send to WFC
                WFCCore.MaxGenerationSpeed = Convert.ToBoolean(value);
                break;
            case 8:
                WFCCore.GenerationSpeed = Convert.ToInt32(value);
                break;
            case 9: //seed, send to WFC
                WFCCore.Seed = value.ToString();
                break;
            case 10: //output time slider, get WFC snapshot at value
                WFCCore.OutputTimeValue = Convert.ToInt32(value);
                break;
            default: //shouldn't happen but just in case
                break;
        }
    }

    private static void AddDefaultSettings()
    {
        settings.Add(0, 8);
        settings.Add(1, 20);
        settings.Add(2, "None");
        settings.Add(3, true);
        settings.Add(4, false);
        settings.Add(5, "Tile by tile");
        settings.Add(6, "Entropy");
        settings.Add(7, true);
        settings.Add(8, 10);
        settings.Add(9, "00000000");
        settings.Add(10, 100);
    }
    
    public static void AddTile((int, int) index, Guid id)
    {
        lock (_lock)
        {
            if (!tiles.TryAdd(index, id))
            {
                tiles[index] = id;
            }
            OnDisplayChange?.Invoke();
        }
    }

    public static void SetPalette(int number, Guid id)
    {
        tilePalette[number] = id;
    }

    public static List<Tile> GetTilesFromDbByCategory(string category)
    {
        List<Tile> tiles = new();
        foreach (Tile tile in tileDatabase.Values)
        {
            if (tile.Category == category)
            {
                tiles.Add(tile);
            }
        }
        return tiles;
    }
    public static Tile GetTileFromDb(Guid id)
    {
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

    public static void InvokeSettingsChangeEvent<T>(int id, T value)
    {
        Type type = value.GetType();
        if (type == typeof(double))
        {
            OnSettingChangeDouble?.Invoke(id, Convert.ToDouble(value));
        }

        if (type == typeof(int))
        {
            OnSettingChangeInt?.Invoke(id, Convert.ToInt32(value));
        }

        if (type == typeof(string))
        {
            OnSettingChangeString?.Invoke(id, value.ToString());
        }

        if (type == typeof(bool))
        {
            OnSettingChangeBool?.Invoke(id, Convert.ToBoolean(value));
        }
        if (value != null) {settings[id] = value;}
        else{settings.Add(id, value);}
    }
}