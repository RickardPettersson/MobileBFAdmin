using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BFAdmin.Helpers
{
    public class Utils
    {
        public static string GetSquadName(int SquadID)
        {
            List<string> squadNames = new List<string>();
            squadNames.Add("No Squad");
            squadNames.Add("Alpha");
            squadNames.Add("Bravo");
            squadNames.Add("Charlie");
            squadNames.Add("Delta");
            squadNames.Add("Echo");
            squadNames.Add("Foxtrot");
            squadNames.Add("Golf");
            squadNames.Add("Hotel");
            squadNames.Add("India");
            squadNames.Add("Juliet");
            squadNames.Add("Kilo");
            squadNames.Add("Lima");
            squadNames.Add("Mike");
            squadNames.Add("November");
            squadNames.Add("Oscar");
            squadNames.Add("Papa");
            squadNames.Add("Quebec");
            squadNames.Add("Romeo");
            squadNames.Add("Sierra");
            squadNames.Add("Tango");
            squadNames.Add("Uniform");
            squadNames.Add("Victor");
            squadNames.Add("Whiskey");
            squadNames.Add("X-Ray");
            squadNames.Add("Yankee");
            squadNames.Add("Zulu");

            return squadNames[SquadID];
        }
    }
}
