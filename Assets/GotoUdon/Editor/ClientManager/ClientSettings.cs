namespace GotoUdon.Editor.ClientManager
{
    [System.Serializable]
    public class ClientSettings
    {
        public string name;
        public int profile;
        public int duplicates = 1;
        public bool enabled;
        public bool vr;

        public ClientSettings withDuplicates(int duplicates)
        {
            return new ClientSettings
            {
                name = name,
                profile = profile,
                duplicates = duplicates,
                enabled = enabled,
                vr = vr
            };
        }
    }
}