using System;
using System.Collections.Generic;

namespace WorldWarX.Models
{
    public enum VisibilityState
    {
        Unseen,      // Never seen before - completely black
        Previously,  // Seen before but not currently - foggy/darkened terrain only
        Visible      // Currently visible - full vision
    }

    public class FogOfWarSystem
    {
        private readonly GameMap _gameMap;
        private Dictionary<int, VisibilityState[,]> _playerVisibility = new Dictionary<int, VisibilityState[,]>();

        public bool Enabled { get; set; } = true;
        public bool UseAdvancedMode { get; set; } = true; // Added property to fix error

        public FogOfWarSystem(GameMap gameMap)
        {
            _gameMap = gameMap;
            Initialize();
        }

        public void Initialize()
        {
            foreach (Player player in _gameMap.Players)
            {
                _playerVisibility[player.PlayerId] = new VisibilityState[_gameMap.Width, _gameMap.Height];

                // Initially all tiles are unseen
                for (int x = 0; x < _gameMap.Width; x++)
                {
                    for (int y = 0; y < _gameMap.Height; y++)
                    {
                        _playerVisibility[player.PlayerId][x, y] = VisibilityState.Unseen;
                    }
                }
            }

            // Initial update to set visibility for starting positions
            UpdateVisibility();
        }

        public void UpdateVisibility()
        {
            if (!Enabled)
                return;

            foreach (Player player in _gameMap.Players)
            {
                // First, mark all currently visible tiles as previously seen
                for (int x = 0; x < _gameMap.Width; x++)
                {
                    for (int y = 0; y < _gameMap.Height; y++)
                    {
                        if (_playerVisibility[player.PlayerId][x, y] == VisibilityState.Visible)
                            _playerVisibility[player.PlayerId][x, y] = VisibilityState.Previously;
                    }
                }

                // Update visibility from units
                foreach (Unit unit in player.Units)
                {
                    UpdateVisibilityFromUnit(unit, player);
                }

                // Update visibility from properties
                foreach (Tile property in player.Properties)
                {
                    UpdateVisibilityFromProperty(property, player);
                }
            }
        }

        private void UpdateVisibilityFromUnit(Unit unit, Player player)
        {
            // Get the tile the unit is standing on
            Tile unitTile = _gameMap.Tiles[unit.X, unit.Y];

            // Calculate effective vision range based on unit type and terrain
            int visionRange = VisionSystem.CalculateEffectiveVisionRange(unit, unitTile.TerrainType);

            // Mark tiles within vision range as visible, accounting for terrain obstructions
            for (int x = Math.Max(0, unit.X - visionRange); x <= Math.Min(_gameMap.Width - 1, unit.X + visionRange); x++)
            {
                for (int y = Math.Max(0, unit.Y - visionRange); y <= Math.Min(_gameMap.Height - 1, unit.Y + visionRange); y++)
                {
                    // Use Manhattan distance for visibility calculation
                    int distance = Math.Abs(x - unit.X) + Math.Abs(y - unit.Y);

                    if (distance <= visionRange)
                    {
                        // Check for line-of-sight obstructions
                        if (HasLineOfSight(unit, unit.X, unit.Y, x, y))
                        {
                            _playerVisibility[player.PlayerId][x, y] = VisibilityState.Visible;
                        }
                    }
                }
            }
        }

        private bool HasLineOfSight(Unit viewer, int startX, int startY, int endX, int endY)
        {
            // Always visible if it's the same tile
            if (startX == endX && startY == endY)
                return true;

            // Simple line-of-sight check using Bresenham's line algorithm
            int dx = Math.Abs(endX - startX);
            int dy = Math.Abs(endY - startY);
            int sx = startX < endX ? 1 : -1;
            int sy = startY < endY ? 1 : -1;
            int err = dx - dy;

            int x = startX;
            int y = startY;
            int remainingVision = viewer.VisionRange;

            while (true)
            {
                // Break if we've reached the end point
                if (x == endX && y == endY)
                    break;

                // Calculate error
                int e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }

                // Skip the starting position
                if (x == startX && y == startY)
                    continue;

                // Check if this intermediate tile blocks vision
                if (x >= 0 && x < _gameMap.Width && y >= 0 && y < _gameMap.Height)
                {
                    TerrainType terrainType = _gameMap.Tiles[x, y].TerrainType;

                    // Get how much this terrain blocks vision
                    int blockage = VisionSystem.CalculateVisionBlockage(terrainType);

                    // Reduce remaining vision based on blockage
                    remainingVision -= blockage;

                    // If vision is exhausted by obstacles, there's no line of sight
                    if (remainingVision <= 0)
                        return false;
                }
            }

