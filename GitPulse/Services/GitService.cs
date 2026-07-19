using System.IO;
using GitPulse.Models;
using LibGit2Sharp;

namespace GitPulse.Services;

public class GitService
{
    private Repository? _repo;

    public bool IsRepoOpen => _repo is not null;

    public string? RepoPath => _repo?.Info?.WorkingDirectory;

    public void OpenRepo(string path)
    {
        _repo?.Dispose();
        _repo = new Repository(path);
        EnsureGitConfig();
    }

    public void EnsureGitConfig()
    {
        if (_repo is null) return;
        var localName = _repo.Config.Get<string>("user.name", ConfigurationLevel.Local);
        var localEmail = _repo.Config.Get<string>("user.email", ConfigurationLevel.Local);
        if (localName is not null && localEmail is not null) return;
        var globalName = _repo.Config.Get<string>("user.name", ConfigurationLevel.Global);
        var globalEmail = _repo.Config.Get<string>("user.email", ConfigurationLevel.Global);
        if (localName is null && globalName?.Value is not null)
            _repo.Config.Set("user.name", globalName.Value, ConfigurationLevel.Local);
        if (localEmail is null && globalEmail?.Value is not null)
            _repo.Config.Set("user.email", globalEmail.Value, ConfigurationLevel.Local);
    }

    public bool HasUserConfig()
    {
        if (_repo is null) return false;
        var name = _repo.Config.Get<string>("user.name");
        var email = _repo.Config.Get<string>("user.email");
        return name is not null && email is not null;
    }

    public void SetLocalUserConfig(string name, string email)
    {
        if (_repo is null) return;
        _repo.Config.Set("user.name", name, ConfigurationLevel.Local);
        _repo.Config.Set("user.email", email, ConfigurationLevel.Local);
    }

