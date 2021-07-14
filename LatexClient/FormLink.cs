using System.Dynamic;

namespace LatexClient
{
    public class FormValue
    {
        public string Name { get; set; }
        public string Label { get; set; }

        public bool Required { get; set; }
    }

    public class FormLink
    {
        public string Href { get; set; }

        public string[] Rel { get; set; }

        public string Method { get; set; }

        public FormValue[] Value { get; set; }

    }
}