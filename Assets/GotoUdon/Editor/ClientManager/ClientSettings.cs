using System;

namespace GotoUdon.Editor.ClientManager
{
    [Serializable]
    public class ClientSettings
    {
        public string name;
        public int profile;
        public int instances = 1;
        public bool enabled;
        public bool vr;

        public ClientSettings WithInstances(int instances)
        {
            return new ClientSettings
            {
                name = name,
                profile = profile,
                instances = instances,
                enabled = enabled,
                vr = vr
            };
        }
    }
}