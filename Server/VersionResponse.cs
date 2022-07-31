namespace Structur.Server
{
    internal class VersionResponse
    {
        public int Major { get; set; }

        public int Minor { get; set; }

        public int Build { get; set; }

        public string KeyHash { get; set; }

        public int KeyCount { get; set; }
    }
}