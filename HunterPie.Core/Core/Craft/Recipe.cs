using System;
using System.Linq;
using HunterPie.Core.Definitions;

namespace HunterPie.Core.Craft
{
    public struct RecipeRequirement
    {
        public int ItemId;
        public int Amount;
    }
    public class Recipe
    {
        public int OutputMultiplier { get; set; }
        public RecipeRequirement[] MaterialsNeeded { get; set; }
        public int OutputId { get; set; }

        public int Calculate(sItem[] items)
        {
            int[] temp = new int[MaterialsNeeded.Length];
            int lowest = int.MaxValue;
            for (int i = 0; i < MaterialsNeeded.Length; i++)
            {
                RecipeRequirement requir = MaterialsNeeded[i];
                temp[i] = items.Where(item => item.ItemId == requir.ItemId).FirstOrDefault().Amount / requir.Amount;
                lowest = Math.Min(lowest, temp[i]);
            }
            return lowest != int.MaxValue ? lowest * OutputMultiplier : 0;
        }
    }
}
