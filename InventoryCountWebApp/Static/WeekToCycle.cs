using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryCountWebApp.Database
{
    public class WeekToCycle
    {
        public List<int> DetermineCode(int weekNumber)
        {

            List<int> classA = new List<int>();
            List<int> classB = new List<int>();
            List<int> classC = new List<int>();

            // This is all hard-coded based off the structure of the cycle-count-codes
            // provided by Brian
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 13; j++)
                {
                    classA.Add(j);
                }
            }

            for (int i = 1; i <= 2; i++)
            {
                for (int j = 14; j <= 39; j++)
                {
                    classB.Add(j);
                }
            }

            for (int j = 40; j <= 91; j++)
            {
                classC.Add(j);
            }

            List<int> CycleCodes = new List<int>();

            CycleCodes.Add(classA[weekNumber-1]);
            CycleCodes.Add(classB[weekNumber-1]);
            CycleCodes.Add(classC[weekNumber-1]);

            return CycleCodes;
        }
    }
}