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
