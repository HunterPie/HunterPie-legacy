using NUnit.Framework;
using System.Collections.Generic;
using HunterPie.Core.Definitions;

namespace HunterPie.Core.Tests
{
    [TestFixture]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members",
        Justification = "Test case data functions are used by TestCaseSource(). Looks like reflection")]
    public class ItemBoxTest
    {
        [Test, TestCaseSource("OnItemBoxUpdateData")]
        public void OnItemBoxUpdateTest(List<sItem> inititalList, List<sItem> refreshedList, bool eventTriggered)
        {
            var box = new ItemBox();
            var triggered = false;

            sItem[] empty = { };
            box.Refresh(inititalList.ToArray(), empty, empty, empty);
            box.OnItemBoxUpdate += (sender, args) => { triggered = true; };

            box.Refresh(refreshedList.ToArray(), empty, empty, empty);

            Assert.AreEqual(eventTriggered, triggered);
        }

        private static IEnumerable<TestCaseData> OnItemBoxUpdateData()
        {
            var array1 = new List<sItem> {
                new sItem { ItemId = 1, Amount = 900 },
                new sItem { ItemId = 2, Amount = 700 },
            };
            var array2 = new List<sItem>(array1)
            {
                new sItem { ItemId = 3, Amount = 1 }
            };
            var array3 = new List<sItem>(array2);
            array3.Reverse();

            yield return new TestCaseData(array1, array1, false)
                .SetName("OnItemBoxUpdate is not triggered if box hasn't changed");
            yield return new TestCaseData(array2, array3, false)
                .SetName("OnItemBoxUpdate is not triggered if box is reordered");
            yield return new TestCaseData(array1, array2, true)
                .SetName("OnItemBoxUpdate is triggered if box has changed");
        }

        [Test, TestCaseSource("FindItemsInTabData")]
        public void FindItemsInTabTest(List<sItem> consumables, HashSet<int> ids, HashSet<int> expectedIds)
        {
            var box = new ItemBox();

            sItem[] empty = {  };
            box.Refresh(consumables.ToArray(), empty, empty, empty);
            var actualItems = ItemBox.FindItemsInTab(box.Consumables, ids);
            Assert.NotNull(actualItems);
            CollectionAssert.AreEquivalent(expectedIds, actualItems.Keys);
        }

        private static IEnumerable<TestCaseData> FindItemsInTabData()
        {
            var array1 = new List<sItem> {
                new sItem { ItemId = 1, Amount = 900 },
                new sItem { ItemId = 2, Amount = 700 },
                new sItem { ItemId = 3, Amount = 1 },
            };

            yield return new TestCaseData(array1, new HashSet<int> { 1, 2 }, new HashSet<int> { 1, 2 })
                .SetName("FindItemsInTab should return dictionary with the items that were found");
            yield return new TestCaseData(array1, new HashSet<int> { 1, 2, 4 }, new HashSet<int> { 1, 2 })
                .SetName("FindItemsInTab should not error if query includes ids that are not in the dictionary");
            yield return new TestCaseData(array1, new HashSet<int> { 7 }, new HashSet<int> { })
                .SetName("FindItemsInTab should return empty Dictionary if no matches found");
        }

        [Test, TestCaseSource("FindItemsInBoxData")]
        public void FindItemsInBoxTest(List<sItem> consumables,
            List<sItem> ammo,
            List<sItem> materials,
            List<sItem> decorations,
            HashSet<int> ids,
            HashSet<int> expectedIds)
        {
            var box = new ItemBox();

            box.Refresh(consumables.ToArray(), ammo.ToArray(), materials.ToArray(), decorations.ToArray());
            var actualItems = box.FindItemsInBox(ids);
            CollectionAssert.AreEquivalent(expectedIds, actualItems.Keys);
        }

        private static IEnumerable<TestCaseData> FindItemsInBoxData()
        {
            var consumables = new List<sItem> { new sItem { ItemId = 1, Amount = 900 } };
            var ammo = new List<sItem> { new sItem { ItemId = 137, Amount = 300 } };
            var materials = new List<sItem> { new sItem { ItemId = 205, Amount = 13 } };
            var decorations = new List<sItem> { new sItem { ItemId = 727, Amount = 900 } };

            yield return new TestCaseData(consumables, ammo, materials, decorations, new HashSet<int> { 1, 205 }, new HashSet<int> { 1, 205 })
                .SetName("FindItemsInBox should return dictionary with the items that were found");
            yield return new TestCaseData(consumables, ammo, materials, decorations, new HashSet<int> { 1, 2, 4 }, new HashSet<int> { 1 })
                .SetName("FindItemsInBox should not error if query includes ids that are not in the box");
            yield return new TestCaseData(consumables, ammo, materials, decorations, new HashSet<int> { 7 }, new HashSet<int> { })
                .SetName("FindItemsInBox should return empty Dictionary if no matches found");
        }

        [Test]
        public void DuplicatesShouldDiscarded()
        {
            sItem[] dups = {
                new sItem { ItemId = 1, Amount = 900 },
                new sItem { ItemId = 1, Amount = 99 },
            };

            var box = new ItemBox();
            sItem[] empty = { };
            box.Refresh(dups, empty, empty, empty);
            Assert.AreEqual(900, box.Consumables[1]);
        }
    }
}
