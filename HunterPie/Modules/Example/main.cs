using System;
using HunterPie;
using HunterPie.Core;
using HunterPie.Logger;

namespace HunterPie.Plugins
{
    public class Example : IPlugin
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Game Context { get; set; }

        public void Initialize(Game context)
        {
            Name = "ExampleModule";
            Description = "A plugin example for HunterPie";

            Context = context;
            Context.Player.OnCharacterLogin += OnCharacterLogin;
        }

        public void Unload()
        {
            Context.Player.OnCharacterLogin -= OnCharacterLogin;
        }

        private void OnCharacterLogin(object source, EventArgs args)
        {
            Debugger.Module("This message was triggered by the OnCharacterLogin event!", Name);
        }
    }
}
