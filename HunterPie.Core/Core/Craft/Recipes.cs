using System;
using System.Collections.Generic;
using Debugger = HunterPie.Logger.Debugger;
using System.IO;
using System.Linq;
using System.Xml;

namespace HunterPie.Core.Craft
{
    public class Recipes
    {
        private static readonly Dictionary<int, List<Recipe>> list = new Dictionary<int, List<Recipe>>();

        public static IReadOnlyDictionary<int, List<Recipe>> List => list;

        internal static void LoadRecipes()
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HunterPie.Resources/Data/CraftingData.xml"));

                XmlNodeList items = document.SelectNodes("//Crafting/Item");

                IEnumerable<Recipe> recipes = items.Cast<XmlNode>()
                    .Select(node => XmlNodeToRecipe(node));

                foreach (Recipe recipe in recipes)
                {
                    AddNewRecipe(recipe.OutputId, recipe);
                }

                Debugger.Warn($"Loaded {recipes.Count()} different item crafting recipes!");

                document = null;
            }
            catch (Exception err)
            {
                Debugger.Error($"Failed to load crafting data.\n{err}");
            }

        }

        public static Recipe XmlNodeToRecipe(XmlNode recipeData)
        {
            if (recipeData?.Attributes is null)
            {
                throw new ArgumentNullException(nameof(recipeData), "Crafting data cannot be null!");
            }

            Recipe recipe = new Recipe();

            XmlNodeList requirNodes = recipeData.SelectNodes("Material");
            RecipeRequirement[] requirements = new RecipeRequirement[requirNodes?.Count ?? 0];

            // Set the materials
            for (int i = 0; i < requirements.Length; i++)
            {
                RecipeRequirement req = new RecipeRequirement()
                {
                    ItemId = int.Parse(requirNodes[i].Attributes["Id"]?.Value ?? "0"),
                    Amount = int.Parse(requirNodes[i].Attributes["Amount"]?.Value ?? "0")
                };
                requirements[i] = req;
            }

            recipe.OutputMultiplier = int.Parse(recipeData.Attributes["OutMultiplier"]?.Value ?? "0");
            recipe.MaterialsNeeded = requirements;
            recipe.OutputId = int.Parse(recipeData.Attributes["Output"]?.Value ?? "0");

            return recipe;
        }

        /// <summary>
        /// Finds the crafting recipe for the given id
        /// </summary>
        /// <param name="id">Item Id to craft</param>
        /// <returns>Recipe</returns>
        public static Recipe FindRecipe(int id)
        {
            if (List.ContainsKey(id))
            {
                return List[id].First();
            }
            else
            {
                return null;
            }
        }

        public static List<Recipe> FindRecipes(int id)
        {
            if (List.ContainsKey(id))
            {
                return List[id];
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Adds a new recipe to the Recipes list
        /// </summary>
        /// <param name="id">Id of the item that will be crafted by this recipe</param>
        /// <param name="recipe">Recipe to craft the item</param>
        /// <returns>True if the recipe has been added succesfully, false if not</returns>
        public static bool AddNewRecipe(int id, Recipe recipe)
        {
            if (FindRecipe(id) is null)
            {
                list.Add(id, new List<Recipe> { recipe });
                return true;
            } else
            {
                list[id].Add(recipe);
                return true;
            }
        }
    }
}