    public Task CloneAsync(string url, string path)
    {
        return Task.Run(() =>
        {
            try
            {
                Repository.Clone(url, path);
                OpenRepo(path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Clone failed: {ex.Message}", ex);
            }
        });
    }

    public RepositoryState GetState()
    {
        try
        {
            if (_repo is null) return new RepositoryState();
            return new RepositoryState
            {
                IsOpen = true,
                CurrentBranch = _repo.Head?.FriendlyName ?? "(detached)",
                RepoPath = _repo.Info?.WorkingDirectory ?? ""
            };
        }
        catch
        {
            return new RepositoryState { IsOpen = true };
        }
    }

    public List<BranchInfo> GetBranches()
    {
        try
        {
            if (_repo is null) return [];
            var result = new List<BranchInfo>();
            var current = _repo.Head?.FriendlyName ?? "";
            foreach (var b in _repo.Branches)
            {
                if (b.IsRemote) continue;
                result.Add(new BranchInfo
                {
                    Name = b.FriendlyName,
                    IsCurrent = b.FriendlyName == current,
                    IsRemote = false,
                    TipSha = b.Tip?.Sha ?? ""
                });
            }
            return result;
        }
        catch
        {
            return [];
        }
    }

    public void SwitchBranch(string name)
    {
        try
        {
            if (_repo is null) return;
            Commands.Checkout(_repo, name);
        }
        catch
        {
            // handled by caller
        }
    }

    public List<CommitInfo> GetCommits(int count = 100)
    {
        try
        {
            if (_repo is null) return [];
            var result = new List<CommitInfo>();
            var branchTips = GetBranchTipMap();

            foreach (var commit in _repo.Commits.Take(count))
            {
                var branchNames = new List<string>();
                if (branchTips.TryGetValue(commit.Sha, out var names))
                {
                    branchNames.AddRange(names);
                }

                result.Add(new CommitInfo
                {
                    Sha = commit.Sha,
                    Author = commit.Author.Name,
                    Message = commit.MessageShort,
                    AuthorTime = commit.Author.When,
                    ParentShas = commit.Parents.Select(p => p.Sha).ToList(),
                    BranchNames = branchNames
                });
            }
            return result;
        }
        catch
        {
            return [];
        }
    }

    public List<FileChange> GetStatus()
    {
        try
        {
            if (_repo is null) return [];
            var result = new List<FileChange>();

            foreach (var entry in _repo.RetrieveStatus())
            {
                var s = entry.State;
                if (s.HasFlag(LibGit2Sharp.FileStatus.NewInWorkdir))
                    result.Add(new FileChange { FilePath = entry.FilePath, Status = Models.FileStatus.Untracked, IsStaged = false });
                if (s.HasFlag(LibGit2Sharp.FileStatus.ModifiedInWorkdir))
                    result.Add(new FileChange { FilePath = entry.FilePath, Status = Models.FileStatus.Modified, IsStaged = false });
                if (s.HasFlag(LibGit2Sharp.FileStatus.NewInIndex))
                    result.Add(new FileChange { FilePath = entry.FilePath, Status = Models.FileStatus.Added, IsStaged = true });
                if (s.HasFlag(LibGit2Sharp.FileStatus.ModifiedInIndex))
                    result.Add(new FileChange { FilePath = entry.FilePath, Status = Models.FileStatus.Staged, IsStaged = true });
                if (s.HasFlag(LibGit2Sharp.FileStatus.DeletedFromWorkdir))
                    result.Add(new FileChange { FilePath = entry.FilePath, Status = Models.FileStatus.Deleted, IsStaged = false });
            }
            return result;
        }
        catch
        {
            return [];
        }
    }

    public void Stage(string filePath)
    {
        try
        {
            if (_repo is null) return;
            Commands.Stage(_repo, filePath);
        }
        catch { }
    }

    public void Unstage(string filePath)
    {
        try
        {
            if (_repo is null) return;
            Commands.Unstage(_repo, filePath);
        }
        catch { }
    }

    public (bool Success, string ErrorMessage) Commit(string message)
    {
        try
        {
            if (_repo is null) return (false, "No repository open.");
            EnsureGitConfig();
            var author = _repo.Config.BuildSignature(DateTimeOffset.Now);
            if (author is null) return (false, "Git user name and email are not configured.");
            if (!_repo.RetrieveStatus().IsDirty)
                return (false, "Nothing to commit. Stage your changes first.");
            _repo.Commit(message, author, author);
            return (true, "");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public string GetDiff(string filePath, bool includeUntrackedContent = false)
    {
        try
        {
            if (_repo is null) return "";

            // Untracked files have no blob in HEAD/index to compare against,
            // so synthesize an all-additions diff from the working copy.
            if (includeUntrackedContent)
            {
                var synthesized = ReadUntrackedAsAdditions(filePath);
                if (synthesized is not null) return synthesized;
            }

            var oldTree = _repo.Head?.Tip?.Tree;
            if (oldTree is null) return "";
            var patch = _repo.Diff.Compare<Patch>(oldTree,
                DiffTargets.Index | DiffTargets.WorkingDirectory,
                new[] { filePath });
            return patch?.Content ?? "";
        }
        catch
        {
            return "";
        }
    }

    private string? ReadUntrackedAsAdditions(string filePath)
    {
        try
        {
            var workdir = _repo?.Info?.WorkingDirectory;
            if (string.IsNullOrEmpty(workdir)) return null;
            var fullPath = Path.Combine(workdir, filePath);
            if (!File.Exists(fullPath)) return null;

            var info = new FileInfo(fullPath);
            if (info.Length > 1_048_576) return "File too large to preview (1 MB limit)";

            const int maxLines = 5000;
            var sb = new System.Text.StringBuilder();
            using var reader = new StreamReader(fullPath);
            string? line;
            var count = 0;
            while ((line = reader.ReadLine()) is not null)
            {
                if (line.Contains('\0')) return "Binary file - no preview available";
                if (count++ >= maxLines)
                {
                    sb.AppendLine($"... (truncated after {maxLines} lines)");
                    break;
                }
                sb.Append('+').AppendLine(line);
            }
            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }

    public Task PullAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                if (_repo is null) return;
                var signature = _repo.Config.BuildSignature(DateTimeOffset.Now);
                if (signature is null) return;
                Commands.Pull(_repo, signature, new PullOptions
                {
                    FetchOptions = new FetchOptions()
                });
            }
            catch { }
        });
    }

