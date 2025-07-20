using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WorldWarX.Models
{
    public enum WeatherType
    {
        Clear,      // Good visibility, normal movement
        Rain,       // Reduced visibility, slowed movement (Summer only)
        Snow,       // Greatly reduced visibility, greatly slowed movement (Winter only)
        Fog         // Very low visibility, slightly slowed movement (Both seasons)
    }

    public enum MapSeason
    {
        Summer,
        Winter
    }

    public class WeatherSystem
    {
        private readonly Dictionary<int, WeatherType> _weatherSchedule;
        private int _currentDay = 1;
        private readonly MapSeason _mapSeason;
        private readonly Random _random;
        private readonly GameDifficulty _difficulty;
        private readonly bool _weatherEffectsEnabled;

        public WeatherType CurrentWeather => _weatherSchedule[_currentDay];
        public WeatherType NextDayWeather => _weatherSchedule[_currentDay + 1];
        public MapSeason Season => _mapSeason;
        
        // Movement penalty multipliers for each weather type (1.0 = no penalty)
        public static readonly Dictionary<WeatherType, float> MovementPenalties = new Dictionary<WeatherType, float>
        {
            { WeatherType.Clear, 1.0f },
            { WeatherType.Rain, 1.2f },  // 20% slower movement
            { WeatherType.Snow, 1.5f },  // 50% slower movement
            { WeatherType.Fog, 1.1f }    // 10% slower movement
        };

        // Combat accuracy penalties for each weather type (1.0 = no penalty)
        public static readonly Dictionary<WeatherType, float> AccuracyPenalties = new Dictionary<WeatherType, float>
        {
            { WeatherType.Clear, 1.0f },
            { WeatherType.Rain, 0.9f },  // 10% accuracy reduction
            { WeatherType.Snow, 0.8f },  // 20% accuracy reduction
            { WeatherType.Fog, 0.7f }    // 30% accuracy reduction
        };

        // Vision range penalties for each weather type (1.0 = no penalty)
        public static readonly Dictionary<WeatherType, float> VisionPenalties = new Dictionary<WeatherType, float>
        {
            { WeatherType.Clear, 1.0f },
            { WeatherType.Rain, 0.8f },  // 20% vision reduction
            { WeatherType.Snow, 0.7f },  // 30% vision reduction
            { WeatherType.Fog, 0.5f }    // 50% vision reduction
        };

        public WeatherSystem(MapSeason season, GameDifficulty difficulty, bool weatherEffectsEnabled, int? seed = null)
        {
            _mapSeason = season;
            _difficulty = difficulty;
            _weatherEffectsEnabled = weatherEffectsEnabled;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            _weatherSchedule = GenerateWeatherSchedule(150);
        }

        private Dictionary<int, WeatherType> GenerateWeatherSchedule(int days)
        {
            var schedule = new Dictionary<int, WeatherType>();
            
            // If weather effects are disabled, all days are clear
            if (!_weatherEffectsEnabled)
            {
                for (int day = 1; day <= days; day++)
                {
                    schedule[day] = WeatherType.Clear;
                }
                return schedule;
            }

            // Weather probability distributions based on map season and difficulty
            Dictionary<WeatherType, int> weatherProbabilities = GetWeatherProbabilities();
            
            // Generate initial weather
            WeatherType currentWeather = GetRandomWeather(weatherProbabilities);
            schedule[1] = currentWeather;

            // Generate remaining days with some persistence (weather tends to stay similar for a few days)
            for (int day = 2; day <= days; day++)
            {
                // 70% chance weather stays the same, 30% chance it changes
                if (_random.Next(100) < 70)
                {
                    schedule[day] = schedule[day - 1];
                }
                else
                {
                    schedule[day] = GetRandomWeather(weatherProbabilities);
                }
            }

            return schedule;
        }

        private Dictionary<WeatherType, int> GetWeatherProbabilities()
        {
            // Base probabilities
            var probabilities = new Dictionary<WeatherType, int>();
            
            // Adjust probabilities based on season
            if (_mapSeason == MapSeason.Summer)
            {
                probabilities[WeatherType.Clear] = 50;
                probabilities[WeatherType.Rain] = 30;
                probabilities[WeatherType.Fog] = 20;
                // No snow in summer
            }
            else // Winter
            {
                probabilities[WeatherType.Clear] = 40;
                probabilities[WeatherType.Snow] = 40;
                probabilities[WeatherType.Fog] = 20;
                // No rain in winter
            }

            // Adjust based on difficulty
            AdjustProbabilitiesForDifficulty(probabilities);

            return probabilities;
        }

        private void AdjustProbabilitiesForDifficulty(Dictionary<WeatherType, int> probabilities)
        {
            switch (_difficulty)
            {
                case GameDifficulty.Easy:
                    // More clear weather on Easy
                    IncreaseWeatherProbability(probabilities, WeatherType.Clear, 20);
                    break;
                    
                case GameDifficulty.Medium:
                    // Default probabilities
                    break;
                    
                case GameDifficulty.Hard:
                    // More severe weather on Hard
                    if (_mapSeason == MapSeason.Summer)
                    {
                        IncreaseWeatherProbability(probabilities, WeatherType.Rain, 10);
                        IncreaseWeatherProbability(probabilities, WeatherType.Fog, 10);
                    }
                    else // Winter
                    {
                        IncreaseWeatherProbability(probabilities, WeatherType.Snow, 10);
                        IncreaseWeatherProbability(probabilities, WeatherType.Fog, 10);
                    }
                    break;
                    
                case GameDifficulty.Expert:
                    // Much more severe weather on Expert
                    if (_mapSeason == MapSeason.Summer)
                    {
                        IncreaseWeatherProbability(probabilities, WeatherType.Rain, 20);
                        IncreaseWeatherProbability(probabilities, WeatherType.Fog, 20);
                    }
                    else // Winter
                    {
                        IncreaseWeatherProbability(probabilities, WeatherType.Snow, 20);
                        IncreaseWeatherProbability(probabilities, WeatherType.Fog, 20);
                    }
                    break;
            }
        }

        private void IncreaseWeatherProbability(Dictionary<WeatherType, int> probabilities, WeatherType type, int increase)
        {
            probabilities[type] += increase;
            
            // Normalize other probabilities to maintain a total of 100
            int total = 0;
            foreach (var p in probabilities.Values)
            {
                total += p;
            }
            
            float normalizationFactor = 100f / total;
            
            foreach (var key in probabilities.Keys.ToList())
            {
                probabilities[key] = (int)(probabilities[key] * normalizationFactor);
            }
            
            // Ensure the sum is exactly 100
            int currentSum = probabilities.Values.Sum();
            if (currentSum < 100)
            {
                probabilities[WeatherType.Clear] += (100 - currentSum);
            }
            else if (currentSum > 100)
            {
                probabilities[WeatherType.Clear] -= (currentSum - 100);
            }
        }

        private WeatherType GetRandomWeather(Dictionary<WeatherType, int> probabilities)
        {
            int roll = _random.Next(100);
            int cumulativeProbability = 0;
            
            foreach (var pair in probabilities)
            {
                cumulativeProbability += pair.Value;
                if (roll < cumulativeProbability)
                {
                    return pair.Key;
                }
            }
            
            // Default to clear if something goes wrong
            return WeatherType.Clear;
        }

        public void AdvanceDay()
        {
            _currentDay++;
            if (!_weatherSchedule.ContainsKey(_currentDay))
            {
                // If we somehow exceeded our schedule, generate a new day
                _weatherSchedule[_currentDay] = GetRandomWeather(GetWeatherProbabilities());
            }
            
            // Ensure we also have the next day's weather ready
            if (!_weatherSchedule.ContainsKey(_currentDay + 1))
            {
                _weatherSchedule[_currentDay + 1] = GetRandomWeather(GetWeatherProbabilities());
            }
        }
        
        public ImageSource GetWeatherIcon(WeatherType weather)
        {
            string imagePath = "pack://application:,,,/Assets/Weather/";
            
            switch (weather)
            {
                case WeatherType.Clear:
                    imagePath += "clear.png";
                    break;
                case WeatherType.Rain:
                    imagePath += "rain.png";
                    break;
                case WeatherType.Snow:
                    imagePath += "snow.png";
                    break;
                case WeatherType.Fog:
                    imagePath += "fog.png";
                    break;
            }
            
            try
            {
                return new BitmapImage(new Uri(imagePath));
            }
            catch (Exception)
            {
                // Return a default image if the weather icon can't be loaded
                return new BitmapImage(new Uri("pack://application:,,,/Assets/Weather/clear.png"));
            }
        }
        
        public Color GetWeatherOverlayColor(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Clear:
                    return Color.FromArgb(0, 255, 255, 255); // Transparent
                case WeatherType.Rain:
                    return Color.FromArgb(40, 0, 0, 150); // Slight blue
                case WeatherType.Snow:
                    return Color.FromArgb(40, 200, 200, 255); // Light blue-white
                case WeatherType.Fog:
                    return Color.FromArgb(80, 200, 200, 200); // Grey
                default:
                    return Color.FromArgb(0, 0, 0, 0); // Transparent
            }
        }
        
        public float GetMovementPenalty()
        {
            return MovementPenalties[CurrentWeather];
        }
        
        public float GetAccuracyPenalty()
        {
            return AccuracyPenalties[CurrentWeather];
        }
        
        public float GetVisionPenalty()
        {
            return VisionPenalties[CurrentWeather];
        }
    }
}