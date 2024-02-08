using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EmbraceSDK.Validators
{
    public static class AndroidBaseProjectTemplateValidator
    {
        private static Regex dependenciesRegex = new Regex(@"dependencies\s*{([^}]*?)}");
        private static Regex classpathRegex = new Regex(@"classpath\s*[""|']io.embrace:embrace-swazzler:\d+.\d+.\d+[""|']");

        // TODO: This regex will need to be updated to whatever the bug shake plugin version ends up being.
        private static Regex bugshakeRegex =
            new Regex(@"classpath\s*[""|']io.embrace:embrace-bug-shake-gradle-plugin:\d+.\d+.\d+[""|']");
        
        #if !UNITY_2022_2_OR_NEWER
        private static Regex repositoriesRegex = new Regex(@"repositories\s*{.*(\s+.+\s+)+}");
        private static Regex mavenCentralRegex = new Regex(@"mavenCentral\(\)");
        #endif

        #if UNITY_2022_2_OR_NEWER
        public static bool Validate(string filepath)
        {
            var fileText = File.ReadAllText(filepath);
            
            var dependenciesMatches = dependenciesRegex.Matches(fileText);

            return dependenciesMatches.Count > 0 && 
                   dependenciesMatches.Any(match => classpathRegex.Match(match.ToString()).Success);
        }
        #else
        public static (bool foundImport, bool allRepositoriesValid) Validate(string filepath)
        {
            var fileText = File.ReadAllText(filepath);

            // Check dependencies
            var dependenciesMatches = dependenciesRegex.Matches(fileText);

            var foundImport = false;
            
            if (dependenciesMatches.Count > 0)
            {
                foreach (var match in dependenciesMatches)
                {
                    var matchString = match.ToString().Trim();
                    var lines = matchString.Split('\n');
                    
                    foreach (var line in lines)
                    {
                        #if EMBRACE_ENABLE_BUGSHAKE_FORM
                        if (bugshakeRegex.Match(line).Success)
                        {
                            foundImport = true;
                            break;
                        }
                        #else
                        if (classpathRegex.Match(line).Success)
                        {
                            foundImport = true;
                            break;
                        }
                        #endif
                    }

                    if (foundImport)
                    {
                        break;
                    }
                }
            }

            // Check repository declarations
            var repositoriesMatches = repositoriesRegex.Matches(fileText);
            var allRepositoriesValid = repositoriesMatches.Count > 0 && 
                                       repositoriesMatches.Cast<Match>().All(match => mavenCentralRegex.Match(match.ToString()).Success);

            return (foundImport, allRepositoriesValid);
        }
        #endif
    }
}