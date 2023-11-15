using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using LibGit2Sharp;

namespace Corellian.Versio
{
    public static class GitVersion
    {
        public static bool Create(string projectName, string projectDirectory, string outputDirectory)
        {
            var repositoryDiff = GitVersion.Create(projectDirectory);

            if (repositoryDiff == null)
            {
                return false;
            }

            var hashes = new Dictionary<string, string>
            {
                { "Repository", repositoryDiff.Hash }
            };

            var diffHashes = new Dictionary<string, string>
            {
                { "Repository", repositoryDiff.DiffHash }
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

            File.WriteAllText(Path.Combine(outputDirectory, projectName + ".versio.txt"), Assembly.GetExecutingAssembly().GetName().Version.ToString());

            var repositoryDiffVersionFile = Path.Combine(outputDirectory, projectName + ".versio.json");
            File.WriteAllText(repositoryDiffVersionFile, JsonSerializer.Serialize(repositoryDiffVersion));

            if (!string.IsNullOrWhiteSpace(repositoryDiff.Diff))
            {
                File.WriteAllText(Path.Combine(outputDirectory, projectName + ".versio.Repository.diff"), repositoryDiff.Diff);
            }

            foreach (var repositoryDiffSubmodule in repositoryDiff.Submodules)
            {
                File.WriteAllText(Path.Combine(outputDirectory, projectName + ".versio." + repositoryDiffSubmodule.Name + ".diff"), repositoryDiffSubmodule.Diff);
            }

            return true;
        }

        public static RepositoryDiff? Create(string startingPath)
        {
            var gitPath = Repository.Discover(startingPath);

            if (gitPath == null)
            {
                return null;
            }

            var repositoryPath = Directory.GetParent(gitPath)!.Parent!.FullName;

            var repositoryDiff = new RepositoryDiff();

            using (var repository = new Repository(repositoryPath))
            {
                repositoryDiff.Hash = repository.Head.Tip.Id.Sha;

                //using (var changes = repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.WorkingDirectory))
                //{
                //    ;
                //}

                using (var patch = repository.Diff.Compare<Patch>(repository.Head.Tip.Tree, DiffTargets.WorkingDirectory))
                {
                    repositoryDiff.Diff = patch.Content;
                    if (!string.IsNullOrWhiteSpace(repositoryDiff.Diff))
                    {
                        repositoryDiff.DiffHash = Utils.ComputeHash(repositoryDiff.Diff);
                    }
                    else
                    {
                        repositoryDiff.Diff = string.Empty;
                    }
                }

                foreach (var submodule in repository.Submodules)
                {
                    var submoduleRepositoryPath = Path.Combine(repositoryPath, submodule.Path);

                    if (!Repository.IsValid(submoduleRepositoryPath))
                    {
                        continue;
                    }

                    using (var submoduleRepository = new Repository(submoduleRepositoryPath))
                    {
                        var repositoryDiffSubmodule = new RepositoryDiffSubmodule
                        {
                            Name = Path.GetFileName(submoduleRepositoryPath),
                            Hash = submoduleRepository.Head.Tip.Id.Sha
                        };
                        repositoryDiff.Submodules.Add(repositoryDiffSubmodule);

                        using (var patch = submoduleRepository.Diff.Compare<Patch>(submoduleRepository.Head.Tip.Tree, DiffTargets.WorkingDirectory))
                        {
                            repositoryDiffSubmodule.Diff = patch.Content;
                            if (!string.IsNullOrWhiteSpace(repositoryDiffSubmodule.Diff))
                            {
                                repositoryDiffSubmodule.DiffHash = Utils.ComputeHash(repositoryDiffSubmodule.Diff);
                            }
                            else
                            {
                                repositoryDiffSubmodule.Diff = string.Empty;
                            }
                        }
                    }
                }
            }

            repositoryDiff.Submodules = repositoryDiff.Submodules.OrderBy(s => s.Name).ToList();

            return repositoryDiff;
        }
    }
}