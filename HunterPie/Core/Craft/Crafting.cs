using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunterPie.Core.Definitions;

namespace HunterPie.Core.Craft
{
    public class Crafting
    {
        public static int CalculateTotal(sItem[] items, Recipe recipe)
        {
            return recipe.Calculate(items);
        }
    }
}
