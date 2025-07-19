using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WorldWarX.Models;

namespace WorldWarX.Views
{
    public partial class GameControl : UserControl
    {
        // Core game data
        private GameMode _gameMode;
        private Country _playerCountry;
        private Country _opponentCountry;
        private Map _map;
        private GameMap _gameMap;
        private Player _currentPlayer;
        private Player _humanPlayer;
        private Player _aiPlayer;
        private int _currentTurn = 1;

        // UI elements
        private Rectangle[,] _tileRects;
        private Image[,] _tileImages;
        private Image[,] _unitImages;
        private const int TILE_SIZE = 40;

        // Game state
        private Unit _selectedUnit;
        private Tile _selectedTile;
        private List<Tile> _movementRange;
        private List<Tile> _attackRange;
        private bool _isMovingUnit;
        private bool _isAttacking;

        // Events for navigation
        public event EventHandler BackToMainMenuRequested;

        public GameControl(GameMode gameMode, Country playerCountry)
        {
            InitializeComponent();

            _gameMode = gameMode;
            _playerCountry = playerCountry;

            SetupGame();
        }

        public GameControl(GameMode gameMode, Country playerCountry, Country opponentCountry, Map map)
        {
            InitializeComponent();

            _gameMode = gameMode;
            _playerCountry = playerCountry;
            _opponentCountry = opponentCountry;
            _map = map;

            SetupGame();
        }

        private void SetupGame()
        {
            // Initialize players
            _humanPlayer = new Player(0) { Name = "Player", IsAI = false };
            _humanPlayer.SetCountry(_playerCountry);

            _aiPlayer = new Player(1) { Name = "Enemy", IsAI = true };
            if (_opponentCountry != null)
                _aiPlayer.SetCountry(_opponentCountry);

            // Create test map
            _gameMap = GameMap.CreateTestMap(_humanPlayer, _aiPlayer);

            // Set current player to human
            _currentPlayer = _humanPlayer;

            // Initialize UI
            InitializeGameUI();

            // Add some initial units for testing
            AddInitialUnits();

            // Update UI
            UpdateGameInfo();
        }

        private void InitializeGameUI()
        {
            // Clear canvas
            GameCanvas.Children.Clear();

            // Set canvas size
            GameCanvas.Width = _gameMap.Width * TILE_SIZE;
            GameCanvas.Height = _gameMap.Height * TILE_SIZE;

            // Initialize tile arrays
            _tileRects = new Rectangle[_gameMap.Width, _gameMap.Height];
            _tileImages = new Image[_gameMap.Width, _gameMap.Height];
            _unitImages = new Image[_gameMap.Width, _gameMap.Height];

            // Create tiles
            for (int x = 0; x < _gameMap.Width; x++)
            {
                for (int y = 0; y < _gameMap.Height; y++)
                {
                    // Create tile background
                    Rectangle tileRect = new Rectangle
                    {
                        Width = TILE_SIZE,
                        Height = TILE_SIZE,
                        Fill = new SolidColorBrush(Colors.Transparent),
                        Stroke = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                        StrokeThickness = 1
                    };

                    Canvas.SetLeft(tileRect, x * TILE_SIZE);
                    Canvas.SetTop(tileRect, y * TILE_SIZE);
                    Canvas.SetZIndex(tileRect, 0);

                    GameCanvas.Children.Add(tileRect);
                    _tileRects[x, y] = tileRect;

                    // Create tile image
                    Image tileImage = new Image
                    {
                        Width = TILE_SIZE,
                        Height = TILE_SIZE,
                        Source = _gameMap.Tiles[x, y].TileImage
                    };

                    Canvas.SetLeft(tileImage, x * TILE_SIZE);
                    Canvas.SetTop(tileImage, y * TILE_SIZE);
                    Canvas.SetZIndex(tileImage, 1);

                    GameCanvas.Children.Add(tileImage);
                    _tileImages[x, y] = tileImage;

                    // Add click handler for tile
                    int tileX = x;
                    int tileY = y;
                    tileRect.MouseDown += (s, e) => TileClicked(tileX, tileY);
                }
            }

            // Disable unit action buttons initially
            BtnAttack.IsEnabled = false;
            BtnCapture.IsEnabled = false;
            BtnWait.IsEnabled = false;
            BtnLoad.IsEnabled = false;
            BtnUnload.IsEnabled = false;
            BtnResupply.IsEnabled = false;

            // Hide unit info panel
            SelectedUnitPanel.Visibility = Visibility.Collapsed;
            NoSelectionText.Visibility = Visibility.Visible;
        }

