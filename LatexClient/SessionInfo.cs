namespace LatexClient
{
    public class SessionInfo
    {
        public string Key { get; set; }
        public string Target { get; set; }

        public FormLink AddFile { get; set; }
        public FormLink AddTemplates { get; set; }
        public FormLink Finalize { get; set; }

    }
}