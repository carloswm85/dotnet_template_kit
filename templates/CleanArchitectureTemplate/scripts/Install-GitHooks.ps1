<#
.SYNOPSIS
    Enables the repository's Git hooks (.githooks) for this clone.
.DESCRIPTION
    Sets the repository-local core.hooksPath to .githooks. Run once per clone.
    See .githooks/README.md.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = git rev-parse --show-toplevel

if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repositoryRoot)) {
    throw 'Run this script from inside the Git repository.'
}

git -C $repositoryRoot config --local core.hooksPath .githooks

if ($LASTEXITCODE -ne 0) {
    throw 'Git could not configure the repository hook path.'
}

Write-Host 'Git hooks enabled from .githooks.'