        private void AddInitialUnits()
        {
            // Add some units for the human player
            AddUnit(new Unit(UnitType.Infantry, _humanPlayer) { X = 1, Y = 2 });
            AddUnit(new Unit(UnitType.Tank, _humanPlayer) { X = 2, Y = 1 });
            AddUnit(new Unit(UnitType.TransportVehicle, _humanPlayer) { X = 1, Y = 4 });

            // Add some units for the AI player
            AddUnit(new Unit(UnitType.Infantry, _aiPlayer) { X = 10, Y = 2 });
            AddUnit(new Unit(UnitType.Tank, _aiPlayer) { X = 9, Y = 1 });
        }

        private void AddUnit(Unit unit)
        {
            // Add unit to player's unit list
            unit.Owner.AddUnit(unit);

            // Place unit on tile
            _gameMap.Tiles[unit.X, unit.Y].OccupyingUnit = unit;

            // Create unit image
            Image unitImage = new Image
            {
                Width = TILE_SIZE,
                Height = TILE_SIZE,
                Source = unit.UnitImage
            };

            Canvas.SetLeft(unitImage, unit.X * TILE_SIZE);
            Canvas.SetTop(unitImage, unit.Y * TILE_SIZE);
            Canvas.SetZIndex(unitImage, 2);

            GameCanvas.Children.Add(unitImage);
            _unitImages[unit.X, unit.Y] = unitImage;
        }

        private void UpdateGameInfo()
        {
            // Update player info
            PlayerNameText.Text = $"Player: {_playerCountry.Name}";
            FundsText.Text = $"{_humanPlayer.Funds}G";
            PowerMeterBar.Value = _humanPlayer.PowerMeter;

            // Update turn info
            TurnInfoText.Text = $"Turn {_currentTurn} - {_currentPlayer.Name}'s Turn";

            // Enable/disable power button
            BtnUsePower.IsEnabled = _currentPlayer == _humanPlayer && _humanPlayer.PowerMeter >= 100 && !_humanPlayer.IsPowerActive;
        }

        private void TileClicked(int x, int y)
        {
            // Get tile that was clicked
            Tile clickedTile = _gameMap.Tiles[x, y];

            // Update terrain info
            UpdateTerrainInfo(clickedTile);

            // If we're in movement mode, try to move the unit
            if (_isMovingUnit && _selectedUnit != null && _movementRange != null && _movementRange.Contains(clickedTile))
            {
                MoveUnit(_selectedUnit, x, y);
                _isMovingUnit = false;
                ClearHighlights();
                return;
            }

            // If we're in attack mode, try to attack
            if (_isAttacking && _selectedUnit != null && _attackRange != null && _attackRange.Contains(clickedTile))
            {
                AttackTile(_selectedUnit, clickedTile);
                _isAttacking = false;
                ClearHighlights();
                return;
            }

            // Otherwise, select the unit on this tile (if any)
            Unit unitOnTile = clickedTile.OccupyingUnit;

            // Only allow selection of current player's units
            if (unitOnTile != null && unitOnTile.Owner == _currentPlayer && !unitOnTile.HasMoved)
            {
                SelectUnit(unitOnTile);
            }
            else
            {
                // Deselect if clicking elsewhere
                ClearSelection();
            }
        }

        private void SelectUnit(Unit unit)
        {
            // Clear any previous selection
            ClearSelection();

            // Set selected unit
            _selectedUnit = unit;
            _selectedTile = _gameMap.Tiles[unit.X, unit.Y];

            // Update UI
            UpdateUnitInfo(unit);
            HighlightTile(unit.X, unit.Y, Colors.Yellow);

            // Show movement range
            ShowMovementRange(unit);

            // Enable action buttons
            BtnWait.IsEnabled = true;

            // Enable attack button if there are enemies in range
            bool canAttack = CheckForTargetsInRange(unit);
            BtnAttack.IsEnabled = canAttack && unit.AttackPower > 0;

            // Enable capture button if unit is infantry on capturable tile
            bool canCapture = unit.UnitType == UnitType.Infantry &&
                             _selectedTile.Capturable &&
                             (_selectedTile.Owner == null || _selectedTile.Owner != unit.Owner);
            BtnCapture.IsEnabled = canCapture;

            // Enable transport-related buttons
            BtnLoad.IsEnabled = unit.CanTransport && CheckForLoadableUnits(unit);
            BtnUnload.IsEnabled = unit.CanTransport && unit.TransportedUnits.Count > 0;

            // Enable resupply button if unit's fuel is below maximum
            BtnResupply.IsEnabled = unit.GetFuelPercentage() < 1.0f;

            // Enter movement selection mode
            _isMovingUnit = true;
        }

