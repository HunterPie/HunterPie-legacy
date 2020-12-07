using System;
using HunterPie.Core;
using HunterPie.Core.Input;
using HunterPie.Logger;
using HunterPie.Core.Events;

namespace HunterPie.Plugins.Example
{
    public class ExamplePlugin : IPlugin
    {
        // This is your plugin name
        public string Name { get; set; } = "Example Plugin";

        // This is your plugin description, try to be as direct as possible on what your plugin does
        public string Description { get; set; } = "An example plugin that you can use as base.";

        // This is our game context, you'll use it to track in-game information and hook events
        public Game Context { get; set; }

        // This is the function HunterPie will call when the game is opened, treat this as your
        // constructor.
        public void Initialize(Game context)
        {
            // We need to set Context here
            Context = context;

            // Read Hotkey API Example
            CreateHotkeys();

            // Read Event Hooking Example
            HookEvents();

            // Read Plugin Extensions Example
            PluginExtensionDemos();
        }

        // This is the function HunterPie will call when unloading your plugin, you MUST unhook all events
        // and clear all the unmanaged stuff here.
        public void Unload()
        {
            RemoveHotkeys();

            UnhookEvents();
        }

        #region Hotkey API Example
        // Example for the Hotkey API, we need a variable to store our hotkey id so we can
        // unregister it on unload.
        readonly int[] hotkeyIds = new int[2];
        public void CreateHotkeys()
        {
            // We can either use lambda functions as callback, or an actual function
            hotkeyIds[0] = Hotkey.Register("Alt+N", () =>
            {
                this.Log("You've pressed Alt+N!");
            });

            hotkeyIds[1] = Hotkey.Register("Alt+B", HotkeyCallback);

            // REMEMBER, YOU MUST SAVE THE ID THAT Hotkey.Register GIVES YOU
            // SO WE CAN UNREGISTER THEM LATER.
        }

        public void HotkeyCallback()
        {
            this.Log("You've pressed Alt+B!");
        }

        public void RemoveHotkeys()
        {
            foreach (int id in hotkeyIds)
                Hotkey.Unregister(id);
        }
        #endregion

        #region Event Hooking Examples
        private void HookEvents()
        {
            // We can access the Player, Monsters and World from Context
            Context.Player.OnZoneChange += OnZoneChangeCallback;
            Context.Player.OnActionChange += OnActionChangeCallback;

            foreach (Monster monster in Context.Monsters)
                monster.OnMonsterSpawn += OnMonsterSpawnCallback;
                
        }

        private void UnhookEvents()
        {
            // To unhook events, we just do the same thing but with a minus instead of a plus
            Context.Player.OnZoneChange -= OnZoneChangeCallback;
            Context.Player.OnActionChange -= OnActionChangeCallback;

            foreach (Monster monster in Context.Monsters)
                monster.OnMonsterSpawn -= OnMonsterSpawnCallback;
        }

        private void OnMonsterSpawnCallback(object source, MonsterSpawnEventArgs args)
        {
            // source will always be a Monster type, so we can cast it to Monster to access other information
            Monster src = (Monster)source;

            this.Log($"{args.Name} just spawned!");
        }

        private void OnActionChangeCallback(object source, EventArgs args)
        {
            // source will always be a Player type, so we can cast it to Player if we want
            Player src = (Player)source;

            // Player actions have an unique name, you can find them all here: https://docs.hunterpie.me/?p=Internal/playerActions.md
            switch (src.PlayerActionRef)
            {
                case "Common::DAMAGE_SLEEP_IDLE":
                case "Common::DAMAGE_SLEEP_MOVE":
                case "Common::DAMAGE_SLEEP_DOWN":
                case "Common::DAMAGE_SLEEP_DOWN_IDLE":
                    this.Log("Time to sleep :peepoSleepo:");
                    break;
            }
        }

        private void OnZoneChangeCallback(object source, EventArgs args)
        {
            PlayerLocationEventArgs nArgs = (PlayerLocationEventArgs)args;

            this.Log($"You're now in {nArgs.ZoneName}");
        }
        #endregion

        #region Plugin Extensions

        private void PluginExtensionDemos()
        {
            // This will log to the console with the name of your plugin
            this.Log("Hello World!");

            // The above is a wrapper for the following:
            Debugger.Module("Hello World!", this.Name);

            // Logging errors
            Debugger.Error("Whoops! Debugger.Error()");
            this.Error("Whoops! this.Error()");

            // Getting this plugin directory path
            string absPath = this.GetPath();
            this.Log($"This plugin is located at: {absPath}");
        }

        #endregion
    }
}
