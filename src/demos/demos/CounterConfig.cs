using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <Summary>
/// This file contains some utilities commonly used.
/// </Summary>

namespace TFSCodeCounter
{
    public class CounterConfig
    {
        public string Tfs { get; set; }
        public string Project { get; set; }
        public string ServerLocation { get; set; }
        public string ClientLocation { get; set; }
        public string CurrentRevision { get; set; }
        public string PreviousRevision { get; set; }
        public string OutputFile { get; set; }
        public bool IsRemain { get; set; }
    }
}