        private void ShowMovementRange(Unit unit)
        {
            // Calculate movement range
            _movementRange = _gameMap.CalculateMovementRange(unit, unit.X, unit.Y, unit.MovementRange);

            // Highlight tiles in range
            foreach (Tile tile in _movementRange)
            {
                // Skip the unit's current tile
                if (tile.X == unit.X && tile.Y == unit.Y)
                    continue;

                // Skip tiles with friendly units
                if (tile.OccupyingUnit != null && tile.OccupyingUnit.Owner == unit.Owner)
                    continue;

                // Highlight with semi-transparent blue
                HighlightTile(tile.X, tile.Y, Color.FromArgb(100, 0, 100, 255));
            }
        }

        private bool CheckForTargetsInRange(Unit unit)
        {
            // Calculate attack range from current position
            _attackRange = new List<Tile>();

            for (int x = unit.X - unit.AttackRange; x <= unit.X + unit.AttackRange; x++)
            {
                for (int y = unit.Y - unit.AttackRange; y <= unit.Y + unit.AttackRange; y++)
                {
                    // Skip if out of bounds
                    if (x < 0 || x >= _gameMap.Width || y < 0 || y >= _gameMap.Height)
                        continue;

                    // Check if within Manhattan distance (not diagonal)
                    int distance = Math.Abs(x - unit.X) + Math.Abs(y - unit.Y);
                    if (distance <= unit.AttackRange)
                    {
                        Tile tile = _gameMap.Tiles[x, y];
                        _attackRange.Add(tile);

                        // Check if there's an enemy unit on this tile
                        if (tile.OccupyingUnit != null && tile.OccupyingUnit.Owner != unit.Owner)
                            return true;
                    }
                }
            }

            return false;
        }

        private void MoveUnit(Unit unit, int newX, int newY)
        {
            // Don't move if destination is the same as current position
            if (unit.X == newX && unit.Y == newY)
                return;

            // Check if unit has fuel to move
            if (!unit.HasFuelToMove())
            {
                MessageBox.Show($"{unit.Name} is out of fuel and cannot move.");
                return;
            }

            // Get destination tile
            Tile destTile = _gameMap.Tiles[newX, newY];

            // Check if destination has an enemy unit
            if (destTile.OccupyingUnit != null)
            {
                // Can't move onto enemy units
                if (destTile.OccupyingUnit.Owner != unit.Owner)
                    return;
            }

            // Calculate path distance (this is simplified - ideally you'd use the actual path distance)
            int distance = Math.Abs(newX - unit.X) + Math.Abs(newY - unit.Y);

            // Consume fuel for movement
            ConsumeFuelForMovement(unit, distance);

            // Update map
            _gameMap.Tiles[unit.X, unit.Y].OccupyingUnit = null;
            _gameMap.Tiles[newX, newY].OccupyingUnit = unit;

            // Update unit position
            int oldX = unit.X;
            int oldY = unit.Y;
            unit.X = newX;
            unit.Y = newY;

            // Update UI
            _unitImages[oldX, oldY] = null;
            _unitImages[newX, newY] = _unitImages[oldX, oldY];

            Canvas.SetLeft(_unitImages[newX, newY], newX * TILE_SIZE);
            Canvas.SetTop(_unitImages[newX, newY], newY * TILE_SIZE);

            // Mark unit as moved
            unit.HasMoved = true;

            // Reset capture progress if unit was capturing
            if (unit.IsCapturing)
            {
                unit.IsCapturing = false;
                _gameMap.Tiles[oldX, oldY].ResetCaptureProgress();
            }

            // Check if we can attack from new position
            bool canAttack = CheckForTargetsInRange(unit);
            BtnAttack.IsEnabled = canAttack && unit.AttackPower > 0;

            // Check if unit can capture from new position
            bool canCapture = unit.UnitType == UnitType.Infantry &&
                             destTile.Capturable &&
                             (destTile.Owner == null || destTile.Owner != unit.Owner);
            BtnCapture.IsEnabled = canCapture;

            // Update selected tile
            _selectedTile = destTile;

            // Update terrain info
            UpdateTerrainInfo(destTile);

            // Update unit info to show it has moved
            UpdateUnitInfo(unit);

            // Add power charge for movement
            unit.Owner.AddPowerCharge(1);
            UpdateGameInfo();
        }

