using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EmbraceSDK.Validators
{
    #if UNITY_2022_2_OR_NEWER
    public static class AndroidSettingsTemplateValidator
    {
        private static Regex repositoriesRegex = new Regex(@"repositories {.*(\s+.+\s+)+}");
        private static Regex mavenCentralRegex = new Regex(@"mavenCentral\(\)");
        
        public static bool Validate(string filepath)
        {
            var fileText = File.ReadAllText(filepath);

            var repositoriesMatches = repositoriesRegex.Matches(fileText);

            var allRepositoriesValid = repositoriesMatches.Count > 0 && 
                                       repositoriesMatches.All(match => mavenCentralRegex.Match(match.ToString()).Success);

            return allRepositoriesValid;
        }
    }
    #endif
}