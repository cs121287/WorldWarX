using System;
using System.Collections.Generic;

namespace WorldWarX.Models
{
    /// <summary>
    /// Handles the vision system mechanics for the game, including unit-terrain interactions
    /// </summary>
    public static class VisionSystem
    {
        // Vision modifiers based on unit's movement type and the terrain it's standing on
        public static readonly Dictionary<MovementType, Dictionary<TerrainType, int>> UnitTerrainVisionModifiers = new Dictionary<MovementType, Dictionary<TerrainType, int>>
        {
            // Infantry units (on foot)
            { MovementType.Infantry, new Dictionary<TerrainType, int> 
                {
                    { TerrainType.Plain, 0 },
                    { TerrainType.Forest, -2 },       // Dense trees reduce vision
                    { TerrainType.Mountain, 4 },      // Good vantage point
                    { TerrainType.Road, 1 },
                    { TerrainType.City, 2 },          // Urban vantage points
                    { TerrainType.Factory, 1 },
                    { TerrainType.Airport, 2 },
                    { TerrainType.Seaport, 2 },
                    { TerrainType.HQ, 3 },
                    { TerrainType.Sea, -5 },          // Very poor visibility on water
                    { TerrainType.Beach, 2 },         // Good view from coastline
                    { TerrainType.River, -2 },
                    { TerrainType.Bridge, 3 }         // Elevated position
                }
            },
            
            // Wheeled vehicles
            { MovementType.Wheeled, new Dictionary<TerrainType, int> 
                {
                    { TerrainType.Plain, 1 },
                    { TerrainType.Forest, -3 },       // Very poor in forests
                    { TerrainType.Mountain, -2 },     // Difficult to position well
                    { TerrainType.Road, 2 },          // Better positioning on roads
                    { TerrainType.City, 1 },
                    { TerrainType.Factory, 1 },
                    { TerrainType.Airport, 2 },
                    { TerrainType.Seaport, 2 },
                    { TerrainType.HQ, 2 },
                    { TerrainType.Sea, -8 },          // Cannot operate on water
                    { TerrainType.Beach, 1 },
                    { TerrainType.River, -3 },
                    { TerrainType.Bridge, 2 }
                }
            },
            
            // Treaded vehicles (tanks)
            { MovementType.Treaded, new Dictionary<TerrainType, int> 
                {
                    { TerrainType.Plain, 1 },
                    { TerrainType.Forest, -2 },
                    { TerrainType.Mountain, -1 },     // Better than wheeled on rough terrain
                    { TerrainType.Road, 1 },
                    { TerrainType.City, 0 },
                    { TerrainType.Factory, 0 },
                    { TerrainType.Airport, 1 },
                    { TerrainType.Seaport, 1 },
                    { TerrainType.HQ, 1 },
                    { TerrainType.Sea, -8 },          // Cannot operate on water
                    { TerrainType.Beach, 0 },
                    { TerrainType.River, -4 },
                    { TerrainType.Bridge, 2 }
                }
            },
            
            // Air units
            { MovementType.Air, new Dictionary<TerrainType, int> 
                {
                    { TerrainType.Plain, 2 },         // Good visibility from air
                    { TerrainType.Forest, 1 },        // Can see over forests somewhat
                    { TerrainType.Mountain, -2 },     // Mountains can obstruct vision
                    { TerrainType.Road, 2 },
                    { TerrainType.City, 1 },
                    { TerrainType.Factory, 1 },
                    { TerrainType.Airport, 4 },       // Excellent visibility around airports
                    { TerrainType.Seaport, 2 },
                    { TerrainType.HQ, 2 },
                    { TerrainType.Sea, 3 },           // Great visibility over open water
                    { TerrainType.Beach, 2 },
                    { TerrainType.River, 1 },
                    { TerrainType.Bridge, 2 }
                }
            },
            
            // Naval vessels
            { MovementType.Ship, new Dictionary<TerrainType, int> 
                {
                    { TerrainType.Plain, -5 },        // Cannot see well inland
                    { TerrainType.Forest, -6 },
                    { TerrainType.Mountain, -3 },     // Can see mountains from distance
                    { TerrainType.Road, -5 },
                    { TerrainType.City, -4 },
                    { TerrainType.Factory, -4 },
                    { TerrainType.Airport, -4 },
                    { TerrainType.Seaport, 4 },       // Good visibility at ports
                    { TerrainType.HQ, -4 },
                    { TerrainType.Sea, 5 },           // Excellent on open water
                    { TerrainType.Beach, 2 },         // Good along coasts
                    { TerrainType.River, -2 },
                    { TerrainType.Bridge, -3 }
                }
            },
            
            // Landers (amphibious)
            { MovementType.Lander, new Dictionary<TerrainType, int> 
                {
                    { TerrainType.Plain, -3 },
                    { TerrainType.Forest, -4 },
                    { TerrainType.Mountain, -2 },
                    { TerrainType.Road, -3 },
                    { TerrainType.City, -3 },
                    { TerrainType.Factory, -3 },
                    { TerrainType.Airport, -2 },
                    { TerrainType.Seaport, 3 },
                    { TerrainType.HQ, -3 },
                    { TerrainType.Sea, 3 },          // Good on water
                    { TerrainType.Beach, 4 },        // Best on beaches (their specialty)
                    { TerrainType.River, 2 },
                    { TerrainType.Bridge, -2 }
                }
            }
        };
        
        // Terrain "opacity" - how much each terrain type blocks vision through it
        public static readonly Dictionary<TerrainType, int> TerrainVisionBlockers = new Dictionary<TerrainType, int>
        {
            { TerrainType.Plain, 0 },          // Open terrain doesn't block vision
            { TerrainType.Forest, 4 },         // Forests block vision significantly
            { TerrainType.Mountain, 5 },       // Mountains block vision heavily
            { TerrainType.Road, 0 },
            { TerrainType.City, 2 },           // Cities block some vision
            { TerrainType.Factory, 3 },        // Factories block vision
            { TerrainType.Airport, 1 },
            { TerrainType.Seaport, 1 },
            { TerrainType.HQ, 2 },
            { TerrainType.Sea, 0 },            // Open water doesn't block vision
            { TerrainType.Beach, 0 },
            { TerrainType.River, 0 },
            { TerrainType.Bridge, 1 }
        };
        
        // Calculate effective vision range for a unit on a specific terrain
        public static int CalculateEffectiveVisionRange(Unit unit, TerrainType terrainType)
        {
            // Start with the unit's base vision range
            int effectiveRange = unit.VisionRange;
            
            // Apply terrain modifier for the unit's movement type if available
            if (UnitTerrainVisionModifiers.TryGetValue(unit.MovementType, out var terrainModifiers))
            {
                if (terrainModifiers.TryGetValue(terrainType, out int modifier))
                {
                    effectiveRange += modifier;
                }
            }
            
            // Ensure vision range doesn't go below 1
            return Math.Max(1, effectiveRange);
        }
        
        // Calculate how much a line of sight is blocked between two points
        public static int CalculateVisionBlockage(TerrainType terrainType)
        {
            if (TerrainVisionBlockers.TryGetValue(terrainType, out int blockage))
            {
                return blockage;
            }
            return 0; // Default: no blockage
        }
    }
}