        private void AttackTile(Unit attacker, Tile targetTile)
        {
            // Check if there's a unit to attack
            Unit defender = targetTile.OccupyingUnit;
            if (defender == null || defender.Owner == attacker.Owner)
                return;

            // Get the attacker's tile for terrain defense calculation
            Tile attackerTile = _gameMap.Tiles[attacker.X, attacker.Y];

            // Show combat preview window
            CombatPreviewWindow previewWindow = new CombatPreviewWindow(attacker, defender, targetTile, attackerTile);
            previewWindow.ShowDialog();

            // If player cancels the attack, return without attacking
            if (!previewWindow.ConfirmAttack)
                return;

            // Calculate damage
            int damage = attacker.CalculateDamageAgainst(defender, targetTile);

            // Apply damage to defender
            bool destroyed = defender.TakeDamage(damage);

            // If defender is not destroyed and can counter-attack, apply counter-attack damage
            if (!destroyed)
            {
                // Check if defender is in range to counter-attack
                int distance = Math.Abs(attacker.X - defender.X) + Math.Abs(attacker.Y - defender.Y);
                if (distance <= defender.AttackRange)
                {
                    // Calculate counter damage
                    int counterDamage = defender.CalculateDamageAgainst(attacker, attackerTile);

                    // Apply counter damage
                    bool attackerDestroyed = attacker.TakeDamage(counterDamage);

                    if (attackerDestroyed)
                    {
                        MessageBox.Show($"{attacker.Name} attacks for {damage} damage but is destroyed by {defender.Name}'s counter-attack!");
                        RemoveUnit(attacker);

                        // Add power charge for destroying an enemy
                        defender.Owner.AddPowerCharge(5);

                        // Check for victory condition
                        CheckVictoryCondition();
                        return;
                    }
                    else
                    {
                        MessageBox.Show($"{attacker.Name} attacks for {damage} damage! {defender.Name} counter-attacks for {counterDamage} damage!");
                    }
                }
                else
                {
                    MessageBox.Show($"{attacker.Name} attacks {defender.Name} for {damage} damage!");
                }
            }
            else
            {
                MessageBox.Show($"{attacker.Name} attacks and destroys {defender.Name}!");
                RemoveUnit(defender);

                // Add power charge for destroying an enemy
                attacker.Owner.AddPowerCharge(5);

                // Check for victory condition
                CheckVictoryCondition();
            }

            // Mark attacker as having attacked
            attacker.HasAttacked = true;

            // Update unit info
            UpdateUnitInfo(attacker);

            // Update game info
            UpdateGameInfo();
        }

        private void RemoveUnit(Unit unit)
        {
            // Remove unit from map
            _gameMap.Tiles[unit.X, unit.Y].OccupyingUnit = null;

            // Remove unit from player's list
            unit.Owner.RemoveUnit(unit);

            // Remove unit from UI
            if (_unitImages[unit.X, unit.Y] != null)
            {
                GameCanvas.Children.Remove(_unitImages[unit.X, unit.Y]);
                _unitImages[unit.X, unit.Y] = null;
            }
        }

