using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HunterPie.Core.Definitions;
using HunterPie.Core.Events;

namespace HunterPie.Core
{
    public class ItemBox
    {
        public IReadOnlyDictionary<int, sItem> consumables { get; private set; }
        public IReadOnlyDictionary<int, sItem> ammo { get; private set; }
        public IReadOnlyDictionary<int, sItem> materials { get; private set; }
        public IReadOnlyDictionary<int, sItem> decorations { get; private set; }

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
        public static List<sItem> FindItemsInTab(IReadOnlyDictionary<int, sItem> tab, HashSet<int> ids)
        {
            return ids.Where(id => tab.ContainsKey(id))
                      .Select(id => tab[id])
                      .ToList();
        }


        /// <summary>
        /// Find multiple items in all 4 tabs of the box
        /// </summary>
        /// <param name="ids">Item ids to be searched for in the entire box</param>
        /// <returns>List with all the items found</returns>
        public List<sItem> FindItemsInBox(HashSet<int> ids)
        {
            var foundConsumables = FindItemsInTab(consumables, ids);
            var foundAmmo = FindItemsInTab(ammo, ids);
            var foundMaterials = FindItemsInTab(materials, ids);
            var foundDecorations = FindItemsInTab(decorations, ids);

            return foundConsumables.Concat(foundAmmo)
                                   .Concat(foundMaterials)
                                   .Concat(foundDecorations)
                                   .ToList();
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
            var dConsumables = aConsumables.ToImmutableDictionary(i => i.ItemId, i => i);
            var dAmmo = aAmmo.ToImmutableDictionary(i => i.ItemId, i => i);
            var dMaterials = aMaterials.ToImmutableDictionary(i => i.ItemId, i => i);
            var dDecorations = aDecorations.ToImmutableDictionary(i => i.ItemId, i => i);

            bool updateBox = !(dConsumables.Equals(consumables)
                && dAmmo.Equals(ammo)
                && dMaterials.Equals(materials)
                && dDecorations.Equals(decorations));

            consumables = dConsumables;
            ammo = dAmmo;
            materials = dMaterials;
            decorations = dDecorations;

            if (updateBox)
            {
                OnItemBoxUpdate?.Invoke(this, new ItemBoxUpdatedEventArgs(this));
            }
        }
    }
}
