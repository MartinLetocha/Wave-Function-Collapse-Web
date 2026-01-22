using WaveFunctionCollapseWeb.Data;
using WaveFunctionCollapseWeb.Models;

namespace WaveFunctionCollapseWeb.Algorithm;

public static class WFCCore
{
    public static List<List<Tile>> GenerateTiles(int width, int height)
    {
        return null;
    }

    public static void Generate()
    {
        if (DataManager.GetTileCount() != 0)
            return;
        Guid greenSquare = DataManager.AddTileToDatabase(new Tile() { Text = "🟩", Name = "Test - Green Square", Category = "Debug", Background = "#445", TextColor = "#fff"});
        Guid redSquare = DataManager.AddTileToDatabase(new Tile() { Text = "🟥", Name = "Test - Red Square", Category = "Debug", Background = "#445", TextColor = "#fff" });
        Guid blueSquare = DataManager.AddTileToDatabase(new Tile() { Text = "🟦", Name = "Test - Blue Square", Category = "Debug", Background = "#445", TextColor = "#fff"});
        for (int y = -50; y < 50; y++)
        {
            for (int x = -50; x < 50; x++)
            {
                // DataManager.tiles.TryAdd((x, y), new Models.Tile() { X = x, Y = y, Text = $"{x},{y}"});
                if (x == 19 || y == 19 || x == 0 || y == 0)
                {
                    DataManager.AddTile((x, y), greenSquare);
                }
                else if (x % 2 == 0)
                {
                    DataManager.AddTile((x, y), redSquare);
                }
                else
                {
                    DataManager.AddTile((x, y), blueSquare);
                }
            }
        }
    }
}