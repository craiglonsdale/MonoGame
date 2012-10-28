using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoMGFX
{
    public class Options
    {
        [Utilities.CommandLineParser.Required]
        public string Source;

        [Utilities.CommandLineParser.Required]
        public string Output = string.Empty;

        [Utilities.CommandLineParser.Name("DX11")]
        public bool DX11Profile;

        [Utilities.CommandLineParser.Name("DEBUG")]
        public bool Debug;

        [Utilities.CommandLineParser.Name("ISFILE")]
        public bool IsFile;

        [Utilities.CommandLineParser.Name("ISFOLDER")]
        public bool IsFolder;
    }
}
