namespace WaveFunctionCollapseWeb.Models;

public class Tile
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public string Background { get; set; }
    public string Text { get; set; }
    public Image Image { get; set; }
    public string Biome { get; set; }
    public ExceptionType ExceptionType { get; set; }
}