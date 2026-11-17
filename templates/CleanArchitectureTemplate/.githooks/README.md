# Git staging normalization

> Keeps every committed text file in UTF-8 without BOM and with LF line
> endings.

---

## (s1) Rules

| # | Control | Purpose |
| --- | --- | --- |
| `r1` | `.gitattributes` | Stores text files with LF line endings in Git. |
| `r2` | `.editorconfig` | Makes editors save UTF-8 without BOM, with LF. |
| `r3` | `.githooks/pre-commit` | Rejects staged text with a UTF-8 BOM or non-LF line endings. |

The hook reads staged blobs from the Git index, so unstaged changes do not
affect the result. Binary files are skipped. EF Core migrations under
`solution/CleanArchitectureTemplate.Infrastructure/Migrations/` may keep a
UTF-8 BOM, as declared by the `allow-utf8-bom` attribute in `.gitattributes`.

## (s2) Enable the hook

Run the installer once in each clone:

```powershell
./scripts/Install-GitHooks.ps1
```

It sets the repository-local `core.hooksPath` to `.githooks`.

## (s3) Normalize tracked files

After changing `.gitattributes`, renormalize the tracked files and review
the result before committing:

```bash
git add --renormalize .
git diff --cached --stat
```