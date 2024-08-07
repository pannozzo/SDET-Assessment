using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDET_Assessment
{
    public class Resources
    {
        public enum BrowserType
        {
            Chrome,
            Firefox,
            Edge
        }
        public enum WeatherSummary : ushort
        {
            Undefined = 0,
            Freezing = 1,
            Bracing = 2,
            Chilly = 3,
            Cool = 4,
            Mild = 5,
            Warm = 6,
            Balmy = 7,
            Hot = 8,
            Sweltering = 9,
            Scorching = 10
        }
        public record class WeatherRecord
        (
            DateOnly Date,
            int TemperatureC,
            WeatherSummary Summary
        );
        public enum InvalidWeatherSummary : int
        {
            LowInvalidSummary = -1,
            Undefined = 0,
            Freezing = 1,
            Bracing = 2,
            Chilly = 3,
            Cool = 4,
            Mild = 5,
            Warm = 6,
            Balmy = 7,
            Hot = 8,
            Sweltering = 9,
            Scorching = 10,
            HighInvalidSummary = 11,
        }

        public record class InvalidWeatherRecord
        (
            DateOnly Date,
            int TemperatureC,
            InvalidWeatherSummary Summary
        );
    }
}
