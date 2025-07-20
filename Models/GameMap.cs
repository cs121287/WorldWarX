using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WorldWarX.Models
{
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
        
        public GameMap(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
            Players = new List<Player>();
            Season = MapSeason.Summer; // Default to summer
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
        public Tile GetTile(int x, int y)
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
            }
        }
        
        // Initialize weather system
        public void InitializeWeather(MapSeason season, GameDifficulty difficulty, bool weatherEffectsEnabled, int? seed = null)
        {
            Season = season;
            WeatherSystem = new WeatherSystem(season, difficulty, weatherEffectsEnabled, seed);
        }
        
        // Load a map from a file
        public static GameMap LoadFromFile(string filePath)
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
                
                // Initialize weather with default settings
                map.InitializeWeather(MapSeason.Summer, GameDifficulty.Medium, true);
                
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