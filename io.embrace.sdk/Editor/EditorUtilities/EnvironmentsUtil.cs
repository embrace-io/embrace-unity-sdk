namespace EmbraceSDK.EditorView
{
    public static class EnvironmentsUtil
    {
        public const string EMBRACE_SYSTEM_ENV_INDEX = "EMBRACE_ENVIRONMENTS_INDEX";
        public const string EMBRACE_SYSTEM_ENV_NAME = "EMBRACE_ENVIRONMENTS_NAME";

        internal static Environments ConfigureForBuild()
        {
            // Load our Environments.asset object
            var environments = AssetDatabaseUtil.LoadEnvironments();

            // Check either of our external system environment variables are defined.
            var extIndexVar = System.Environment.GetEnvironmentVariable(EMBRACE_SYSTEM_ENV_INDEX);
            var extNameVar = System.Environment.GetEnvironmentVariable(EMBRACE_SYSTEM_ENV_NAME);

            // If neither is defined, return the environments object as is.
            if (string.IsNullOrEmpty(extIndexVar) && string.IsNullOrEmpty(extNameVar))
            {
                EmbraceLogger.Log($"{EMBRACE_SYSTEM_ENV_INDEX} not defined, skipping build-time Environments configuration.");
                return environments;
            }

            // If both are defined, this is considered an error and should fail the build.
            if (!string.IsNullOrEmpty(extIndexVar) && !string.IsNullOrEmpty(extNameVar))
            {
                EmbraceLogger.LogError($"{EMBRACE_SYSTEM_ENV_INDEX} and {EMBRACE_SYSTEM_ENV_NAME} are both defined. Please choose one environment variable to build with.");
                return null;
            }

            if (!string.IsNullOrEmpty(extIndexVar))
            {
                int.TryParse(extIndexVar, out int envIndex);
                ConfigureByIndex(environments, envIndex);
            }else if (!string.IsNullOrEmpty(extNameVar))
            {
                ConfigureByName(environments, extNameVar);
            }

            return environments;
        }

        private static void ConfigureByIndex(Environments environments, int envIndex)
        {
            // Check for out of range value
            var outOfRange =
                envIndex < 0 ||
                envIndex >= environments.environmentConfigurations.Count;

            // Error if out-of-range
            if (outOfRange)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"{EMBRACE_SYSTEM_ENV_INDEX} was set to an out-of-range index {envIndex}. ");
                sb.Append($"Embrace Environments contains {environments.environmentConfigurations.Count} configurations. ");
                EmbraceLogger.LogError(sb.ToString());
            }

            EmbraceLogger.Log($"Environments.activeEnvironmentIndex was set via {EMBRACE_SYSTEM_ENV_INDEX}={envIndex}");
            environments.activeEnvironmentIndex = envIndex;
        }

        private static void ConfigureByName(Environments environments, string envName)
        {
            var matchingIndex = -1;
            for (int i = 0; i < environments.environmentConfigurations.Count; i++)
            {
                if (environments.environmentConfigurations[i].name == envName)
                {
                    matchingIndex = i;
                    break;
                }
            }

            // Error if name not found
            if (matchingIndex ==-1)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"{EMBRACE_SYSTEM_ENV_NAME} was set to {envName}. ");
                sb.Append($"Embrace Environments does not contain a configuration with this name.");
                EmbraceLogger.LogError(sb.ToString());
            }

            EmbraceLogger.Log($"Environments.activeEnvironmentIndex was set via {EMBRACE_SYSTEM_ENV_NAME}={envName}");
            environments.activeEnvironmentIndex = matchingIndex;
        }
    }
}