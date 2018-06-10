using System.Linq;
using System.Text.RegularExpressions;

namespace Cod4ServerBrowser
{
    static class Util
    {
        public static string RemoveColours(string s)
        {
            return Regex.Replace(s, @"\^\d", "");
        }

        public static string FriendlyGametype(string gametype)
        {
            switch (gametype)
            {
                case "dm": return "Free For All";
                case "war": return "Team Deathmatch";
                case "sd": return "Search And Destroy";
                case "sab": return "Sabotage";
                case "Koth": return "Headquarters";
                case "dom": return "Domination";
                case "ctf": return "Capture The Flag";
                case "cj": return "CodJump";
                case "obs_elim": return "Obscurity";
                case "deathrun": return "Deathrun";
                case "ktk": return "Kill The King";
                case "oitc": return "One in the Chamber";
                case "surv": return "Survival";
                case "hns": return "Hide and Seek";
                default: return gametype;
            }
        }

        public static string FriendlyMapName(string mapname)
        {
            if (Regex.Match(mapname, @"^([A-Za-z0-9]+[_]{1}[A-Za-z0-9]{2,100})+$").Success)
            {
                int startIndex = mapname.IndexOf('_') + 1;
                mapname = mapname.Substring(startIndex, mapname.Length - startIndex);
                return mapname.First().ToString().ToUpper() + mapname.Substring(1);
            }

            return mapname;
        }
    }
}
