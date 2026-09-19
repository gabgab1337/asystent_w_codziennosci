using AssistantLogic.Model;
using AssistantLogic.Triggers.Weather;
using AssistantLogic.IExternalServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace AssistantLogic.Services
{
    public class WeatherService : IWeatherService
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly string _apiKey;

        public WeatherService(IConfiguration configuration)
        {
            _apiKey = configuration["WeatherApi:ApiKey"]
                ?? throw new InvalidOperationException("Missing \"WeatherApi:ApiKey\" configuration.");
        }

        public async Task<WeatherModel?> GetCurrentWeather()
        {
            var response = await client.GetStringAsync(
                $"http://api.openweathermap.org/data/2.5/weather?q=Opole&appid={_apiKey}");

            JObject json = JObject.Parse(response);
            if(json == null)
            {
                return null;
            }
          
            return ConvertToModel(json);

        }

        private WeatherModel? ConvertToModel(JObject json)
        {
            WeatherModel model = new WeatherModel();

            float temperature = float.Parse(json["main"]["temp"].ToString());
            model.Temperature = (int)((temperature - 273.15) + 0.5);
            float windSpeed = float.Parse(json["wind"]["speed"].ToString());
            model.WindPower = GetBoffortScaleWind(windSpeed);

            model.WeatherType = WeatherType.Default;

            int weatherId = int.Parse(json["weather"][0]["id"].ToString());
            switch (weatherId)
            {
                case 200:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.ThunderstormWithLightRain;
                    break;
                case 201:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.ThunderstormWithRain;
                    break;
                case 202:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.ThunderstormWithHeavyRain;
                    break;
                case 210:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.LightThunderstorm;
                    break;
                case 211:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.Thunderstorm;
                    break;
                case 212:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.HeavyThunderstorm;
                    break;
                case 221:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.RaggedThunderstorm;
                    break;
                case 230:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.ThunderstormWithLightDrizzle;
                    break;
                case 231:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.ThunderstormWithDrizzle;
                    break;
                case 232:
                    model.WeatherType = WeatherType.Thunderstorm;
                    model.ThunderstormLevel = ThunderstormLevel.ThunderstormWithHeavyDrizzle;
                    break;
                case 300:
                    model.WeatherType = WeatherType.Drizzle;
                    model.DrizzleLevel = DrizzleLevel.LightIntensityDrizzle;
                    break;
                case 301:
                    model.WeatherType = WeatherType.Drizzle;
                    model.DrizzleLevel = DrizzleLevel.Drizzle;
                    break;
                case 302:
                    model.WeatherType = WeatherType.Drizzle;
                    model.DrizzleLevel = DrizzleLevel.HeavyIntensityDrizzle;
                    break;
                case 310:
                    model.WeatherType = WeatherType.Drizzle;
                    model.DrizzleLevel = DrizzleLevel.LightIntensityDrizzleRain;
                    break;
                case 311:
                    model.WeatherType = WeatherType.Drizzle;
                    model.DrizzleLevel = DrizzleLevel.DrizzleRain;
                    break;
                case 312:
                    model.WeatherType = WeatherType.Drizzle;
                    model.DrizzleLevel = DrizzleLevel.HeavyIntensityDrizzleRain;
                    break;
                case 313:
                    model.WeatherType = WeatherType.Drizzle;
                    model.DrizzleLevel = DrizzleLevel.ShowerRainAndDrizzle;
                    break;
                case 314:
                    model.WeatherType = WeatherType.Drizzle;
                    model.DrizzleLevel = DrizzleLevel.HeavyShowerRainAndDrizzle;
                    break;
                case 321:
                    model.WeatherType = WeatherType.Drizzle;
                    model.DrizzleLevel = DrizzleLevel.ShowerDrizzle;
                    break;
                case 500:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.LightRain;
                    break;
                case 501:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.ModerateRain;
                    break;
                case 502:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.HeavyIntensityRain;
                    break;
                case 503:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.VeryHeavyRain;
                    break;
                case 504:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.ExtremeRain;
                    break;
                case 511:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.FreezingRain;
                    break;
                case 520:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.LightIntensityShowerRain;
                    break;
                case 521:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.ShowerRain;
                    break;
                case 522:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.HightIntensityShowerRain;
                    break;
                case 531:
                    model.WeatherType = WeatherType.Rain;
                    model.RainFall = RainLevel.RaggedShowerRain;
                    break;
                case 600:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.LightSnow;
                    break;
                case 601:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.Snow;
                    break;
                case 602:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.HeavySnow;
                    break;
                case 611:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.Sleet;
                    break;
                case 612:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.LightShowerSleet;
                    break;
                case 613:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.ShowerSleet;
                    break;
                case 615:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.LightRainAndSnow;
                    break;
                case 616:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.RainAndSnow;
                    break;
                case 620:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.LightShowerSnow;
                    break;
                case 621:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.ShowerSnow;
                    break;
                case 622:
                    model.WeatherType = WeatherType.Snow;
                    model.SnowFall = SnowLevel.HeavyShowerSnow;
                    break;
                case 711:
                    model.WeatherType = WeatherType.Smoke;
                    break;
                case 721:
                    model.WeatherType = WeatherType.Haze;
                    break;
                case 731:
                    model.WeatherType = WeatherType.Dust;
                    break;
                case 741:
                    model.WeatherType = WeatherType.Fog;
                    break;
                case 751:
                    model.WeatherType = WeatherType.Sand;
                    break;
                case 761:
                    model.WeatherType = WeatherType.Dust;
                    break;
                case 762:
                    model.WeatherType = WeatherType.Ash;
                    break;
                case 771:
                    model.WeatherType = WeatherType.Squall;
                    break;
                case 781:
                    model.WeatherType = WeatherType.Tornado;
                    break;
                case 800:
                    model.WeatherType = WeatherType.Clear;
                    break;
                case 801:
                    model.WeatherType = WeatherType.Clouds;
                    model.Cloud = CloudLevel.FewClouds;
                    break;
                case 802:
                    model.WeatherType = WeatherType.Clouds;
                    model.Cloud = CloudLevel.ScatteredClouds;
                    break;
                case 803:
                    model.WeatherType = WeatherType.Clouds;
                    model.Cloud = CloudLevel.BrokenClouds;
                    break;
                case 804:
                    model.WeatherType = WeatherType.Clouds;
                    model.Cloud = CloudLevel.OvercastClouds;
                    break;
            }
            model.LastUpdate = DateTime.Now;
            return model;
        }

        ScaleWind GetBoffortScaleWind(float wind)
        {
            if (wind < 0.3)
            {
                return ScaleWind.Calm;
            }
            if (wind <= 1.5)
            {
                return ScaleWind.LightAir;
            }
            if (wind <= 3.3)
            {
                return ScaleWind.LightBreeze;
            }
            if (wind <= 5.5)
            {
                return ScaleWind.GentleBreeze;
            }
            if (wind <= 7.9)
            {
                return ScaleWind.ModerateBreeze;
            }
            if (wind <= 10.7)
            {
                return ScaleWind.FreshBreeze;
            }
            if (wind <= 13.8)
            {
                return ScaleWind.StrongBreeze;
            }
            if (wind <= 17.1)
            {
                return ScaleWind.HighWind;
            }
            if (wind <= 20.7)
            {
                return ScaleWind.Gale;
            }
            if (wind <= 24.4)
            {
                return ScaleWind.StrongGale;
            }
            if (wind <= 28.4)
            {
                return ScaleWind.WholeGale;
            }
            if (wind <= 32.6)
            {
                return ScaleWind.ViolentGale;
            }
            
            return ScaleWind.Hurricane;
        }
    }


}
