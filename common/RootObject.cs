using System.Collections.Generic;

namespace json_for_csharp.common
{
    public class RootObject
    {
        public int ResultCount { get; set; }
        public IList<Result> Results { get; set; }
    }
}
