using System;
using System.Windows;
using System.Windows.Media;
using WorldWarX.Models;

namespace WorldWarX.Views
{
    public partial class CombatPreviewWindow : Window
    {
        private Unit _attacker;
        private Unit _defender;
        private Tile _defenderTile;
        private Tile _attackerTile;
        private int _projectedDamage;
        private int _projectedCounterDamage;
        private bool _defenderWillSurvive;
        private float _attackerEffectiveness;
        private float _defenderEffectiveness;

        public bool ConfirmAttack { get; private set; } = false;

        public CombatPreviewWindow(Unit attacker, Unit defender, Tile defenderTile, Tile attackerTile)
        {
            InitializeComponent();

            _attacker = attacker;
            _defender = defender;
            _defenderTile = defenderTile;
            _attackerTile = attackerTile;

            // Calculate projected damages and effectiveness
            CalculateProjectedDamages();

            // Set UI elements
            UpdateUI();
        }

        private void CalculateProjectedDamages()
        {
            // Calculate damage effectiveness between unit types
            _attackerEffectiveness = GetTypeMatchupMultiplier(_attacker.UnitType, _defender.UnitType);
            _defenderEffectiveness = GetTypeMatchupMultiplier(_defender.UnitType, _attacker.UnitType);

            // Calculate damage attacker will deal to defender
            _projectedDamage = _attacker.CalculateDamageAgainst(_defender, _defenderTile);

            // Check if defender will survive
            int defenderRemainingHealth = Math.Max(0, _defender.Health - _projectedDamage);
            _defenderWillSurvive = defenderRemainingHealth > 0;

            // Calculate counter-attack damage (if defender survives)
            if (_defenderWillSurvive)
            {
                // Only calculate counter-damage if defender is in range to counter-attack
                int distance = Math.Abs(_attacker.X - _defender.X) + Math.Abs(_attacker.Y - _defender.Y);
                if (distance <= _defender.AttackRange)
                {
                    // Calculate counter damage based on defender's reduced health
                    float healthPercentage = (float)defenderRemainingHealth / 100f;
                    _projectedCounterDamage = (int)(_defender.CalculateDamageAgainst(_attacker, _attackerTile) * healthPercentage);
                }
                else
                {
                    // Defender can't counter-attack (out of range)
                    _projectedCounterDamage = 0;
                }
            }
            else
            {
                // Defender will be destroyed, so no counter-attack
                _projectedCounterDamage = 0;
            }
        }

