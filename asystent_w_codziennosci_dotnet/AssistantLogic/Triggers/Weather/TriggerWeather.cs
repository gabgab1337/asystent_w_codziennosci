using AssistantDatabase.Model;
using AssistantLogic.ViewModel;
using AssistantLogic.Common;
using AssistantLogic.Model;
using System.Text;

namespace AssistantLogic.Triggers.Weather
{

    public class TriggerWeather : ITrigger
    {
        string name;
        WeatherType weatherType;
        int? minTemperature;
        int? maxTemperature;
        RainLevel? minRainFall;
        RainLevel? maxRainFall;
        SnowLevel? minSnowFall;
        SnowLevel? maxSnowFall;
        ScaleWind? minWindPower;
        ScaleWind? maxWindPower;
        string? data;
        bool negation;
        
        public TriggerWeather(TriggerDM dm)
        {
            weatherType = (WeatherType)dm.SubType;
            name = dm.Name;
            data = dm.Data;
            negation = dm.Negation;
            string[] weatherIngredients = dm.Data.Split(";");
            minTemperature = ParseInt(weatherIngredients[0]);
            maxTemperature = ParseInt(weatherIngredients[1]);
            switch (weatherType)
            {
                case WeatherType.Rain:
                    int typeminRainFall = int.Parse(weatherIngredients[2]);
                    minRainFall = (RainLevel)typeminRainFall;
                    int typemaxRainFall = int.Parse(weatherIngredients[3]);
                    maxRainFall = (RainLevel)typemaxRainFall;
                    int typeminWindPower = int.Parse(weatherIngredients[4]);
                    minWindPower = (ScaleWind)typeminWindPower;
                    int typemaxWindPower = int.Parse(weatherIngredients[5]);
                    maxWindPower = (ScaleWind)typemaxWindPower;
                    break;
                case WeatherType.Snow:
                    int typeminSnowFall = int.Parse(weatherIngredients[2]);
                    minSnowFall = (SnowLevel)typeminSnowFall;
                    int typemaxSnowFall = int.Parse(weatherIngredients[3]);
                    maxSnowFall = (SnowLevel)typemaxSnowFall;
                    int typeminWindPower2 = int.Parse(weatherIngredients[4]);
                    minWindPower = (ScaleWind)typeminWindPower2;
                    int typemaxWindPower2 = int.Parse(weatherIngredients[5]);
                    maxWindPower = (ScaleWind)typemaxWindPower2;
                    break;
                default:
                    int typeminWindPower3 = int.Parse(weatherIngredients[2]);
                    minWindPower = (ScaleWind)typeminWindPower3;
                    int typemaxWindPower3 = int.Parse(weatherIngredients[3]);
                    maxWindPower = (ScaleWind)typemaxWindPower3;
                    break;
            }
        }

        public static TriggerDM ConvertToDM(TriggerAddEditVM vm)
        {
            TriggerDM dm = new TriggerDM();
            dm.Id = vm.Id;
            dm.Name = vm.Name;
            dm.Type = TriggerType.Weather;
            dm.SubType = (int)vm.WeatherType;
            switch (vm.WeatherType)
            {
                case WeatherType.Rain:
                    dm.Data = vm.MinTemperature.ToString() + ";" + vm.MaxTemperature.ToString() + ";" +
                        ((int)vm.MinRainFall).ToString() + ";" + ((int)vm.MaxRainFall).ToString() + ";"
                        + ((int)vm.MinWindPower).ToString() + ";" + ((int)vm.MaxWindPower).ToString();
                    break;
                case WeatherType.Snow:
                    dm.Data = vm.MinTemperature.ToString() + ";" + vm.MaxTemperature.ToString() + ";" +
                        ((int)vm.MinSnowFall).ToString() + ";" + ((int)vm.MaxSnowFall).ToString() + ";"
                         + ((int)vm.MinWindPower).ToString() + ";" + ((int)vm.MaxWindPower).ToString();
                    break;
                default:
                    dm.Data = vm.MinTemperature.ToString() + ";" + vm.MaxTemperature.ToString() + ";"
                         + ((int)vm.MinWindPower).ToString() + ";" + ((int)vm.MaxWindPower).ToString();
                    break;
            }
            return dm;
        }

