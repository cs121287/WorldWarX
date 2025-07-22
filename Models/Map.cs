namespace WorldWarX
{
    /// <summary>
    /// Basic map data structure.
    /// For full game map functionality, use WorldWarX.Models.GameMap instead.
    /// </summary>
    public class Map
    {
        public required string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Description { get; set; }
        public string? PreviewImagePath { get; set; }
    }
}