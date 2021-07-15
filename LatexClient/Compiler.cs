using System.Collections.Generic;

namespace LatexClient
{
    public enum Compiler
    {
        PdfLatex,
        XeLatex,
        LuaLatex
    }

    public static class CompilerTypeExtensions
    {
        private static readonly Dictionary<Compiler, string> _compilers = new Dictionary<Compiler, string>
        {
            {Compiler.PdfLatex, "pdflatex"},
            {Compiler.XeLatex, "xelatex"},
            {Compiler.LuaLatex, "lualatex"}
        };

        public static string ToCompilerString(this Compiler c)
        {
            return _compilers[c];
        }
    }
}