using System.Collections.Generic;
using System.Linq;
using HunterPie.Core.Definitions;

namespace HunterPie.Core.Local
{
    public class Inventory
    {
        readonly Dictionary<int, sItem> items = new Dictionary<int, sItem>();
        readonly Dictionary<int, sItem> ammo = new Dictionary<int, sItem>();

        public IReadOnlyDictionary<int, sItem> Items => items;
        public IReadOnlyDictionary<int, sItem> Ammo => ammo;

        /// <summary>
        /// Finds consumable/items in the player's item pouch
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public sItem? FindItem(int id)
        {
            if (Items.ContainsKey(id))
            {
                return Items[id];
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds an ammo with the specified id in the player's ammo pouch
        /// </summary>
        /// <param name="id">Ammo item Id</param>
        /// <returns>sItem if the ammo was found, null otherwise</returns>
        public sItem? FindAmmo(int id)
        {
            if (Ammo.ContainsKey(id))
            {
                return Ammo[id];
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Refreshes the player inventory
        /// </summary>
        /// <param name="itemArray">Array with the items</param>
        internal void RefreshPouch(sItem[] itemArray)
        {
            HashSet<int> set = itemArray.Select(i => i.ItemId).ToHashSet();
            // Our Item pouch has 24 slots
            for (int i = 0; i < 24; i++)
            {
                sItem item = itemArray[i];

                items[item.ItemId] = item;
                set.Remove(item.ItemId);
            }

            // Our Ammo pouch only has 16 slots
            for (int i = 24; i < itemArray.Length; i++)
            {
                sItem item = itemArray[i];

                ammo[item.ItemId] = item;
                set.Remove(item.ItemId);
            }

            // Now we clear the dictionary to remove items that are not in our inventory anymore
            foreach (int id in set)
            {
                if (items.ContainsKey(id))
                {
                    items.Remove(id);
                }
                else if (ammo.ContainsKey(id))
                {
                    ammo.Remove(id);
                }
            }
        }
    }
}
