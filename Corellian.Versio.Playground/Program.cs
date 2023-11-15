using System.Collections.Generic;
using System.Linq;

namespace Corellian.Versio.Playground
{
    public class Program
    {
        static void Main(string[] args)
        {
            var repositoryDiff = GitVersion.Create("D:\\Source\\GitHub\\Versio\\Corellian.Versio.MsBuild");

            if (repositoryDiff == null)
            {
                return;
            }

            var hashes = new Dictionary<string, string>
            {
                { "Repository", repositoryDiff.Hash }
            };

            var diffHashes = new Dictionary<string, string>
            {
                { "Repository", repositoryDiff.Hash }
            };

            foreach (var repositoryDiffSubmodule in repositoryDiff.Submodules)
            {
                hashes.Add(repositoryDiffSubmodule.Name, repositoryDiffSubmodule.Hash);
                diffHashes.Add(repositoryDiffSubmodule.Name, repositoryDiffSubmodule.DiffHash);
            }

            var repositoryDiffVersion = new RepositoryDiffVersion
            {
                Hash = Utils.ComputeHash(string.Join(",", new List<string> { repositoryDiff.Hash }.Concat(repositoryDiff.Submodules.Select(s => s.Hash)))),
                DiffHash = Utils.ComputeHash(string.Join(",", new List<string> { repositoryDiff.DiffHash }.Concat(repositoryDiff.Submodules.Select(s => s.DiffHash)))),
                Hashs = hashes,
                DiffHashs = diffHashes
            };
        }
    }
}