using System.Collections.Generic;

namespace Corellian.Versio
{
    public class RepositoryDiff
    {
        public string Hash { get; set; }
        public string Diff { get; set; }
        public string DiffHash { get; set; }

        public List<RepositoryDiffSubmodule> Submodules { get; set; } = new List<RepositoryDiffSubmodule>();
    }
}
