namespace GitVersion
{
    using System.Linq;
    using LibGit2Sharp;

    abstract class DevelopBasedVersionFinderBase
    {
        protected SemanticVersion FindVersion(
            GitVersionContext context,
            BranchType branchType)
        {
            var ancestor = FindCommonAncestorWithDevelop(context.Repository, context.CurrentBranch, branchType);
            
            if (!IsThereAnyCommitOnTheBranch(context.Repository, context.CurrentBranch))
            {
                var developVersionFinder = new DevelopVersionFinder();
                return developVersionFinder.FindVersion(context);
            }

            var versionOnMasterFinder = new VersionOnMasterFinder();
            var versionFromMaster = versionOnMasterFinder.Execute(context, context.CurrentBranch.Tip.Committer.When);

            var numberOfCommitsOnBranchSinceCommit = NumberOfCommitsOnBranchSinceCommit(context.CurrentBranch, ancestor);
            var sha = context.CurrentBranch.Tip.Sha;
            var releaseDate = ReleaseDateFinder.Execute(context.Repository, sha, 0);
            var semanticVersion = new SemanticVersion
            {
                Major = versionFromMaster.Major,
                Minor = versionFromMaster.Minor + 1,
                Patch = 0,
                PreReleaseTag = "unstable0",
                BuildMetaData = new SemanticVersionBuildMetaData(
                    numberOfCommitsOnBranchSinceCommit,
                    context.CurrentBranch.Name, sha,
                    releaseDate.OriginalDate, releaseDate.Date)
            };

            return semanticVersion;
        }

        protected int NumberOfCommitsOnBranchSinceCommit(Branch branch, Commit commit)
        {
            return branch.Commits
                .TakeWhile(x => x != commit)
                .Count();
        }

        Commit FindCommonAncestorWithDevelop(IRepository repo, Branch branch, BranchType branchType)
        {
            var ancestor = repo.Commits.FindCommonAncestor(
                repo.FindBranch("develop").Tip,
                branch.Tip);

            if (ancestor != null)
            {
                return ancestor;
            }

            throw new ErrorException(
                string.Format("A {0} branch is expected to branch off of 'develop'. "
                              + "However, branch 'develop' and '{1}' do not share a common ancestor."
                    , branchType, branch.Name));
        }

        public bool IsThereAnyCommitOnTheBranch(IRepository repo, Branch branch)
        {
            var filter = new CommitFilter
            {
                Since = branch,
                Until = repo.FindBranch("develop")
            };

            var commits = repo.Commits.QueryBy(filter);

            if (!commits.Any())
            {
                return false;
            }

            return true;
        }
    }
}
