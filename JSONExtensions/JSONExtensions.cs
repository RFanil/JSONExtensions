using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace JSONExtensions
{
    public static class JSONExtensions
    {
        private static int levelDepth = 0;

        public static List<JToken> FindTokens(this JToken containerToken, string name)
        {
            List<JToken> matches = new List<JToken>();
            FindTokens(containerToken, name, matches);
            return matches;
        }

        /// </summary>
        /// Return the value by path. The path must be separated by point. Example: "var1:var2:var3"
        /// <param name="containerToken">json tokrn</param>
        /// <param name="path">path to value</param>
        /// <param name="level">level: if the path is not found, the method can return the found "high-level" paths,
        /// for example: input: path = "var1: var2: var3", if var3 is not found, the method returns the path "var1: var2" or "var1".
        /// if the nesting is larger than the level, then we return an empty list</param>
        /// <returns>List<JToken></returns>
        public static List<JToken> FindTokensByPath(this JToken containerToken, string path, int level = 0)
        {
            List<JToken> matches = new List<JToken>();
            List<int> levels = new List<int>() { 0 }; //zero level starts from zero

            string name = "";
            int isLastElement = 0;
            (name, path, isLastElement) = GetChild(path);
            FindTokens(containerToken, name, matches);

            if (matches.Count == 0)
                return matches;
            levels.Add(matches.Count);
            FindTokensByPath(matches, levels, path, matches);

            var newMatches = new List<JToken>();
            if (levels.Count < levelDepth && level == 0)
                return new List<JToken>();
            else if (level == 0)
            {
                for (int i = levels[levels.Count - 2]; i < levels[levels.Count - 1]; i++)
                    newMatches.Add(matches[i]);
                return newMatches;
            }
            else
            {
                for (int i = levels[level - 1]; i < levels[level]; i++)
                    newMatches.Add(matches[i]);
                return newMatches;
            }

        }

        public static List<JToken> FindTokensByPath(List<JToken> containerToken, List<int> levels, string path, List<JToken> matches, int level = 0)
        {
            int matchesCount = matches.Count;
            string name = "";
            int isLastElement = 0;
            (name, path, isLastElement) = GetChild(path);
            if (isLastElement >= 0)
            {
                for (; level < matchesCount; level++)
                    FindTokens(matches[level], name, matches);
                levels.Add(matches.Count);
                FindTokensByPath(matches, levels, path, matches, matchesCount);
            }
            else
            {
                for (; level < matchesCount; level++)
                    FindTokens(matches[level], name, matches);
                levels.Add(matches.Count);
            }
            return matches;
        }

        private static (string, string, int) GetChild(string path)
        {
            levelDepth++;
            int positionOfNewLine = path.IndexOf(":");
            string name = "";
            if (positionOfNewLine >= 0)
            {
                name = path.Substring(0, positionOfNewLine);
                path = path.Substring(positionOfNewLine + 1, path.Length - positionOfNewLine - 1);
            }
            else
            {
                name = path;
            }
            return (name, path, positionOfNewLine);
        }

        private static void FindTokens(JToken containerToken, string name, List<JToken> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    if (child.Name == name)
                    {
                        matches.Add(child.Value);
                    }
                    FindTokens(child.Value, name, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokens(child, name, matches);
                }
            }
        }
    }

    public static class JsonParser
        {
            public static T Parse<T>(string json, string specificKeyName) where T : new()
            {
                T returnType = new T();
                if (!string.IsNullOrEmpty(json))
                {
                    JObject jo = JObject.Parse(json);
                    var tokens = jo.FindTokens(specificKeyName);
                    returnType = JsonConvert.DeserializeObject<T>(tokens[0].ToString());
                }
                return returnType;
            }
        }
    }
