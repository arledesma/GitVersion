﻿namespace AcceptanceTests
{
    using System;
    using System.IO;
    using Helpers;
    using LibGit2Sharp;

    public class EmptyRepositoryFixture : IDisposable
    {
        public readonly string RepositoryPath;
        public readonly Repository Repository;

        public EmptyRepositoryFixture()
        {
            RepositoryPath = PathHelper.GetTempPath();
            Repository.Init(RepositoryPath);
            Console.WriteLine("Created git repository at {0}", RepositoryPath);

            Repository = new Repository(RepositoryPath);
            Repository.Config.Set("user.name", "Test");
            Repository.Config.Set("user.email", "test@email.com");

            var randomFile = Path.Combine(Repository.Info.WorkingDirectory, Guid.NewGuid().ToString());
            File.WriteAllText(randomFile, string.Empty);
            Repository.Index.Stage(randomFile);
            Repository.Commit("Initial Commit", new Signature("Test User", "test@email.com", DateTimeOffset.UtcNow));
        }

        public void Dispose()
        {
            Repository.Dispose();
            try
            {
                //Directory.Delete(RepositoryPath, true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to clean up repository path at {0}. Received exception: {1}", RepositoryPath, e.Message);
            }
        }

        public ExecutionResults ExecuteGitVersion()
        {
            return GitVersionHelper.ExecuteIn(RepositoryPath);
        }
    }
}
