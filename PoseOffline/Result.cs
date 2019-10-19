using System;
using System.Collections.Generic;
using System.Text;

namespace PoseOffline
{
    class Result
    {
        public int PageCount { get; set; }
        public int WriteEntry { get; set; }
        public int ErrorEntry { get; set; }
        public IEnumerable<string> ErrorMessages { get; set; }
    }
}
