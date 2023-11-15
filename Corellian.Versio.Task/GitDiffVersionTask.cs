using Microsoft.Build.Framework;


namespace Corellian.Versio.Task
{
    public class GitDiffVersionTask : ContextAwareTask
    {
        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string ProjectDirectory { get; set; }

        [Required]
        public string OutputDirectory { get; set; }

        protected override bool ExecuteInner()
        {
            var repositoryDiff = GitVersion.Create(ProjectName, ProjectDirectory, OutputDirectory);

            if (repositoryDiff == false)
            {
                Log.LogWarning($"{ProjectDirectory} is not within a git repository.");

                return true;
            }

            Log.LogMessage($"{ProjectDirectory} finished creating Versio file.");

            return true;
        }
    }
}
