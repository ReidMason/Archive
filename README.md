# Archive

This repo stores all my old projects but preserves the commit history.

# How to archive a project

## Automated (recommended)

From the archive repo root:

```bash
./scripts/archive-project.sh <old-repo-url>
```

The script will:

1. Clone the source repo to a temporary directory
2. Move all files into a subdirectory named after the project (using `git mv` to preserve history)
3. Commit `Archived <name>`
4. Push that commit back to the source repo
5. Add the source repo as a remote in the archive (if it isn't already)
6. Merge with `--allow-unrelated-histories`
7. Push the archive repo

### Options

| Flag | Purpose |
|------|---------|
| `-n, --name NAME` | Override the directory name (default: derived from the URL) |
| `-b, --branch BRANCH` | Branch to archive (default: the source repo's default branch) |
| `--skip-source-push` | Don't push the restructure commit to the original repo |
| `--skip-push` | Merge locally but don't push the archive |

### Examples

```bash
./scripts/archive-project.sh git@github.com:ReidMason/my-old-project.git
./scripts/archive-project.sh git@github.com:ReidMason/my-old-project.git -b master
./scripts/archive-project.sh git@github.com:ReidMason/my-old-project.git --skip-source-push
```

Most archived projects here use `master`, so you may need `-b master`.

Use `--skip-source-push` if you want to archive a project without modifying the original repo on GitHub — the script merges the restructured commit directly instead.

## Manual

Clone the old repository and move all files into a subdirectory (in the **source** repo):

```bash
git clone <old-repo-url>
mkdir <old-repo-name>
git ls-tree -z --name-only HEAD | xargs -0 -I {} git mv {} "<old-repo-name>"
git commit -m "Archived <old-repo-name>"
git push
```

Add the remote repository to the local repository:

```bash
git remote add --fetch <old-repo-name> <old-repo-url>
```

Merge the old repository into the current repository:

```bash
git merge --allow-unrelated-histories <old-repo>/<branch>
git push
```
