namespace LatexClient
{
    public class SessionInfo
    {
        public string Key { get; set; }
        public string Target { get; set; }

        public FormLinkInfo AddFile { get; set; }
        public FormLinkInfo AddTemplates { get; set; }
        public FormLinkInfo Finalize { get; set; }

        public LinkInfo Product { get; set; }
        public LinkInfo Log { get; set; }

        public string Status { get; set; }

        public bool IsFinished => this.Status == "success" || this.Status == "error";

    }
}