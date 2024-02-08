using System.IO;
using System.Text.RegularExpressions;

namespace EmbraceSDK.Validators
{
    public static class AndroidLauncherTemplateValidator 
    {
        private static Regex skipRegex = new Regex(@"(//.*)|\s?");
        private static Regex pluginRegex = new Regex(@"apply plugin: [""|'].+[""|']");
        private const string applyPluginLine = "apply plugin: 'embrace-swazzler'";

        public static bool Validate(string filepath)
        {
            var lines = File.ReadAllLines(filepath);
            
            foreach (var line in lines)
            {
                var match = pluginRegex.Match(line);
                        
                if (match.Success)
                {
                    if (match.ToString().Equals(applyPluginLine))
                    {
                        return true;
                    }
                }
                else if (skipRegex.Match(line).Success)
                {
                    continue; // Skip the line
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}