using System.Collections.Generic;

namespace Corellian.Versio
{
    public class RepositoryDiffVersion
    {
        public string Hash { get; set; }
        public string DiffHash { get; set; }

        public Dictionary<string, string> Hashs { get; set; }
        public Dictionary<string, string> DiffHashs { get; set; }
    }

}
