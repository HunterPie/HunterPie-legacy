using Debugger = HunterPie.Logger.Debugger;
using HunterPie.Core;
using HunterPie.Core.Input;

namespace HunterPie.Plugins
{
    public class ExamplePlugin : IPlugin
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Game Context { get; set; }

        // This variable will hold our hotkey id that we use to unregister it on unload,
        // for multiple hotkeys, you can use a List<int> instead, always adding the valid hotkey ids.
        private int myHotkeyId;

        public void Initialize(Game context)
        {
            Name = "Hotkey Example";
            Description = "A HunterPie plugin to demonstrate the Hotkey API.";
            // You MUST set the Context variable in order to access the game information.
            Context = context;

            SetHotkeys();
        }

        public void Unload()
        {
            // Now we can unregister the hotkey we registered
            // WE MUST UNREGISTER IT ON UNLOAD, if we don't then:
            // 1 - We'll create a memory leak that will only be resolved when HunterPie is closed
            // 2 - We will not be able to register this hotkey again next time the mod loads, unless HunterPie is restarted
            Hotkey.Unregister(myHotkeyId);
        }

        private void SetHotkeys()
        {
            // Hotkey.Register will try to add the hotkey, if it fails it will return -1
            // if it succeeds then it will return a valid hotkey id that you can use it unregister it.
            int hkId = Hotkey.Register("Ctrl+8", MyHotkeyCallback);
            if (hkId > 0)
            {
                myHotkeyId = hkId;
            }
        }

        private void MyHotkeyCallback()
        {
            Debugger.Module("Hotkey was pressed!", Name);
        }

    }
}
