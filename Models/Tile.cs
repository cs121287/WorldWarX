using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WorldWarX.Models
{
    public class Tile
    {
        public TerrainType TerrainType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Unit OccupyingUnit { get; set; }

        // Terrain properties
        public int DefenseBonus { get; set; }
        public int MovementCost { get; set; }
        public bool Capturable { get; set; }
        public int IncomeValue { get; set; }

        // Owner (for buildings that can be owned)
        public Player Owner { get; set; }

        // Capture progress (0-100)
        public int CaptureProgress { get; set; }

        // Visual representation
        public ImageSource TileImage { get; set; }

        public Tile(TerrainType type, int x, int y)
        {
            TerrainType = type;
            X = x;
            Y = y;
            CaptureProgress = 0;

            // Set terrain properties based on type
            SetTerrainProperties();

            // Load appropriate image
            LoadImage();
        }

        private void SetTerrainProperties()
        {
            // Set terrain-specific properties
            switch (TerrainType)
            {
                case TerrainType.Plain:
                    DefenseBonus = 0;
                    MovementCost = 1;
                    Capturable = false;
                    IncomeValue = 0;
                    break;

                case TerrainType.Forest:
                    DefenseBonus = 20;
                    MovementCost = 2;
                    Capturable = false;
                    IncomeValue = 0;
                    break;

                case TerrainType.Mountain:
                    DefenseBonus = 40;
                    MovementCost = 3;
                    Capturable = false;
                    IncomeValue = 0;
                    break;

                case TerrainType.Road:
                    DefenseBonus = 0;
                    MovementCost = 1;
                    Capturable = false;
                    IncomeValue = 0;
                    break;

                case TerrainType.City:
                    DefenseBonus = 20;
                    MovementCost = 1;
                    Capturable = true;
                    IncomeValue = 100;
                    break;

                case TerrainType.Factory:
                    DefenseBonus = 20;
                    MovementCost = 1;
                    Capturable = true;
                    IncomeValue = 50;
                    break;

                case TerrainType.HQ:
                    DefenseBonus = 30;
                    MovementCost = 1;
                    Capturable = true;
                    IncomeValue = 150;
                    break;

                case TerrainType.Sea:
                    DefenseBonus = 0;
                    MovementCost = 1;
                    Capturable = false;
                    IncomeValue = 0;
                    break;

                case TerrainType.Beach:
                    DefenseBonus = 0;
                    MovementCost = 1;
                    Capturable = false;
                    IncomeValue = 0;
                    break;

                case TerrainType.River:
                    DefenseBonus = 0;
                    MovementCost = 3;
                    Capturable = false;
                    IncomeValue = 0;
                    break;

                case TerrainType.Bridge:
                    DefenseBonus = 0;
                    MovementCost = 1;
                    Capturable = false;
                    IncomeValue = 0;
                    break;

                case TerrainType.Airport:
                    DefenseBonus = 20;
                    MovementCost = 1;
                    Capturable = true;
                    IncomeValue = 100;
                    break;

                case TerrainType.Seaport:
                    DefenseBonus = 20;
                    MovementCost = 1;
                    Capturable = true;
                    IncomeValue = 100;
                    break;
            }
        }

        private void LoadImage()
        {
            try
            {
                string imageFileName;

                // Load the appropriate image based on terrain type
                switch (TerrainType)
                {
                    case TerrainType.Plain:
                        imageFileName = "plain";
                        break;
                    case TerrainType.Forest:
                        imageFileName = "forest";
                        break;
                    case TerrainType.Mountain:
                        imageFileName = "mountain";
                        break;
                    case TerrainType.Road:
                        imageFileName = "road";
                        break;
                    case TerrainType.City:
                        imageFileName = "city";
                        break;
                    case TerrainType.Factory:
                        imageFileName = "factory";
                        break;
                    case TerrainType.HQ:
                        imageFileName = "hq";
                        break;
                    case TerrainType.Sea:
                        imageFileName = "sea";
                        break;
                    case TerrainType.Beach:
                        imageFileName = "beach";
                        break;
                    case TerrainType.River:
                        imageFileName = "river";
                        break;
                    case TerrainType.Bridge:
                        imageFileName = "bridge";
                        break;
                    case TerrainType.Airport:
                        imageFileName = "airport";
                        break;
                    case TerrainType.Seaport:
                        imageFileName = "seaport";
                        break;
                    default:
                        imageFileName = "plain";
                        break;
                }

                string path = $"pack://application:,,,/Assets/Terrain/{imageFileName}.png";
                TileImage = new BitmapImage(new Uri(path));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load terrain image: {ex.Message}");
                // Use a default image if loading fails
                TileImage = new BitmapImage(new Uri("pack://application:,,,/Assets/Terrain/plain.png"));
            }
        }

        public bool AttemptCapture(Unit unit)
        {
            // Only infantry units can capture
            if (unit == null || unit.UnitType != UnitType.Infantry)
                return false;

            // Check if tile is capturable
            if (!Capturable)
                return false;

            // Add 20% progress per capture attempt
            CaptureProgress += 20;

            // Check if capture is complete
            if (CaptureProgress >= 100)
            {
                CaptureProgress = 100;
                Owner = unit.Owner;
                return true;
            }

            return false;
        }

        public void ResetCaptureProgress()
        {
            CaptureProgress = 0;
        }
    }
}