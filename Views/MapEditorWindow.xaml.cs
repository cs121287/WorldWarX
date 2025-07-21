using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using WorldWarX.Models;

namespace WorldWarX.Views
{
    public partial class MapEditorWindow : Window
    {
        private GameMap _editingMap;
        private TerrainType _selectedTerrain = TerrainType.Plain;
        private const int TILE_SIZE = 32;
        private Rectangle[,] _tileRects;
        private bool _propertyAssignmentMode = false;
        private int _selectedOwnerIndex = 0; // 0 = Unassigned, 1 = Player 1, 2 = Player 2
        private string _previewImagePath = "";

        public MapEditorWindow()
        {
            InitializeComponent();
            InitializeNewMap(20, 15); // Default size
            TerrainListBox.SelectedIndex = 0;
            OwnerComboBox.SelectedIndex = 0;

            WidthTextBox.Text = _editingMap.Width.ToString();
            HeightTextBox.Text = _editingMap.Height.ToString();

            MapNameTextBox.Text = _editingMap.Name;
            MapAuthorTextBox.Text = Environment.UserName;
            MapDescriptionTextBox.Text = _editingMap.Description ?? "";
            UpdateDimensionsText();
            UpdateOwnerModeText();
        }

        private void InitializeNewMap(int width, int height)
        {
            _editingMap = new GameMap("New Map", width, height);
            _editingMap.InitializeEmptyMap();
            DrawMapGrid();
            UpdateDimensionsText();
        }

        private void DrawMapGrid()
        {
            MapCanvas.Children.Clear();
            MapCanvas.Width = _editingMap.Width * TILE_SIZE;
            MapCanvas.Height = _editingMap.Height * TILE_SIZE;

            _tileRects = new Rectangle[_editingMap.Width, _editingMap.Height];

            for (int x = 0; x < _editingMap.Width; x++)
            {
                for (int y = 0; y < _editingMap.Height; y++)
                {
                    var tile = _editingMap.Tiles[x, y];
                    Rectangle rect = new Rectangle
                    {
                        Width = TILE_SIZE,
                        Height = TILE_SIZE,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 1,
                        Fill = GetTerrainBrush(tile.TerrainType)
                    };
                    Canvas.SetLeft(rect, x * TILE_SIZE);
                    Canvas.SetTop(rect, y * TILE_SIZE);

                    rect.Tag = new Tuple<int, int>(x, y);
                    rect.MouseLeftButtonDown += TileRect_MouseLeftButtonDown;

                    // Highlight if owned property
                    if (tile.Owner != null)
                    {
                        rect.Stroke = tile.Owner.PlayerId == 0 ? Brushes.Red : Brushes.Blue;
                        rect.StrokeThickness = 3;
                    }

                    MapCanvas.Children.Add(rect);
                    _tileRects[x, y] = rect;
                }
            }
        }

        private Brush GetTerrainBrush(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Plain: return Brushes.LightGreen;
                case TerrainType.Forest: return Brushes.ForestGreen;
                case TerrainType.Mountain: return Brushes.DimGray;
                case TerrainType.Road: return Brushes.SandyBrown;
                case TerrainType.River: return Brushes.Aqua;
                case TerrainType.Bridge: return Brushes.Gold;
                case TerrainType.City: return Brushes.LightSlateGray;
                case TerrainType.Factory: return Brushes.LightSteelBlue;
                case TerrainType.HQ: return Brushes.OrangeRed;
                case TerrainType.Sea: return Brushes.Blue;
                case TerrainType.Beach: return Brushes.Khaki;
                case TerrainType.Airport: return Brushes.MediumPurple;
                case TerrainType.Seaport: return Brushes.DarkBlue;
                default: return Brushes.White;
            }
        }

