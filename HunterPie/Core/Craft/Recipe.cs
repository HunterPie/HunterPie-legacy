using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
            Span<int> temp = stackalloc int[MaterialsNeeded.Length];
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