        public TriggerAddEditVM VM
        {
            get
            {
                TriggerAddEditVM vm = new TriggerAddEditVM();
                vm.Name = name;
                vm.Type = TriggerType.Weather;

                vm.MinTemperature = minTemperature;
                vm.MaxTemperature = maxTemperature;

                vm.MinWindPower = minWindPower;
                vm.MaxWindPower = maxWindPower;

                switch (weatherType)
                {
                    case WeatherType.Rain:
                        vm.MinRainFall = minRainFall;
                        vm.MaxRainFall = maxRainFall;
                        break;
                    case WeatherType.Snow:
                        vm.MinSnowFall = minSnowFall;
                        vm.MaxSnowFall = maxSnowFall;
                        break;
                }
                return vm;
            }
        }

        public string Description
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                if (negation)
                {
                    builder.Append("NIE ");
                }
               
                if (minTemperature.HasValue && maxTemperature.HasValue)
                {
                    builder.Append($"Temperatura [{minTemperature.Value}, {maxTemperature.Value}], ");
                }
                switch (weatherType)
                {
                    case WeatherType.Rain:
                        if (minRainFall.HasValue && maxRainFall.HasValue)
                        {
                            builder.Append($"Opady deszczu [{minRainFall.Value}, {maxRainFall.Value}], ");
                        }
                        break;
                    case WeatherType.Snow:
                        if (minSnowFall.HasValue && maxSnowFall.HasValue)
                        {
                            builder.Append($"Opady śniegu [{minSnowFall.Value}, {maxSnowFall.Value}], ");
                        }
                        break;
                }
                if (minWindPower.HasValue && maxWindPower.HasValue)
                {
                    builder.Append($"Siła wiatru [{minWindPower.Value}, {maxWindPower.Value}], ");
                }

                if(builder.Length>0)
                {
                    builder[0] = char.ToUpper(builder[0]);
                    builder.Remove(builder.Length - 2, 2);
                }

                return builder.ToString();
            }
        }

        public string ShortDescription
        {
            get
            {
                string textNegation = "";
                if(negation)
                {
                    textNegation = "NIE ";
                }
                switch (weatherType)
                {
                    case WeatherType.Rain:
                        return $"[{textNegation} {minTemperature.Value} - {maxTemperature.Value} C " +
                            $"{minRainFall.GetEnumDescription()} - {maxRainFall.GetEnumDescription()} " + 
                            $"{minWindPower.GetEnumDescription()}-{maxWindPower.GetEnumDescription()}]";
                    case WeatherType.Snow:
                        return $"[{textNegation} {minTemperature.Value} - {maxTemperature.Value} C " +
                            $"{minSnowFall.GetEnumDescription()} - {maxSnowFall.GetEnumDescription()} " + 
                            $"{minWindPower.GetEnumDescription()} - {maxWindPower.GetEnumDescription()}]";
                    default:
                        return $"[{textNegation} {minTemperature.Value} - {maxTemperature.Value} C " + 
                            $"{minWindPower.GetEnumDescription()}-{maxWindPower.GetEnumDescription()}]";
                }
            }
        }

        public string SubTypeName
        {
            get
            {
                return weatherType.GetEnumDescription();
            }
        }

        
        private int? ParseInt(string data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }
            return int.Parse(data);
        }

       
        public bool IsRun(PlanDayParameters parameters)
        {
            DateTime currentDate = parameters.CurrentDate;
            WeatherModel weather = parameters.Weather;

           if (weather.Temperature < minTemperature || maxTemperature < weather.Temperature)
           {
                return false;
           }
            if (weather.WindPower < minWindPower || maxWindPower < weather.WindPower)
            {
                return false;
            }

            switch (weatherType)
            {
                case WeatherType.Default:
                    return true;
                case WeatherType.Rain:
                    return maxRainFall == weather.RainFall;
                case WeatherType.Snow:
                    return maxSnowFall == weather.SnowFall;
                default:
                    return false;
            }   
        }

        public bool Negation
        {
            get
            {
                return negation;
            }
        }

    }
}