        private float GetTypeMatchupMultiplier(UnitType attackerType, UnitType defenderType)
        {
            // Define type advantages (e.g., infantry is weak against tanks)
            switch (attackerType)
            {
                case UnitType.Infantry:
                    if (defenderType == UnitType.Infantry) return 1.0f;
                    if (defenderType == UnitType.Mechanized) return 0.8f;
                    if (defenderType == UnitType.Tank) return 0.2f;
                    if (defenderType == UnitType.HeavyTank) return 0.1f;
                    break;

                case UnitType.Mechanized:
                    if (defenderType == UnitType.Infantry) return 1.2f;
                    if (defenderType == UnitType.Tank) return 0.5f;
                    if (defenderType == UnitType.HeavyTank) return 0.3f;
                    break;

                case UnitType.Tank:
                    if (defenderType == UnitType.Infantry) return 1.5f;
                    if (defenderType == UnitType.Mechanized) return 1.3f;
                    if (defenderType == UnitType.Tank) return 1.0f;
                    if (defenderType == UnitType.HeavyTank) return 0.7f;
                    if (defenderType == UnitType.Artillery) return 1.2f;
                    if (defenderType == UnitType.RocketLauncher) return 1.2f;
                    break;

                case UnitType.HeavyTank:
                    if (defenderType == UnitType.Infantry) return 1.8f;
                    if (defenderType == UnitType.Mechanized) return 1.5f;
                    if (defenderType == UnitType.Tank) return 1.3f;
                    if (defenderType == UnitType.HeavyTank) return 1.0f;
                    if (defenderType == UnitType.Artillery) return 1.4f;
                    if (defenderType == UnitType.RocketLauncher) return 1.4f;
                    break;

                case UnitType.Artillery:
                    if (defenderType == UnitType.Infantry) return 1.2f;
                    if (defenderType == UnitType.Mechanized) return 1.2f;
                    if (defenderType == UnitType.Tank) return 1.0f;
                    if (defenderType == UnitType.HeavyTank) return 0.8f;
                    break;

                case UnitType.RocketLauncher:
                    if (defenderType == UnitType.Infantry) return 1.3f;
                    if (defenderType == UnitType.Mechanized) return 1.3f;
                    if (defenderType == UnitType.Tank) return 1.1f;
                    if (defenderType == UnitType.HeavyTank) return 0.9f;
                    break;

                case UnitType.AntiAir:
                    if (defenderType == UnitType.Helicopter) return 2.0f;
                    if (defenderType == UnitType.TransportHelicopter) return 2.0f;
                    if (defenderType == UnitType.Fighter) return 0.8f;
                    if (defenderType == UnitType.Bomber) return 1.2f;
                    if (defenderType == UnitType.Stealth) return 0.6f;
                    break;

                case UnitType.Helicopter:
                    if (defenderType == UnitType.Infantry) return 1.2f;
                    if (defenderType == UnitType.Mechanized) return 1.2f;
                    if (defenderType == UnitType.Tank) return 1.0f;
                    if (defenderType == UnitType.HeavyTank) return 0.8f;
                    if (defenderType == UnitType.AntiAir) return 0.2f;
                    break;

                case UnitType.Fighter:
                    if (defenderType == UnitType.Helicopter) return 2.0f;
                    if (defenderType == UnitType.TransportHelicopter) return 2.0f;
                    if (defenderType == UnitType.Bomber) return 1.5f;
                    if (defenderType == UnitType.Fighter) return 1.0f;
                    if (defenderType == UnitType.Stealth) return 0.8f;
                    break;

                case UnitType.Bomber:
                    if (defenderType == UnitType.Tank) return 1.5f;
                    if (defenderType == UnitType.HeavyTank) return 1.3f;
                    if (defenderType == UnitType.Artillery) return 1.5f;
                    if (defenderType == UnitType.RocketLauncher) return 1.5f;
                    if (defenderType == UnitType.NavalTransport) return 1.5f;
                    if (defenderType == UnitType.Battleship) return 0.8f;
                    if (defenderType == UnitType.Cruiser) return 1.2f;
                    break;

                case UnitType.Stealth:
                    if (defenderType == UnitType.Fighter) return 1.2f;
                    if (defenderType == UnitType.Bomber) return 1.5f;
                    if (defenderType == UnitType.Stealth) return 1.0f;
                    if (defenderType == UnitType.AntiAir) return 1.3f;
                    break;

                case UnitType.Battleship:
                    if (defenderType == UnitType.Battleship) return 1.0f;
                    if (defenderType == UnitType.Cruiser) return 1.2f;
                    if (defenderType == UnitType.Submarine) return 0.5f;
                    break;

                case UnitType.Cruiser:
                    if (defenderType == UnitType.Submarine) return 1.8f;
                    if (defenderType == UnitType.Battleship) return 0.8f;
                    if (defenderType == UnitType.NavalTransport) return 1.3f;
                    break;

                case UnitType.Submarine:
                    if (defenderType == UnitType.Battleship) return 1.5f;
                    if (defenderType == UnitType.Cruiser) return 0.7f;
                    if (defenderType == UnitType.NavalTransport) return 1.5f;
                    if (defenderType == UnitType.Carrier) return 1.4f;
                    if (defenderType == UnitType.Submarine) return 1.0f;
                    break;

                case UnitType.Naval:
                    if (defenderType == UnitType.Naval) return 1.0f;
                    break;
            }

            // Default multiplier for no specific advantage/disadvantage
            return 1.0f;
        }

