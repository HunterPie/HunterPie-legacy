using NUnit.Framework;
using HunterPie.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;

namespace HunterPie.Plugins.Tests
{
    [TestFixture()]
    public class PluginSettingsHelperTest
    {
        private static JObject emptyJson = new JObject();
        private static JObject disabledJson = JObject.Parse(@"{'IsEnabled': false}");
        private static JObject extraJson = JObject.Parse(@"{
            'IsEnabled': false,
            'SomeOtherProperty': 'Some value'
        }");
        private static JObject extraJson2 = JObject.Parse(@"{
            'IsEnabled': false,
            'SomeOtherProperty': 'Some value'
        }");
        private static JObject extraJsonEnabled = JObject.Parse(@"{
            'IsEnabled': true,
            'SomeOtherProperty': 'Some value'
        }");

        [Test, TestCaseSource("GetPluginSettingsData")]
        public void GetPluginSettingsTest(JObject json, PluginSettings expectedSettings)
        {
            var actualSettings = PluginSettingsHelper.GetPluginSettings(json);
            actualSettings.Should().BeEquivalentTo(expectedSettings);
        }

        private static IEnumerable<TestCaseData> GetPluginSettingsData()
        {
            var enabled = new PluginSettings();
            var disabled = new PluginSettings();
            disabled.IsEnabled = false;

            yield return new TestCaseData(emptyJson, enabled)
                .SetName("Should return default PluginSettings if given empty JSON");
            yield return new TestCaseData(disabledJson, disabled)
                .SetName("Should return matching PluginSettings");
            yield return new TestCaseData(extraJson, disabled)
                .SetName("Should return PluginSettings extracted from JSON");
        }

        [Test, TestCaseSource("MergePluginSettingsData")]
        public void MergePluginSettingsTest(JObject json, PluginSettings overrides, JObject expectedJson)
        {
            PluginSettingsHelper.mergePluginSettings(json, overrides);
            json.Should().BeEquivalentTo(expectedJson);
        }

        private static IEnumerable<TestCaseData> MergePluginSettingsData()
        {
            var enabled = new PluginSettings();
            var disabled = new PluginSettings();
            disabled.IsEnabled = false;

            // Need to make these here because merge modifies the instance...
            var emptyJson = new JObject();
            var extraJson = JObject.Parse(@"{
                'IsEnabled': false,
                'SomeOtherProperty': 'Some value'
            }");

            yield return new TestCaseData(emptyJson, disabled, disabledJson)
                .SetName("Should populate empty settings with default");
            yield return new TestCaseData(extraJson, enabled, extraJsonEnabled)
                .SetName("Should override settings but keep the settings that don't have overrides");
        }
    }
}
