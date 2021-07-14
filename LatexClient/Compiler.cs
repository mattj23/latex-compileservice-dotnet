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
        public static string ToCompilerString(this Compiler c)
        {
            var compilers = new Dictionary<Compiler, string>
            {
                {Compiler.PdfLatex, "pdflatex"},
                {Compiler.XeLatex, "xelatex"},
                {Compiler.LuaLatex, "lualatex"}
            };

            return compilers[c];
        }
    }
}