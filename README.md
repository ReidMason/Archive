# Archive

This repo stores all my old projects but preserves the commit history.

# How to archive a project

Clone the old repository and move all files into a subdirectory:

```bash
git clone <old-repo-url>
mkdir <old-repo-name>
git ls-tree -z --name-only HEAD | xargs -0 -I {} git mv {} <old-repo-name>
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
