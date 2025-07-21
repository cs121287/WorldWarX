using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WorldWarX.Models
{
    public enum MapSize
    {
        Small,      // 40x50 tiles
        Medium,     // 80x100 tiles
        Large,      // 160x200 tiles
        ExtraLarge  // 320x400 tiles
    }

    public class GameMap
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Tile[,] Tiles { get; private set; }
        public List<Player> Players { get; set; }
        public WeatherSystem WeatherSystem { get; private set; }
        public MapSeason Season { get; private set; }
        public FogOfWarSystem FogOfWar { get; private set; }
        public Player CurrentPlayer { get; set; }
        public MapSize Size { get; set; }
        public string PreviewImagePath { get; set; } // NEW: map preview image for editor/gallery
        public string Author { get; set; }           // NEW: author/creator for map metadata
        public DateTime CreatedDate { get; set; }    // NEW: created date for metadata
        public DateTime ModifiedDate { get; set; }   // NEW: last modified date for metadata

        public GameMap(string name, int width, int height)
        {
            Name = name;
            Description = ""; // Default value to avoid CS8618
            PreviewImagePath = ""; // Default value to avoid CS8618
            Author = ""; // Default value to avoid CS8618

            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
            Players = new List<Player>();
            Season = MapSeason.Summer; // Default to summer

            Size = DetermineMapSizeFromDimensions(width, height);

            // WeatherSystem and FogOfWar are initialized by InitializeWeather and InitializeFogOfWar
            WeatherSystem = null!;
            FogOfWar = null!;

            // CurrentPlayer may be set later by game logic
            CurrentPlayer = null!;

            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }

        // Initialize the map with default terrain (plains)
        public void InitializeEmptyMap()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y] = new Tile(TerrainType.Plain, x, y);
                }
            }
        }

        // Get tile at specific coordinates
        public Tile? GetTile(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Tiles[x, y];

            return null;
        }

        // Set tile at specific coordinates
        public void SetTile(int x, int y, TerrainType terrain)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Tiles[x, y] = new Tile(terrain, x, y);
                ModifiedDate = DateTime.UtcNow;
            }
        }

        // Initialize weather system
        public void InitializeWeather(MapSeason season, GameDifficulty difficulty, bool weatherEffectsEnabled, int? seed = null)
        {
            Season = season;
            WeatherSystem = new WeatherSystem(season, difficulty, weatherEffectsEnabled, seed);
        }

        // Initialize fog of war system
        public void InitializeFogOfWar(bool fogOfWarEnabled, bool advancedFogOfWar = true)
        {
            FogOfWar = new FogOfWarSystem(this);
            FogOfWar.Enabled = fogOfWarEnabled;
            FogOfWar.UseAdvancedMode = advancedFogOfWar;
        }

        // Update fog of war visibility
        public void UpdateVisibility()
        {
            if (FogOfWar != null)
                FogOfWar.UpdateVisibility();
        }

        // Returns the standard dimensions for a given map size
        public (int width, int height) GetDimensionsFromSize(MapSize size)
        {
            switch (size)
            {
                case MapSize.Small:
                    return (40, 50);
                case MapSize.Medium:
                    return (80, 100);
                case MapSize.Large:
                    return (160, 200);
                case MapSize.ExtraLarge:
                    return (320, 400);
                default:
                    return (Width, Height); // Custom size
            }
        }

        // Determine map size category based on dimensions
        private MapSize DetermineMapSizeFromDimensions(int width, int height)
        {
            if (width <= 40 && height <= 50)
                return MapSize.Small;
            else if (width <= 80 && height <= 100)
                return MapSize.Medium;
            else if (width <= 160 && height <= 200)
                return MapSize.Large;
            else if (width <= 320 && height <= 400)
                return MapSize.ExtraLarge;
            else
                return MapSize.Large; // Default to large for non-standard sizes
        }

        // Save the map to a file (compatible with LoadFromFile)
        public void SaveToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // First line: map name, width, height
                writer.WriteLine($"{Name},{Width},{Height}");

                // Terrain data
                for (int y = 0; y < Height; y++)
                {
                    var terrainCodes = new List<string>();
                    for (int x = 0; x < Width; x++)
                    {
                        terrainCodes.Add(GetTerrainCode(Tiles[x, y].TerrainType));
                    }
                    writer.WriteLine(string.Join(",", terrainCodes));
                }

                // Player data (HQ positions)
                writer.WriteLine(Players.Count);
                foreach (var player in Players)
                {
                    foreach (var tile in player.Properties)
                    {
                        if (tile.TerrainType == TerrainType.HQ)
                        {
                            writer.WriteLine($"{player.PlayerId},{tile.X},{tile.Y}");
                        }
                    }
                }
                // Optionally: add map metadata as comments at the end
                writer.WriteLine($"# Description:{Description}");
                writer.WriteLine($"# PreviewImage:{PreviewImagePath}");
                writer.WriteLine($"# Author:{Author}");
                writer.WriteLine($"# CreatedDate:{CreatedDate:o}");
                writer.WriteLine($"# ModifiedDate:{ModifiedDate:o}");
            }
        }

        // Helper to get terrain code (inverse of ParseTerrainCode)
        private static string GetTerrainCode(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Plain: return "P";
                case TerrainType.Forest: return "F";
                case TerrainType.Mountain: return "M";
                case TerrainType.Road: return "R";
                case TerrainType.City: return "C";
                case TerrainType.Factory: return "B";
                case TerrainType.HQ: return "H";
                case TerrainType.Sea: return "S";
                case TerrainType.Beach: return "E";
                case TerrainType.River: return "V";
                case TerrainType.Bridge: return "G";
                case TerrainType.Airport: return "A";
                case TerrainType.Seaport: return "T";
                default: return "P";
            }
        }

        // Load a map from a file
        public static GameMap? LoadFromFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                // First line contains map name and dimensions
                string[] mapInfo = lines[0].Split(',');
                string mapName = mapInfo[0];
                int width = int.Parse(mapInfo[1]);
                int height = int.Parse(mapInfo[2]);

                GameMap map = new GameMap(mapName, width, height);

                // Read terrain data
                for (int y = 0; y < height; y++)
                {
                    string[] terrainCodes = lines[y + 1].Split(',');
                    for (int x = 0; x < width; x++)
                    {
                        TerrainType terrain = ParseTerrainCode(terrainCodes[x]);
                        map.Tiles[x, y] = new Tile(terrain, x, y);
                    }
                }

                // Read player data (HQ positions)
                int playerDataStartLine = height + 1;
                int playerCount = int.Parse(lines[playerDataStartLine]);

                for (int i = 0; i < playerCount; i++)
                {
                    string[] playerInfo = lines[playerDataStartLine + i + 1].Split(',');
                    int playerId = int.Parse(playerInfo[0]);
                    int hqX = int.Parse(playerInfo[1]);
                    int hqY = int.Parse(playerInfo[2]);

                    // Set HQ ownership
                    if (hqX >= 0 && hqX < width && hqY >= 0 && hqY < height)
                    {
                        Player player = new Player(playerId);
                        map.Players.Add(player);

                        // HQ is automatically owned by this player
                        map.Tiles[hqX, hqY].Owner = player;
                    }
                }

                // Read map metadata if present in comments
                for (int i = playerDataStartLine + playerCount + 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line.StartsWith("# Description:"))
                        map.Description = line.Substring("# Description:".Length).Trim();
                    else if (line.StartsWith("# PreviewImage:"))
                        map.PreviewImagePath = line.Substring("# PreviewImage:".Length).Trim();
                    else if (line.StartsWith("# Author:"))
                        map.Author = line.Substring("# Author:".Length).Trim();
                    else if (line.StartsWith("# CreatedDate:"))
                    {
                        if (DateTime.TryParse(line.Substring("# CreatedDate:".Length).Trim(), out var dt))
                            map.CreatedDate = dt;
                    }
                    else if (line.StartsWith("# ModifiedDate:"))
                    {
                        if (DateTime.TryParse(line.Substring("# ModifiedDate:".Length).Trim(), out var dt))
                            map.ModifiedDate = dt;
                    }
                }

                // Initialize weather with default settings
                map.InitializeWeather(MapSeason.Summer, GameDifficulty.Medium, true);

                // Initialize fog of war
                map.InitializeFogOfWar(true);

                return map;
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading map: {ex.Message}");
                return null;
            }
        }

        // Create a simple test map
        public static GameMap CreateTestMap(Player player1, Player player2)
        {
            GameMap map = new GameMap("Test Map", 12, 10);
            map.Size = MapSize.Small; // Override with small map size for test map
            map.Players.Add(player1);
            map.Players.Add(player2);

            // Initialize with plains
            map.InitializeEmptyMap();

            // Add some forests
            map.SetTile(2, 2, TerrainType.Forest);
            map.SetTile(2, 3, TerrainType.Forest);
            map.SetTile(3, 2, TerrainType.Forest);
            map.SetTile(8, 6, TerrainType.Forest);
            map.SetTile(9, 6, TerrainType.Forest);
            map.SetTile(8, 7, TerrainType.Forest);

            // Add some mountains
            map.SetTile(5, 4, TerrainType.Mountain);
            map.SetTile(6, 4, TerrainType.Mountain);
            map.SetTile(5, 5, TerrainType.Mountain);

            // Add rivers
            map.SetTile(3, 5, TerrainType.River);
            map.SetTile(3, 6, TerrainType.River);
            map.SetTile(3, 7, TerrainType.River);
            map.SetTile(4, 7, TerrainType.River);
            map.SetTile(5, 7, TerrainType.River);
            map.SetTile(6, 7, TerrainType.River);
            map.SetTile(7, 7, TerrainType.River);
            map.SetTile(8, 7, TerrainType.River);

            // Add a bridge
            map.SetTile(6, 7, TerrainType.Bridge);

            // Add roads
            for (int x = 0; x < 12; x++)
            {
                map.SetTile(x, 1, TerrainType.Road);
            }

            // Add player bases
            // Player 1 (left side)
            map.SetTile(1, 1, TerrainType.HQ);
            map.Tiles[1, 1].Owner = player1;
            player1.Properties.Add(map.Tiles[1, 1]);

            map.SetTile(1, 3, TerrainType.Factory);
            map.Tiles[1, 3].Owner = player1;
            player1.Properties.Add(map.Tiles[1, 3]);

            map.SetTile(3, 1, TerrainType.City);
            map.Tiles[3, 1].Owner = player1;
            player1.Properties.Add(map.Tiles[3, 1]);

            // Player 2 (right side)
            map.SetTile(10, 1, TerrainType.HQ);
            map.Tiles[10, 1].Owner = player2;
            player2.Properties.Add(map.Tiles[10, 1]);

            map.SetTile(10, 3, TerrainType.Factory);
            map.Tiles[10, 3].Owner = player2;
            player2.Properties.Add(map.Tiles[10, 3]);

            map.SetTile(8, 1, TerrainType.City);
            map.Tiles[8, 1].Owner = player2;
            player2.Properties.Add(map.Tiles[8, 1]);

            // Neutral cities
            map.SetTile(5, 8, TerrainType.City);
            map.SetTile(6, 3, TerrainType.City);

            // Add some special terrain for air and naval units
            map.SetTile(4, 9, TerrainType.Airport);
            map.SetTile(7, 9, TerrainType.Seaport);

            // Add some water
            map.SetTile(9, 8, TerrainType.Sea);
            map.SetTile(9, 9, TerrainType.Sea);
            map.SetTile(10, 8, TerrainType.Sea);
            map.SetTile(10, 9, TerrainType.Sea);
            map.SetTile(11, 8, TerrainType.Sea);
            map.SetTile(11, 9, TerrainType.Sea);

            // Add beach (transition between land and sea)
            map.SetTile(8, 8, TerrainType.Beach);
            map.SetTile(8, 9, TerrainType.Beach);

            // Initialize weather
            map.InitializeWeather(MapSeason.Summer, GameDifficulty.Medium, true);

            // Initialize fog of war
            map.InitializeFogOfWar(true);

            return map;
        }

        private static TerrainType ParseTerrainCode(string code)
        {
            switch (code.Trim())
            {
                case "P": return TerrainType.Plain;
                case "F": return TerrainType.Forest;
                case "M": return TerrainType.Mountain;
                case "R": return TerrainType.Road;
                case "C": return TerrainType.City;
                case "B": return TerrainType.Factory;
                case "H": return TerrainType.HQ;
                case "S": return TerrainType.Sea;
                case "E": return TerrainType.Beach;
                case "V": return TerrainType.River;
                case "G": return TerrainType.Bridge;
                case "A": return TerrainType.Airport;
                case "T": return TerrainType.Seaport;
                default: return TerrainType.Plain;
            }
        }

        // Calculate the possible movement range for a unit accounting for weather
        public List<Tile> CalculateMovementRange(Unit unit, int x, int y, int movementPoints)
        {
            List<Tile> accessibleTiles = new List<Tile>();
            bool[,] visited = new bool[Width, Height];

            // Apply weather movement penalty
            float weatherPenalty = WeatherSystem?.GetMovementPenalty() ?? 1.0f;
            int adjustedMovementPoints = (int)(movementPoints / weatherPenalty);

            // Use breadth-first search to find all reachable tiles
            Queue<(int X, int Y, int RemainingMovement)> queue = new Queue<(int, int, int)>();
            queue.Enqueue((x, y, adjustedMovementPoints));
            visited[x, y] = true;
            accessibleTiles.Add(Tiles[x, y]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int currentX = current.X;
                int currentY = current.Y;
                int remainingMovement = current.RemainingMovement;

                // Try all four directions
                TryEnqueueTile(currentX + 1, currentY, remainingMovement, queue, visited, accessibleTiles, unit);
                TryEnqueueTile(currentX - 1, currentY, remainingMovement, queue, visited, accessibleTiles, unit);
                TryEnqueueTile(currentX, currentY + 1, remainingMovement, queue, visited, accessibleTiles, unit);
                TryEnqueueTile(currentX, currentY - 1, remainingMovement, queue, visited, accessibleTiles, unit);
            }

            return accessibleTiles;
        }

        private void TryEnqueueTile(int x, int y, int remainingMovement,
                                  Queue<(int, int, int)> queue,
                                  bool[,] visited,
                                  List<Tile> accessibleTiles,
                                  Unit unit)
        {
            // Check if coordinates are valid
            if (x < 0 || x >= Width || y < 0 || y >= Height || visited[x, y])
                return;

            // Check fog of war if applicable - can't move to unseen tiles
            if (FogOfWar != null && FogOfWar.Enabled && unit.Owner == CurrentPlayer)
            {
                VisibilityState visibility = FogOfWar.GetTileVisibility(x, y, unit.Owner);
                if (visibility == VisibilityState.Unseen)
                {
                    return;
                }
            }

            Tile tile = Tiles[x, y];

            // Check if unit can move on this terrain type
            bool canMove = true;

            // Check terrain restrictions based on unit movement type
            switch (unit.MovementType)
            {
                case MovementType.Infantry:
                    // Infantry can move on any land tile
                    canMove = tile.TerrainType != TerrainType.Sea;
                    break;

                case MovementType.Wheeled:
                    // Wheeled units can move on land but struggle on certain terrain
                    canMove = tile.TerrainType != TerrainType.Sea &&
                              tile.TerrainType != TerrainType.Mountain;
                    break;

                case MovementType.Treaded:
                    // Treaded units can move on most land tiles
                    canMove = tile.TerrainType != TerrainType.Sea;
                    break;

                case MovementType.Air:
                    // Air units can move anywhere
                    break;

                case MovementType.Ship:
                    // Ship units can only move on water
                    canMove = tile.TerrainType == TerrainType.Sea;
                    break;

                case MovementType.Lander:
                    // Landers can move on water and beaches
                    canMove = tile.TerrainType == TerrainType.Sea ||
                              tile.TerrainType == TerrainType.Beach;
                    break;
            }

            // Check if there's an enemy unit on the tile
            if (tile.OccupyingUnit != null && tile.OccupyingUnit.Owner != unit.Owner)
                canMove = false;

            // Calculate movement cost with potential weather penalty
            int baseCost = tile.MovementCost;

            // Apply additional penalties for certain terrain in bad weather
            if (WeatherSystem != null)
            {
                switch (WeatherSystem.CurrentWeather)
                {
                    case WeatherType.Rain:
                        // Rain makes forests and mountains harder to traverse
                        if (tile.TerrainType == TerrainType.Forest || tile.TerrainType == TerrainType.Mountain)
                            baseCost += 1;
                        break;
                    case WeatherType.Snow:
                        // Snow makes most terrain harder to traverse
                        if (tile.TerrainType != TerrainType.Road && tile.TerrainType != TerrainType.Bridge)
                            baseCost += 1;
                        break;
                    case WeatherType.Fog:
                        // Fog has minimal impact on movement cost
                        break;
                }
            }

            // Check if we have enough movement points left
            if (remainingMovement - baseCost < 0)
                canMove = false;

            // Check if unit has fuel to move
            if (!unit.HasFuelToMove())
                canMove = false;

            if (canMove)
            {
                queue.Enqueue((x, y, remainingMovement - baseCost));
                visited[x, y] = true;
                accessibleTiles.Add(tile);
            }
        }
    }
}