using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace StopTheClip
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private IDalamudPluginInterface? iPluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.iPluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.iPluginInterface!.SavePluginConfig(this);
        }
    }
}
