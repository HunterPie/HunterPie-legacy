using System.Collections.Generic;
using System.Linq;
using HunterPie.Core.Definitions;

namespace HunterPie.Core
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
        /// Find multiple items in the items pouch
        /// </summary>
        /// <param name="ids">Hashset with all the item ids to be searched for</param>
        /// <returns>An array with all the items found</returns>
        public sItem[] FindItems(HashSet<int> ids)
        {
            return Items.Where(slot => ids.Contains(slot.Key)).Select(slot => slot.Value).ToArray();
        }

        /// <summary>
        /// Find multiple ammos in the ammo pouch
        /// </summary>
        /// <param name="ids">Hashset with all the ammo ids to be searched for</param>
        /// <returns>Array with the ammos found</returns>
        public sItem[] FindAmmos(HashSet<int> ids)
        {
            return Ammo.Where(slot => ids.Contains(slot.Key)).Select(slot => slot.Value).ToArray();
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
