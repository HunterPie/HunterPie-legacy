using System;
using System.Collections.Generic;
using System.Linq;
using HunterPie.Core.Definitions;
using HunterPie.Core.Events;
using HunterPie.Utils;

namespace HunterPie.Core
{
    public class ItemBox
    {
        private Dictionary<int, int> consumables = new Dictionary<int, int>();
        private Dictionary<int, int> ammo = new Dictionary<int, int>();
        private Dictionary<int, int> materials = new Dictionary<int, int>();
        private Dictionary<int, int> decorations = new Dictionary<int, int>();

        public IReadOnlyDictionary<int, int> Consumables => consumables;
        public IReadOnlyDictionary<int, int> Ammo => ammo;
        public IReadOnlyDictionary<int, int> Materials => materials;
        public IReadOnlyDictionary<int, int> Decorations => decorations;

        public event EventHandler<ItemBoxUpdatedEventArgs> OnItemBoxUpdate;

        /// <summary>
        /// Find multiple items in the given tab
        /// </summary>
        /// <param name="tab">
        /// ImmutableDictionary for tab to search through i.e. conumables, ammo, etc.
        /// It should be one of the ItemBox dictionaries.
        /// </param>
        /// <param name="ids">Hashset with all the item ids to be searched for</param>
        /// <returns>List with all the items found</returns>
        public static Dictionary<int, int> FindItemsInTab(IReadOnlyDictionary<int, int> tab, HashSet<int> ids)
        {
            return tab.Where(item => ids.Contains(item.Key))
                      .ToDictionary(i => i.Key, i => i.Value);
        }

        /// <summary>
        /// Find multiple items in all 4 tabs of the box
        /// </summary>
        /// <param name="ids">Item ids to be searched for in the entire box</param>
        /// <returns>List with all the items found</returns>
        public Dictionary<int, int> FindItemsInBox(HashSet<int> ids)
        {
            var foundConsumables = FindItemsInTab(Consumables, ids);
            var foundAmmo = FindItemsInTab(Ammo, ids);
            var foundMaterials = FindItemsInTab(Materials, ids);
            var foundDecorations = FindItemsInTab(Decorations, ids);

            return foundConsumables.Concat(foundAmmo)
                                   .Concat(foundMaterials)
                                   .Concat(foundDecorations)
                                   .ToDictionary(i => i.Key, i => i.Value);
        }

        /// <summary>
        /// Refreshes the player box
        /// </summary>
        /// <param name="aConsumables">Array with the consumables</param>
        /// <param name="aAmmo">Array with the ammo</param>
        /// <param name="aMaterials">Array with the materials</param>
        /// <param name="aDecorations">Array with the decorations</param>
        internal void Refresh(sItem[] aConsumables, sItem[] aAmmo, sItem[] aMaterials, sItem[] aDecorations)
        {
            var dConsumables = aConsumables.Where(i => i.ItemId != 0)
                .Distinct(new sItemEqualityComparer())
                .ToDictionary(i => i.ItemId, i => i.Amount);

            var dAmmo = aAmmo.Where(i => i.ItemId != 0)
                .Distinct(new sItemEqualityComparer())
                .ToDictionary(i => i.ItemId, i => i.Amount);

            var dMaterials = aMaterials.Where(i => i.ItemId != 0)
                .Distinct(new sItemEqualityComparer())
                .ToDictionary(i => i.ItemId, i => i.Amount);

            var dDecorations = aDecorations.Where(i => i.ItemId != 0)
                .Distinct(new sItemEqualityComparer())
                .ToDictionary(i => i.ItemId, i => i.Amount);

            bool updateBox = !(dConsumables.IsEqualTo(consumables)
                && dAmmo.IsEqualTo(ammo)
                && dMaterials.IsEqualTo(materials)
                && dDecorations.IsEqualTo(decorations));

            if (updateBox)
            {
                consumables = dConsumables;
                ammo = dAmmo;
                materials = dMaterials;
                decorations = dDecorations;
                
                OnItemBoxUpdate?.Invoke(this, new ItemBoxUpdatedEventArgs(this));
            }
        }
    }
}