        private void CheckVictoryCondition()
        {
            // Check if either player has lost all units
            if (_humanPlayer.Units.Count == 0)
            {
                MessageBox.Show("Game Over! You have been defeated!", "Defeat", MessageBoxButton.OK, MessageBoxImage.Information);
                BackToMainMenuRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (_aiPlayer.Units.Count == 0)
            {
                MessageBox.Show("Victory! You have defeated the enemy!", "Victory", MessageBoxButton.OK, MessageBoxImage.Information);
                BackToMainMenuRequested?.Invoke(this, EventArgs.Empty);
            }

            // Check if HQ has been captured
            foreach (Player player in _gameMap.Players)
            {
                bool hasHQ = false;
                foreach (Tile tile in player.Properties)
                {
                    if (tile.TerrainType == TerrainType.HQ && tile.Owner == player)
                    {
                        hasHQ = true;
                        break;
                    }
                }

                if (!hasHQ)
                {
                    if (player == _humanPlayer)
                    {
                        MessageBox.Show("Game Over! Your HQ has been captured!", "Defeat", MessageBoxButton.OK, MessageBoxImage.Information);
                        BackToMainMenuRequested?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        MessageBox.Show("Victory! You have captured the enemy HQ!", "Victory", MessageBoxButton.OK, MessageBoxImage.Information);
                        BackToMainMenuRequested?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void UpdateUnitInfo(Unit unit)
        {
            if (unit == null)
            {
                SelectedUnitPanel.Visibility = Visibility.Collapsed;
                NoSelectionText.Visibility = Visibility.Visible;
                return;
            }

            // Show unit panel
            SelectedUnitPanel.Visibility = Visibility.Visible;
            NoSelectionText.Visibility = Visibility.Collapsed;

            // Update unit info
            UnitImage.Source = unit.UnitImage;
            UnitNameText.Text = unit.Name;
            UnitHealthText.Text = $"{unit.Health}/100";
            UnitMovementText.Text = unit.MovementRange.ToString();
            UnitAttackText.Text = unit.AttackPower.ToString();
            UnitDefenseText.Text = unit.Defense.ToString();

            // Add fuel info
            UnitFuelText.Text = unit.GetFuelStatusText();

            // Add transported units info if applicable
            if (unit.CanTransport && unit.TransportedUnits.Count > 0)
            {
                TransportedUnitsPanel.Visibility = Visibility.Visible;
                TransportedUnitsText.Text = string.Join(", ", unit.TransportedUnits.Select(u => u.Name));
            }
            else
            {
                TransportedUnitsPanel.Visibility = Visibility.Collapsed;
            }

            // Update status
            if (unit.HasMoved && unit.HasAttacked)
                UnitStatusText.Text = "Done";
            else if (unit.HasMoved)
                UnitStatusText.Text = "Moved";
            else if (unit.IsCapturing)
                UnitStatusText.Text = "Capturing";
            else
                UnitStatusText.Text = "Ready";

            // Update button states
            BtnAttack.IsEnabled = !unit.HasAttacked && unit.AttackPower > 0;
            BtnCapture.IsEnabled = !unit.IsCapturing && unit.UnitType == UnitType.Infantry &&
                                  _selectedTile.Capturable &&
                                  (_selectedTile.Owner == null || _selectedTile.Owner != unit.Owner);
            BtnWait.IsEnabled = true;

            // Enable/disable transport-related buttons
            BtnLoad.IsEnabled = !unit.HasMoved && unit.CanTransport && CheckForLoadableUnits(unit);
            BtnUnload.IsEnabled = !unit.HasMoved && unit.CanTransport && unit.TransportedUnits.Count > 0;
            BtnResupply.IsEnabled = !unit.HasMoved && !unit.HasAttacked && unit.GetFuelPercentage() < 1.0f;
        }

        private void UpdateTerrainInfo(Tile tile)
        {
            if (tile == null)
                return;

            TerrainNameText.Text = tile.TerrainType.ToString();
            TerrainDefenseText.Text = $"{tile.DefenseBonus}%";
            TerrainMovementText.Text = tile.MovementCost.ToString();

            // Show owner if tile is capturable
            if (tile.Capturable)
            {
                TerrainOwnerPanel.Visibility = Visibility.Visible;
                TerrainOwnerText.Text = tile.Owner?.Name ?? "None";
            }
            else
            {
                TerrainOwnerPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void HighlightTile(int x, int y, Color color)
        {
            if (_tileRects[x, y] != null)
            {
                _tileRects[x, y].Fill = new SolidColorBrush(color);
            }
        }

        private void ClearHighlights()
        {
            for (int x = 0; x < _gameMap.Width; x++)
            {
                for (int y = 0; y < _gameMap.Height; y++)
                {
                    if (_tileRects[x, y] != null)
                    {
                        _tileRects[x, y].Fill = new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
        }

        private void ClearSelection()
        {
            _selectedUnit = null;
            _selectedTile = null;
            _movementRange = null;
            _attackRange = null;
            _isMovingUnit = false;
            _isAttacking = false;

            ClearHighlights();
            UpdateUnitInfo(null);

            // Disable action buttons
            BtnAttack.IsEnabled = false;
            BtnCapture.IsEnabled = false;
            BtnWait.IsEnabled = false;
            BtnLoad.IsEnabled = false;
            BtnUnload.IsEnabled = false;
            BtnResupply.IsEnabled = false;
        }

        private void BtnAttack_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnit == null || _selectedUnit.HasAttacked || _selectedUnit.AttackPower <= 0)
                return;

            // Enter attack mode
            _isAttacking = true;
            _isMovingUnit = false;

            // Clear previous highlights
            ClearHighlights();

            // Highlight unit position
            HighlightTile(_selectedUnit.X, _selectedUnit.Y, Colors.Yellow);

            // Highlight attackable tiles
            foreach (Tile tile in _attackRange)
            {
                // Only highlight tiles with enemy units
                if (tile.OccupyingUnit != null && tile.OccupyingUnit.Owner != _selectedUnit.Owner)
                {
                    HighlightTile(tile.X, tile.Y, Color.FromArgb(180, 255, 0, 0));
                }
            }

            // Show message to guide the player
            MessageBox.Show("Select an enemy unit to attack.", "Attack Mode", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnit == null || _selectedUnit.UnitType != UnitType.Infantry)
                return;

            // Start capturing
            _selectedUnit.IsCapturing = true;

            // Attempt to capture
            bool captured = _selectedTile.AttemptCapture(_selectedUnit);

            if (captured)
            {
                MessageBox.Show($"{_selectedTile.TerrainType} captured!");

                // Add to player's properties
                _selectedUnit.Owner.ClaimProperty(_selectedTile);

                // If previous owner exists, remove from their properties
                if (_selectedTile.Owner != null && _selectedTile.Owner != _selectedUnit.Owner)
                {
                    _selectedTile.Owner.LoseProperty(_selectedTile);
                }

                // Check if HQ was captured (victory condition)
                if (_selectedTile.TerrainType == TerrainType.HQ)
                {
                    CheckVictoryCondition();
                }

                // Add power charge for capturing
                _selectedUnit.Owner.AddPowerCharge(10);
            }
            else
            {
                MessageBox.Show($"Started capturing {_selectedTile.TerrainType}. Capturing in progress...");
            }

            // Mark unit as done
            _selectedUnit.HasMoved = true;
            _selectedUnit.HasAttacked = true;

            // Update UI
            UpdateUnitInfo(_selectedUnit);
            UpdateTerrainInfo(_selectedTile);
            UpdateGameInfo();

            ClearHighlights();
            HighlightTile(_selectedUnit.X, _selectedUnit.Y, Colors.Gray);
        }

        private void BtnWait_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnit == null)
                return;

            // Mark unit as done
            _selectedUnit.HasMoved = true;
            _selectedUnit.HasAttacked = true;

            // Update UI
            UpdateUnitInfo(_selectedUnit);

            ClearHighlights();
            HighlightTile(_selectedUnit.X, _selectedUnit.Y, Colors.Gray);
        }

        private void BtnEndTurn_Click(object sender, RoutedEventArgs e)
        {
            // End current player's turn
            if (_currentPlayer == _humanPlayer)
            {
                // Switch to AI player
                _currentPlayer = _aiPlayer;

                // Update UI
                UpdateGameInfo();

                // Clear selection
                ClearSelection();

                // Run AI turn
                ExecuteAITurn();
            }
            else
            {
                // Switch back to human player
                _currentPlayer = _humanPlayer;

                // Increment turn counter
                _currentTurn++;

                // Start new turn
                StartNewTurn();

                // Update UI
                UpdateGameInfo();
            }
        }

        private void StartNewTurn()
        {
            // Reset all units
            _currentPlayer.StartNewTurn();

            // Collect income
            _currentPlayer.CollectIncome();

            // Update UI
            UpdateGameInfo();

            // Clear selection
            ClearSelection();
        }

        private void ExecuteAITurn()
        {
            // Simple AI for demonstration purposes
            foreach (Unit unit in _aiPlayer.Units)
            {
                // Reset unit for new turn
                unit.ResetForNewTurn();

                // TODO: Implement AI logic
                // For now, just mark all units as used
                unit.HasMoved = true;
                unit.HasAttacked = true;
            }

            // End AI turn after a short delay
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                BtnEndTurn_Click(null, null);
            };
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void BtnUsePower_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlayer != _humanPlayer || _humanPlayer.PowerMeter < 100 || _humanPlayer.IsPowerActive)
                return;

            // Activate power
            bool activated = _humanPlayer.TryActivatePower();

            if (activated)
            {
                MessageBox.Show($"Power '{_playerCountry.PowerName}' activated!\n{_playerCountry.PowerDescription}");

                // Apply power effects
                ApplyCountryPowerEffects();

                // Update UI
                UpdateGameInfo();
            }
        }

        private void ApplyCountryPowerEffects()
        {
            // Apply power effects based on country
            // These would implement the specific bonuses each country provides
            switch (_playerCountry.Name)
            {
                case "Redonia": // Ground unit attack power bonus
                    foreach (Unit unit in _humanPlayer.Units)
                    {
                        if (unit.UnitType == UnitType.Tank || unit.UnitType == UnitType.Artillery ||
                            unit.UnitType == UnitType.HeavyTank || unit.UnitType == UnitType.RocketLauncher)
                        {
                            unit.AttackPower = (int)(unit.AttackPower * 1.2f); // 20% attack boost
                        }
                    }
                    break;

                case "Azurea": // Naval movement bonus
                    foreach (Unit unit in _humanPlayer.Units)
                    {
                        if (unit.UnitType == UnitType.Battleship || unit.UnitType == UnitType.Cruiser ||
                            unit.UnitType == UnitType.Submarine || unit.UnitType == UnitType.NavalTransport ||
                            unit.UnitType == UnitType.Carrier)
                        {
                            unit.MovementRange += 2;
                            unit.AttackPower = (int)(unit.AttackPower * 1.1f); // 10% attack boost
                        }
                    }
                    break;

                case "Verdania": // Economy and defense bonus
                    _humanPlayer.Funds += 1000;
                    foreach (Unit unit in _humanPlayer.Units)
                    {
                        unit.Defense = (int)(unit.Defense * 1.1f); // 10% defense boost
                    }
                    break;

                case "Solaris": // Air unit attack bonus
                    foreach (Unit unit in _humanPlayer.Units)
                    {
                        if (unit.UnitType == UnitType.Helicopter || unit.UnitType == UnitType.Bomber ||
                            unit.UnitType == UnitType.Fighter || unit.UnitType == UnitType.Stealth ||
                            unit.UnitType == UnitType.TransportHelicopter)
                        {
                            unit.AttackPower = (int)(unit.AttackPower * 1.3f); // 30% attack boost
                        }
                    }
                    break;
            }
        }

        private void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            // Check if there's a factory available
            List<Tile> factories = new List<Tile>();
            foreach (Tile tile in _humanPlayer.Properties)
            {
                if (tile.TerrainType == TerrainType.Factory && tile.OccupyingUnit == null)
                {
                    factories.Add(tile);
                }
            }

            if (factories.Count == 0)
            {
                MessageBox.Show("No available factories to build units. Factories must be empty.",
                               "No Available Factories", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // If multiple factories, let player choose one (future enhancement)
            // For now, just use the first available factory
            Tile selectedFactory = factories[0];

            // Open unit production window
            UnitProductionWindow productionWindow = new UnitProductionWindow(_humanPlayer, selectedFactory);
            bool? result = productionWindow.ShowDialog();

            if (result == true && productionWindow.ProducedUnit != null)
            {
                // Place the newly produced unit on the map
                Unit newUnit = productionWindow.ProducedUnit;
                AddUnit(newUnit);

                // Update UI
                UpdateGameInfo();
            }
        }

        private void BtnSurrender_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to surrender?", "Surrender", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Game Over! You have surrendered.", "Defeat", MessageBoxButton.OK, MessageBoxImage.Information);
                BackToMainMenuRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            // Show game menu
            MessageBoxResult result = MessageBox.Show("What would you like to do?", "Game Menu", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                // Return to main menu
                BackToMainMenuRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to quit the game? All progress will be lost.",
                                                     "Quit Game",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                BackToMainMenuRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        // Method to handle fuel consumption when moving
        private void ConsumeFuelForMovement(Unit unit, int distance)
        {
            // Different terrain types can affect fuel consumption
            int fuelCost = distance;

            // Reduce fuel
            unit.ResupplyFuel(-fuelCost);

            // If fuel is critically low, display warning
            if (unit.GetFuelPercentage() < 0.2f)
            {
                MessageBox.Show($"Warning: {unit.Name} is low on fuel ({unit.GetFuelStatusText()})!");
            }
        }

        // Method to handle loading a unit into a transport
        private void LoadUnitIntoTransport(Unit unitToLoad, Unit transportUnit)
        {
            if (!transportUnit.CanTransport)
            {
                MessageBox.Show("This unit cannot transport other units.");
                return;
            }

            if (transportUnit.TransportedUnits.Count >= transportUnit.TransportCapacity)
            {
                MessageBox.Show($"This {transportUnit.Name} is already at full capacity.");
                return;
            }

            if (!transportUnit.TransportableUnitTypes.Contains(unitToLoad.UnitType))
            {
                MessageBox.Show($"This {transportUnit.Name} cannot transport {unitToLoad.Name} units.");
                return;
            }

            // Load the unit
            bool success = transportUnit.LoadUnit(unitToLoad);
            if (success)
            {
                // Remove the loaded unit from the map
                _gameMap.Tiles[unitToLoad.X, unitToLoad.Y].OccupyingUnit = null;

                // Remove the unit's visual representation
                GameCanvas.Children.Remove(_unitImages[unitToLoad.X, unitToLoad.Y]);
                _unitImages[unitToLoad.X, unitToLoad.Y] = null;

                // Update the UI
                UpdateUnitInfo(transportUnit);

                MessageBox.Show($"{unitToLoad.Name} loaded into {transportUnit.Name}.");
            }
        }

        // Method to handle unloading units from a transport
        private void UnloadUnitFromTransport(Unit transportUnit, int unitIndex, int x, int y)
        {
            if (!transportUnit.CanTransport || transportUnit.TransportedUnits.Count == 0)
            {
                MessageBox.Show("No units to unload.");
                return;
            }

            // Check if the location is valid for unloading
            if (!transportUnit.CanUnloadAt(x, y, _gameMap))
            {
                MessageBox.Show("Cannot unload at this location.");
                return;
            }

            // Unload the unit
            Unit unloadedUnit = transportUnit.UnloadUnit(unitIndex, x, y);
            if (unloadedUnit != null)
            {
                // Place the unloaded unit on the map
                _gameMap.Tiles[x, y].OccupyingUnit = unloadedUnit;

                // Create visual representation for the unit
                Image unitImage = new Image
                {
                    Width = TILE_SIZE,
                    Height = TILE_SIZE,
                    Source = unloadedUnit.UnitImage
                };

                Canvas.SetLeft(unitImage, x * TILE_SIZE);
                Canvas.SetTop(unitImage, y * TILE_SIZE);
                Canvas.SetZIndex(unitImage, 2);

                GameCanvas.Children.Add(unitImage);
                _unitImages[x, y] = unitImage;

                // Update the UI
                UpdateUnitInfo(transportUnit);

                MessageBox.Show($"{unloadedUnit.Name} unloaded from {transportUnit.Name}.");
            }
        }

        // Method to handle resupplying a unit's fuel
        private void ResupplyUnit(Unit unit)
        {
            // Check if the unit is on a supply structure (HQ, Airport, Seaport, etc.)
            Tile tile = _gameMap.Tiles[unit.X, unit.Y];

            bool canResupply = false;

            // Check for supply structures
            if ((tile.TerrainType == TerrainType.HQ ||
                tile.TerrainType == TerrainType.Factory ||
                tile.TerrainType == TerrainType.Airport ||
                tile.TerrainType == TerrainType.Seaport) &&
                tile.Owner == unit.Owner)
            {
                canResupply = true;
            }

            // Check for adjacent supply units
            if (!canResupply)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int nx = unit.X + dx;
                        int ny = unit.Y + dy;

                        // Check bounds
                        if (nx < 0 || nx >= _gameMap.Width || ny < 0 || ny >= _gameMap.Height)
                            continue;

                        Tile adjacentTile = _gameMap.Tiles[nx, ny];
                        if (adjacentTile.OccupyingUnit != null &&
                            adjacentTile.OccupyingUnit.Owner == unit.Owner &&
                            adjacentTile.OccupyingUnit.CanResupply)
                        {
                            canResupply = true;
                            break;
                        }
                    }
                    if (canResupply) break;
                }
            }

            if (canResupply)
            {
                unit.FullResupply();
                MessageBox.Show($"{unit.Name} has been fully resupplied with fuel.");
                UpdateUnitInfo(unit);
            }
            else
            {
                MessageBox.Show("Unit must be on a friendly supply structure or adjacent to a supply unit to resupply.");
            }
        }

        // Button handlers for the new functionality
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnit == null || _selectedUnit.HasMoved || !_selectedUnit.CanTransport)
                return;

            // Find adjacent units that can be loaded
            List<Unit> loadableUnits = GetLoadableUnitsForTransport(_selectedUnit);

            if (loadableUnits.Count == 0)
            {
                MessageBox.Show("No valid units nearby to load.");
                return;
            }

            // For simplicity, just load the first loadable unit
            // In a more advanced implementation, you'd show a selection dialog
            Unit unitToLoad = loadableUnits[0];

            LoadUnitIntoTransport(unitToLoad, _selectedUnit);
        }

