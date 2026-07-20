#!/usr/bin/env bash
set -euo pipefail

die() { echo "error: $*" >&2; exit 1; }
info() { echo "==> $*"; }

usage() {
  cat <<'EOF'
Usage: archive-project.sh <repo-url> [options]

Archive a git repository into this monorepo while preserving history.

Arguments:
  repo-url                URL of the repository to archive

Options:
  -n, --name NAME         Project directory name (default: derived from URL)
  -b, --branch BRANCH     Branch to archive (default: source repo default branch)
      --skip-source-push  Do not push the restructure commit to the source repo
      --skip-push         Do not push the archive repo after merging
  -h, --help              Show this help

Examples:
  ./scripts/archive-project.sh git@github.com:user/my-old-project.git
  ./scripts/archive-project.sh https://github.com/user/my-old-project.git -n my-project -b master
EOF
}

repo_url=""
project_name=""
branch=""
push_source=true
push_archive=true

while [[ $# -gt 0 ]]; do
  case "$1" in
    -h|--help)
      usage
      exit 0
      ;;
    -n|--name)
      [[ $# -ge 2 ]] || die "missing value for $1"
      project_name="$2"
      shift 2
      ;;
    -b|--branch)
      [[ $# -ge 2 ]] || die "missing value for $1"
      branch="$2"
      shift 2
      ;;
    --skip-source-push)
      push_source=false
      shift
      ;;
    --skip-push)
      push_archive=false
      shift
      ;;
    -*)
      die "unknown option: $1"
      ;;
    *)
      [[ -z "$repo_url" ]] || die "unexpected argument: $1"
      repo_url="$1"
      shift
      ;;
  esac
done

[[ -n "$repo_url" ]] || { usage; exit 1; }

archive_root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$archive_root"
git rev-parse --is-inside-work-tree >/dev/null 2>&1 || die "archive root is not a git repository: $archive_root"

if [[ -z "$project_name" ]]; then
  project_name="$(basename "$repo_url" .git)"
fi

[[ "$project_name" != *"/"* ]] || die "project name cannot contain slashes: $project_name"
[[ ! -e "$archive_root/$project_name" ]] || die "directory already exists: $project_name"

workdir="$(mktemp -d)"
cleanup() { rm -rf "$workdir"; }
trap cleanup EXIT

info "Cloning $repo_url"
git clone "$repo_url" "$workdir/repo"
cd "$workdir/repo"

if [[ -z "$branch" ]]; then
  branch="$(git symbolic-ref --short HEAD)"
fi
git rev-parse --verify "$branch" >/dev/null 2>&1 || die "branch not found in source repo: $branch"
if [[ "$(git symbolic-ref --short HEAD 2>/dev/null || true)" != "$branch" ]]; then
  git checkout "$branch"
fi

info "Moving files into $project_name/"
mkdir "$project_name"
git ls-tree -z --name-only HEAD | while IFS= read -r -d '' path; do
  [[ "$path" == "$project_name" ]] && continue
  git mv "$path" "$project_name/"
done

if git diff --cached --quiet; then
  die "nothing to archive (repository may already be restructured)"
fi

git commit -m "Archived $project_name"

if $push_source; then
  info "Pushing restructure commit to source repository"
  git push origin "$branch"
fi

source_head="$(git rev-parse HEAD)"

cd "$archive_root"

if git remote get-url "$project_name" >/dev/null 2>&1; then
  info "Fetching existing remote $project_name"
  git fetch "$project_name"
else
  info "Adding remote $project_name"
  git remote add --fetch "$project_name" "$repo_url"
fi

if $push_source; then
  merge_ref="$project_name/$branch"
  git rev-parse --verify "$merge_ref" >/dev/null 2>&1 || die "remote branch not found after fetch: $merge_ref"
else
  merge_ref="$source_head"
fi

info "Merging $merge_ref into archive"
git merge --allow-unrelated-histories "$merge_ref" -m "Merge remote-tracking branch '$project_name/$branch'"

if $push_archive; then
  info "Pushing archive repository"
  git push origin HEAD
fi

info "Archived $project_name successfully"
