namespace GotoUdon.Editor
{
    public class ReleaseAsset
    {
        public string Name { get; set; }
        public string DownloadUrl { get; set; }
        public bool IsUnityPackage => Name.EndsWith(".unitypackage");

        public string AsFileName(string path)
        {
            return path.EndsWith("/") ? path + Name : path + "/" + Name;
        }
    }
}