        private void UpdateUI()
        {
            // Attacker Info
            AttackerImage.Source = _attacker.UnitImage;
            AttackerNameText.Text = _attacker.Name;
            AttackerHealthText.Text = $"{_attacker.Health}/100";
            AttackerAttackText.Text = _attacker.AttackPower.ToString();
            AttackerEffectivenessText.Text = $"x{_attackerEffectiveness:0.0}";
            AttackerTerrainDefenseText.Text = $"{_attackerTile.DefenseBonus}%";

            // Calculate attacker's projected health
            int attackerRemainingHealth = Math.Max(0, _attacker.Health - _projectedCounterDamage);

            // Attacker current health bar
            double attackerHealthPercent = _attacker.Health / 100.0;
            AttackerHealthBar.Width = 100 * attackerHealthPercent;
            AttackerHealthBar.Fill = GetHealthColor(_attacker.Health);

            // Attacker projected health bar
            double attackerRemainingPercent = attackerRemainingHealth / 100.0;
            AttackerProjectedHealthBar.Width = 100 * attackerRemainingPercent;
            AttackerProjectedHealthBar.Fill = GetHealthColor(attackerRemainingHealth);

            // Attacker health lost indicator
            double attackerHealthLostPercent = (_attacker.Health - attackerRemainingHealth) / 100.0;
            AttackerHealthLostBar.Width = 100 * attackerHealthLostPercent;
            AttackerHealthLostBar.Margin = new Thickness(100 * attackerRemainingPercent, 0, 0, 0);

            // Update attacker health text
            AttackerProjectedHealthText.Text = $"{attackerRemainingHealth}/100";

            // Defender Info
            DefenderImage.Source = _defender.UnitImage;
            DefenderNameText.Text = _defender.Name;
            DefenderHealthText.Text = $"{_defender.Health}/100";
            DefenderDefenseText.Text = _defender.Defense.ToString();
            DefenderEffectivenessText.Text = $"x{_defenderEffectiveness:0.0}";
            DefenderTerrainDefenseText.Text = $"{_defenderTile.DefenseBonus}%";

            // Calculate defender's projected health
            int defenderRemainingHealth = Math.Max(0, _defender.Health - _projectedDamage);

            // Defender current health bar
            double defenderHealthPercent = _defender.Health / 100.0;
            DefenderHealthBar.Width = 100 * defenderHealthPercent;
            DefenderHealthBar.Fill = GetHealthColor(_defender.Health);

            // Defender projected health bar
            double defenderRemainingPercent = defenderRemainingHealth / 100.0;
            DefenderProjectedHealthBar.Width = 100 * defenderRemainingPercent;
            DefenderProjectedHealthBar.Fill = GetHealthColor(defenderRemainingHealth);

            // Defender health lost indicator
            double defenderHealthLostPercent = (_defender.Health - defenderRemainingHealth) / 100.0;
            DefenderHealthLostBar.Width = 100 * defenderHealthLostPercent;
            DefenderHealthLostBar.Margin = new Thickness(100 * defenderRemainingPercent, 0, 0, 0);

            // Update defender health text
            DefenderProjectedHealthText.Text = $"{defenderRemainingHealth}/100";

            // If defender will be destroyed, indicate that
            if (defenderRemainingHealth == 0)
            {
                DefenderProjectedHealthText.Text = "0/100 (Defeated)";
                DefenderProjectedHealthText.Foreground = Brushes.Red;

                // No counter attack if defeated
                DefenderEffectivenessText.Text = "x0.0 (Defeated)";
            }

            // If attacker will be destroyed by counter, indicate that
            if (attackerRemainingHealth == 0)
            {
                AttackerProjectedHealthText.Text = "0/100 (Defeated)";
                AttackerProjectedHealthText.Foreground = Brushes.Red;
            }

            // Update counter-attack info based on range
            int distance = Math.Abs(_attacker.X - _defender.X) + Math.Abs(_attacker.Y - _defender.Y);
            if (distance > _defender.AttackRange && _defenderWillSurvive)
            {
                DefenderEffectivenessText.Text = "x0.0 (Out of Range)";
            }

            // Disable attack button if this attack would do 0 damage
            BtnAttack.IsEnabled = _projectedDamage > 0;

            // Add fuel information to the display
            if (_attacker.GetFuelPercentage() < 0.3f)
            {
                AttackerAttackText.Text += $" (Low Fuel: {_attacker.GetFuelStatusText()})";
            }

            if (_defender.GetFuelPercentage() < 0.3f)
            {
                DefenderDefenseText.Text += $" (Low Fuel: {_defender.GetFuelStatusText()})";
            }
        }

        private SolidColorBrush GetHealthColor(int health)
        {
            if (health > 60)
                return new SolidColorBrush(Colors.LimeGreen);
            else if (health > 30)
                return new SolidColorBrush(Colors.Yellow);
            else
                return new SolidColorBrush(Colors.Red);
        }

        private void BtnAttack_Click(object sender, RoutedEventArgs e)
        {
            ConfirmAttack = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ConfirmAttack = false;
            Close();
        }
    }
}