        private void TileRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rect && rect.Tag is Tuple<int, int> pos)
            {
                int x = pos.Item1;
                int y = pos.Item2;
                var tile = _editingMap.Tiles[x, y];

                if (_propertyAssignmentMode)
                {
                    if (IsPropertyTerrain(tile.TerrainType))
                    {
                        AssignBuildingOwner(tile, _selectedOwnerIndex);
                        UpdateTileVisual(rect, x, y);
                    }
                }
                else
                {
                    _editingMap.SetTile(x, y, _selectedTerrain);
                    UpdateTileVisual(rect, x, y);

                    // Remove ownership if terrain is not a property
                    if (!IsPropertyTerrain(_selectedTerrain))
                    {
                        _editingMap.Tiles[x, y].Owner = null;
                    }
                }
            }
        }

        private void UpdateTileVisual(Rectangle rect, int x, int y)
        {
            var tile = _editingMap.Tiles[x, y];
            rect.Fill = GetTerrainBrush(tile.TerrainType);

            // Visual feedback for ownership
            if (tile.Owner != null)
            {
                rect.Stroke = tile.Owner.PlayerId == 0 ? Brushes.Red : Brushes.Blue;
                rect.StrokeThickness = 3;
            }
            else
            {
                rect.Stroke = Brushes.Gray;
                rect.StrokeThickness = 1;
            }
        }

        private void TerrainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TerrainListBox.SelectedItem is ListBoxItem item)
            {
                Enum.TryParse(item.Content.ToString(), out TerrainType terrain);
                _selectedTerrain = terrain;
            }
        }

        private void PropertyAssignmentCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _propertyAssignmentMode = true;
            OwnerComboBox.IsEnabled = true;
            UpdateOwnerModeText();
        }

        private void PropertyAssignmentCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _propertyAssignmentMode = false;
            OwnerComboBox.IsEnabled = false;
            UpdateOwnerModeText();
        }

        private void OwnerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedOwnerIndex = OwnerComboBox.SelectedIndex;
            UpdateOwnerModeText();
        }

        private bool IsPropertyTerrain(TerrainType terrain)
        {
            return terrain == TerrainType.HQ || terrain == TerrainType.Factory ||
                   terrain == TerrainType.City || terrain == TerrainType.Airport ||
                   terrain == TerrainType.Seaport;
        }

        // Assign building owner: 0 = Unassigned, 1 = Player 1, 2 = Player 2
        private void AssignBuildingOwner(Tile tile, int ownerIndex)
        {
            if (ownerIndex == 0)
            {
                tile.Owner = null; // Unassigned
            }
            else
            {
                // Ensure players exist
                while (_editingMap.Players.Count < ownerIndex)
                {
                    _editingMap.Players.Add(new Player(_editingMap.Players.Count));
                }
                tile.Owner = _editingMap.Players[ownerIndex - 1];
                if (!tile.Owner.Properties.Contains(tile))
                    tile.Owner.Properties.Add(tile);
            }
        }

        private void BtnResizeMap_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WidthTextBox.Text, out int w) && int.TryParse(HeightTextBox.Text, out int h)
                && w > 0 && h > 0 && w <= 320 && h <= 400)
            {
                InitializeNewMap(w, h);
            }
            else
            {
                MessageBox.Show("Please enter valid positive integers within max size (Width ≤ 320, Height ≤ 400).");
            }
        }

        private void BtnSaveMap_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "WorldWarX Map Files (*.wwxmap)|*.wwxmap|All Files (*.*)|*.*",
                DefaultExt = "wwxmap"
            };
            if (saveDialog.ShowDialog() == true)
            {
                _editingMap.Name = MapNameTextBox.Text;
                _editingMap.Author = MapAuthorTextBox.Text;
                _editingMap.Description = MapDescriptionTextBox.Text;
                _editingMap.PreviewImagePath = _previewImagePath;
                _editingMap.SaveToFile(saveDialog.FileName);
                MessageBox.Show("Map saved successfully.");
            }
        }

        private void BtnLoadMap_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "WorldWarX Map Files (*.wwxmap)|*.wwxmap|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() == true)
            {
                var loadedMap = GameMap.LoadFromFile(openDialog.FileName);
                if (loadedMap != null)
                {
                    _editingMap = loadedMap;
                    DrawMapGrid();
                    MapNameTextBox.Text = _editingMap.Name;
                    MapAuthorTextBox.Text = _editingMap.Author;
                    MapDescriptionTextBox.Text = _editingMap.Description;
                    _previewImagePath = _editingMap.PreviewImagePath;
                    if (!string.IsNullOrEmpty(_previewImagePath) && File.Exists(_previewImagePath))
                    {
                        MapPreviewImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_previewImagePath));
                    }
                    else
                    {
                        MapPreviewImage.Source = null;
                    }
                    WidthTextBox.Text = _editingMap.Width.ToString();
                    HeightTextBox.Text = _editingMap.Height.ToString();
                    UpdateDimensionsText();
                }
                else
                {
                    MessageBox.Show("Failed to load map.");
                }
            }
        }

        private void BtnChoosePreviewImage_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp|All Files (*.*)|*.*"
            };
            if (openDialog.ShowDialog() == true)
            {
                _previewImagePath = openDialog.FileName;
                MapPreviewImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_previewImagePath));
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(MapCanvas);
            int x = (int)(point.X / TILE_SIZE);
            int y = (int)(point.Y / TILE_SIZE);
            if (x >= 0 && x < _editingMap.Width && y >= 0 && y < _editingMap.Height)
            {
                var tile = _editingMap.Tiles[x, y];
                if (_propertyAssignmentMode)
                {
                    if (IsPropertyTerrain(tile.TerrainType))
                    {
                        AssignBuildingOwner(tile, _selectedOwnerIndex);
                        UpdateTileVisual(_tileRects[x, y], x, y);
                    }
                }
                else
                {
                    _editingMap.SetTile(x, y, _selectedTerrain);
                    UpdateTileVisual(_tileRects[x, y], x, y);
                    if (!IsPropertyTerrain(_selectedTerrain))
                        _editingMap.Tiles[x, y].Owner = null;
                }
            }
        }

        private void UpdateDimensionsText()
        {
            DimensionsTextBlock.Text = $"Width: {_editingMap.Width}, Height: {_editingMap.Height}";
        }

        private void UpdateOwnerModeText()
        {
            string ownerStr = (OwnerComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Unassigned";
            OwnerModeTextBlock.Text = _propertyAssignmentMode ? $"Property Assignment: {ownerStr}" : "Terrain Editing";
        }
    }
}