            return true;
        }

        private void UpdateVisibilityFromProperty(Tile property, Player player)
        {
            // Base vision range for different property types
            int baseVisionRange = GetPropertyVisionRange(property.TerrainType);

            // Mark tiles within vision range as visible
            for (int x = Math.Max(0, property.X - baseVisionRange); x <= Math.Min(_gameMap.Width - 1, property.X + baseVisionRange); x++)
            {
                for (int y = Math.Max(0, property.Y - baseVisionRange); y <= Math.Min(_gameMap.Height - 1, property.Y + baseVisionRange); y++)
                {
                    // Use Manhattan distance for visibility calculation
                    int distance = Math.Abs(x - property.X) + Math.Abs(y - property.Y);

                    if (distance <= baseVisionRange)
                    {
                        // Properties have more simple line of sight (basic check)
                        bool hasLineOfSight = CheckSimpleLineOfSight(property.X, property.Y, x, y);

                        if (hasLineOfSight)
                        {
                            _playerVisibility[player.PlayerId][x, y] = VisibilityState.Visible;
                        }
                    }
                }
            }
        }

        // Simpler line of sight check for properties
        private bool CheckSimpleLineOfSight(int startX, int startY, int endX, int endY)
        {
            // Always visible if it's the same tile
            if (startX == endX && startY == endY)
                return true;

            int dx = Math.Abs(endX - startX);
            int dy = Math.Abs(endY - startY);
            int sx = startX < endX ? 1 : -1;
            int sy = startY < endY ? 1 : -1;
            int err = dx - dy;

            int x = startX;
            int y = startY;
            int blockageCount = 0;

            while (true)
            {
                // Break if we've reached the end point
                if (x == endX && y == endY)
                    break;

                // Calculate error
                int e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }

                // Skip the starting position
                if (x == startX && y == startY)
                    continue;

                // Check if this intermediate tile blocks vision
                if (x >= 0 && x < _gameMap.Width && y >= 0 && y < _gameMap.Height)
                {
                    TerrainType terrainType = _gameMap.Tiles[x, y].TerrainType;

                    // Count heavily blocking terrain (mountains, forests)
                    if (terrainType == TerrainType.Mountain || terrainType == TerrainType.Forest)
                    {
                        blockageCount++;

                        // Allow seeing through a few blocking tiles for properties
                        if (blockageCount > 2)
                            return false;
                    }
                }
            }

            return true;
        }

        private int GetPropertyVisionRange(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.HQ:
                    return 12;
                case TerrainType.Factory:
                    return 10;
                case TerrainType.City:
                    return 8;
                case TerrainType.Airport:
                    return 15;
                case TerrainType.Seaport:
                    return 12;
                default:
                    return 5;
            }
        }

        public VisibilityState GetTileVisibility(int x, int y, Player player)
        {
            if (!Enabled)
                return VisibilityState.Visible;

            if (x < 0 || x >= _gameMap.Width || y < 0 || y >= _gameMap.Height)
                return VisibilityState.Unseen;

            return _playerVisibility[player.PlayerId][x, y];
        }

        public bool IsTileVisible(int x, int y, Player player)
        {
            return GetTileVisibility(x, y, player) == VisibilityState.Visible;
        }

        public bool IsTileRevealed(int x, int y, Player player)
        {
            VisibilityState state = GetTileVisibility(x, y, player);
            return state == VisibilityState.Visible || state == VisibilityState.Previously;
        }

        // Public method to check line of sight for debug purposes
        public bool HasLineOfSightPublic(Unit unit, int startX, int startY, int endX, int endY)
        {
            return HasLineOfSight(unit, startX, startY, endX, endY);
        }
    }
}