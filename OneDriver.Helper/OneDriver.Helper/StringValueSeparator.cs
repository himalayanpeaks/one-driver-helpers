using System.Globalization;
using System.Text.RegularExpressions;

namespace OneDriver.Helper
{
    public class StringValueSeparator
    {
        public static string[] SeparateCommaStrings(string aStringValues)
        {
            Regex regex1 = new Regex("(,{1}|$)");
            int length = 0;
            for (Match match = regex1.Match(aStringValues); match.Success; match = match.NextMatch())
                ++length;
            List<string> source = new List<string>();
            string[] strArray = new string[length];
            Regex regex2 = new Regex("[\\s]*[A-Za-z0-9_.@-]+[\\s]*");
            if (length > 1)
            {
                Match match = regex2.Match(aStringValues);
                int num = 0;
                while (match.Success)
                {
                    string str = Regex.Replace(match.ToString(), "[\\s]*", "");
                    if (!string.IsNullOrEmpty(str))
                        source.Add(str);
                    match = match.NextMatch();
                    ++num;
                }
            }
            else
                source.Add(aStringValues);
            return source.ToArray<string>();
        }

        public static string SeparateLines(string aLinesNameState) => Regex.Replace(aLinesNameState, "\\s*={1}\\s*\\d*[.]*\\d*", "");

        public static float[] SeperateValues(string aLinesNameState)
        {
            Regex regex = new Regex("(,{1}|$)");
            int length = 0;
            for (Match match = regex.Match(aLinesNameState); match.Success; match = match.NextMatch())
                ++length;
            float[] numArray = new float[length];
            Match match1 = new Regex("[0-9]*[.]*[0-9]+[\\s]*(,{1}|$)").Match(aLinesNameState);
            int index = 0;
            while (match1.Success)
            {
                string str = Regex.Replace(match1.ToString(), "[,]?", "");
                numArray[index] = Convert.ToSingle(str.ToString(), CultureInfo.InvariantCulture);
                match1 = match1.NextMatch();
                ++index;
            }
            return numArray;
        }
    }
}