        private void BtnUnload_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnit == null || _selectedUnit.HasMoved || !_selectedUnit.CanTransport || _selectedUnit.TransportedUnits.Count == 0)
                return;

            // Find adjacent tiles where units can be unloaded
            List<System.Windows.Point> unloadPositions = new List<System.Windows.Point>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = _selectedUnit.X + dx;
                    int ny = _selectedUnit.Y + dy;

                    if (_selectedUnit.CanUnloadAt(nx, ny, _gameMap))
                    {
                        unloadPositions.Add(new System.Windows.Point(nx, ny));
                    }
                }
            }

            if (unloadPositions.Count == 0)
            {
                MessageBox.Show("No valid positions to unload units.");
                return;
            }

            // For simplicity, just unload the first unit to the first valid position
            // In a more advanced implementation, you'd show a selection dialog
            System.Windows.Point unloadPos = unloadPositions[0];

            UnloadUnitFromTransport(_selectedUnit, 0, (int)unloadPos.X, (int)unloadPos.Y);
        }

        private void BtnResupply_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUnit == null || _selectedUnit.HasMoved || _selectedUnit.HasAttacked)
                return;

            ResupplyUnit(_selectedUnit);
        }

        private List<Unit> GetLoadableUnitsForTransport(Unit transportUnit)
        {
            List<Unit> loadableUnits = new List<Unit>();

            // Check if transport is at capacity
            if (transportUnit.TransportedUnits.Count >= transportUnit.TransportCapacity)
                return loadableUnits;

            // Check adjacent tiles for units that can be loaded
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = transportUnit.X + dx;
                    int ny = transportUnit.Y + dy;

                    // Check bounds
                    if (nx < 0 || nx >= _gameMap.Width || ny < 0 || ny >= _gameMap.Height)
                        continue;

                    Tile adjacentTile = _gameMap.Tiles[nx, ny];
                    Unit adjacentUnit = adjacentTile.OccupyingUnit;

                    if (adjacentUnit != null &&
                        adjacentUnit.Owner == transportUnit.Owner &&
                        transportUnit.TransportableUnitTypes.Contains(adjacentUnit.UnitType))
                    {
                        loadableUnits.Add(adjacentUnit);
                    }
                }
            }

            return loadableUnits;
        }

        private bool CheckForLoadableUnits(Unit transportUnit)
        {
            return GetLoadableUnitsForTransport(transportUnit).Count > 0;
        }
    }
}