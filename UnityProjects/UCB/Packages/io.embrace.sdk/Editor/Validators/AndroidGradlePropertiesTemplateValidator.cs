using System.IO;
using System.Text.RegularExpressions;

namespace EmbraceSDK.Validators
{
    public static class AndroidGradlePropertiesTemplateValidator
    {
        private static Regex androidXRegex = new Regex(@"android.useAndroidX\s*=\s*true");
        private static Regex jetifierRegex = new Regex(@"android.enableJetifier\s*=\s*true");

        public static (bool foundAndroidX, bool foundJetifier) Validate(string filepath)
        {
            var fileText = File.ReadAllText(filepath);
            return (androidXRegex.Match(fileText).Success, jetifierRegex.Match(fileText).Success);
        }
    }
}