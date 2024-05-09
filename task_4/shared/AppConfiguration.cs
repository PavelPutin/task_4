using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace task_4.shared
{
    public class AppConfiguration
    {
        private static AppConfiguration? instance;

        public AppConfiguration()
        {
            var config = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("application.json").Build();

            DISTANCE = parseIntConfig(config, "DISTANCE", 100);

            QUADCOPTERS_INIT_NUMBER = parseIntConfig(config, "QUADCOPTERS_INIT_NUMBER");
            OPERATORS_INIT_NUMBER = parseIntConfig(config, "OPERATORS_INIT_NUMBER");
            SPECIALIZED_MECHANICS_INIT_NUMBER = parseIntConfig(config, "SPECIALIZED_MECHANICS_INIT_NUMBER");

            MAXIMUM_NUMBER_QUADCOPTERS_SERVICED = parseIntConfig(config, "MAXIMUM_NUMBER_QUADCOPTERS_SERVICED");

            QUADCOPTER_LOADING_TIME = parseIntConfig(config, "QUADCOPTER_LOADING_TIME");
            QUADCOPTER_TAKEOFF_TIME = parseIntConfig(config, "QUADCOPTER_TAKEOFF_TIME");
            QUADCOPTER_LANDING_TIME = parseIntConfig(config, "QUADCOPTER_LANDING_TIME");
            QUADCOPTER_TRAVEL_SPEED = parseIntConfig(config, "QUADCOPTER_TRAVEL_SPEED");
            QUADCOPTER_BREAKDOWN_RATE = parseDoubleConfig(config, "QUADCOPTER_BREAKDOWN_RATE", 0.2);

            MECHANIC_TRAVEL_SPEED = parseIntConfig(config, "MECHANIC_TRAVEL_SPEED");
            SPECIALIST_MECHANIC_REPAIR_TIME = parseIntConfig(config, "SPECIALIST_MECHANIC_REPAIR_TIME");
            OPERATOR_REPAIR_TIME = parseIntConfig(config, "OPERATOR_REPAIR_TIME");
        }

        public static AppConfiguration Instance => instance ??= new AppConfiguration();
        
        public int DISTANCE { get; }
        public int QUADCOPTERS_INIT_NUMBER { get; }
        public int OPERATORS_INIT_NUMBER { get; }
        public int SPECIALIZED_MECHANICS_INIT_NUMBER { get; }
        public int MAXIMUM_NUMBER_QUADCOPTERS_SERVICED {  get; }
        public int QUADCOPTER_LOADING_TIME { get; }
        public int QUADCOPTER_TAKEOFF_TIME { get; }
        public int QUADCOPTER_LANDING_TIME { get; }
        public int QUADCOPTER_TRAVEL_SPEED { get; }
        public double QUADCOPTER_BREAKDOWN_RATE { get; }

        public int MECHANIC_TRAVEL_SPEED { get; }
        public int SPECIALIST_MECHANIC_REPAIR_TIME { get; }
        public int OPERATOR_REPAIR_TIME { get; }

        private int parseIntConfig(IConfigurationRoot config, string key, int defaultValue = 1)
        {
            //TODO: добавить обработку ошибок
            if (config[key] == null)
            {
                return defaultValue;
            }
            return int.Parse(config[key]!);
        }

        private double parseDoubleConfig(IConfigurationRoot config, string key, double defaultValue = 1.0)
        {
            //TODO: добавить обработку ошибок
            if (config[key] == null)
            {
                return defaultValue;
            }
            return double.Parse(config[key]!, CultureInfo.InvariantCulture);
        }
    }
}