    public Task PushAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                if (_repo is null) return;
                var pushRef = _repo.Head?.CanonicalName;
                if (string.IsNullOrEmpty(pushRef)) return;
                _repo.Network.Push(_repo.Network.Remotes["origin"], pushRef);
            }
            catch { }
        });
    }

    public Task FetchAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                if (_repo is null) return;
                var remote = _repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(r => r.Specification);
                Commands.Fetch(_repo, remote.Name, refSpecs, new FetchOptions(), "");
            }
            catch { }
        });
    }

    public void CreateBranch(string name)
    {
        try
        {
            if (_repo is null) return;
            _repo.CreateBranch(name);
        }
        catch { }
    }

    public void DeleteBranch(string name)
    {
        try
        {
            if (_repo is null) return;
            _repo.Branches.Remove(name);
        }
        catch { }
    }

    public string MergeBranch(string branchName)
    {
        try
        {
            if (_repo is null) return "No repo open";
            var signature = _repo.Config.BuildSignature(DateTimeOffset.Now);
            if (signature is null) return "No git config";
            var branch = _repo.Branches[branchName];
            if (branch is null) return "Branch not found";
            var result = _repo.Merge(branch, signature);
            return result.Status.ToString();
        }
        catch (Exception ex)
        {
            return $"Merge failed: {ex.Message}";
        }
    }

    public List<string> GetConflicts()
    {
        try
        {
            if (_repo is null) return [];
            var result = new List<string>();
            foreach (var conflict in _repo.Index.Conflicts)
            {
                result.Add(conflict.Ancestor?.Path ?? "");
            }
            return result;
        }
        catch
        {
            return [];
        }
    }

    public void ResolveConflict(string filePath, string resolution)
    {
        try
        {
            if (_repo is null) return;
            switch (resolution)
            {
                case "ours":
                    _repo.CheckoutPaths("HEAD", new[] { filePath }, new CheckoutOptions());
                    break;
                case "theirs":
                    _repo.CheckoutPaths("MERGE_HEAD", new[] { filePath }, new CheckoutOptions());
                    break;
            }
            Commands.Stage(_repo, filePath);
        }
        catch { }
    }

    public List<CommitInfo> SearchCommits(string query)
    {
        try
        {
            if (_repo is null || string.IsNullOrWhiteSpace(query)) return [];
            var result = new List<CommitInfo>();
            var branchTips = GetBranchTipMap();
            var lower = query.ToLowerInvariant();

            foreach (var commit in _repo.Commits)
            {
                var matches = commit.MessageShort.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
                              commit.Author.Name.Contains(lower, StringComparison.OrdinalIgnoreCase);
                if (!matches) continue;

                var branchNames = new List<string>();
                if (branchTips.TryGetValue(commit.Sha, out var names))
                    branchNames.AddRange(names);

                result.Add(new CommitInfo
                {
                    Sha = commit.Sha,
                    Author = commit.Author.Name,
                    Message = commit.MessageShort,
                    AuthorTime = commit.Author.When,
                    ParentShas = commit.Parents.Select(p => p.Sha).ToList(),
                    BranchNames = branchNames
                });

                if (result.Count >= 100) break;
            }
            return result;
        }
        catch
        {
            return [];
        }
    }

    private Dictionary<string, List<string>> GetBranchTipMap()
    {
        try
        {
            var map = new Dictionary<string, List<string>>();
            if (_repo is null) return map;
            foreach (var b in _repo.Branches)
            {
                if (b.IsRemote || b.Tip is null) continue;
                if (!map.ContainsKey(b.Tip.Sha))
                    map[b.Tip.Sha] = [];
                map[b.Tip.Sha].Add(b.FriendlyName);
            }
            return map;
        }
        catch
        {
            return [];
        }
    }

    public void Dispose()
    {
        try
        {
            _repo?.Dispose();
        }
        catch { }
        _repo = null;
    }
}
