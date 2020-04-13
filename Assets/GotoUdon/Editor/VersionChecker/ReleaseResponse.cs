namespace GotoUdon.Editor
{
    public class ReleaseResponse
    {
        public string Error { get; set; }
        public ReleaseInfo ReleaseInfo { get; set; }
        public bool IsError => Error != null;
    }
}