using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoFiler
{
    public class FileName
    {
        public string fileName { get; set; }
        public string fileOption { get; set; } //beginswith, contains, endswith
        public bool doOverride { get; set; }   //files will be autofiled based on filename instead of filetype 
        public string destination { get; set; }
    }
}
