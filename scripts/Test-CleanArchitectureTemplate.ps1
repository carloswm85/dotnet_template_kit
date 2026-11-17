function Test-CleanArchitectureTemplate {
    <#
    @desc   Smoke-tests the DTK Clean Architecture template: installs it from
            this repository, generates a solution under a different name,
            then builds it, runs the unit tests and checks that no
            "CleanArchitectureTemplate" text survived the rename.
    @alias  tcat
    .PARAMETER Name
        Solution name for the generated project (replaces the sourceName).
    .PARAMETER OutputRoot
        Folder for the generated solution. Defaults to a new temp folder.
    .PARAMETER Keep
        Keep the generated solution and the template installation.
    .EXAMPLE
        ./scripts/Test-CleanArchitectureTemplate.ps1
    .EXAMPLE
        . ./scripts/Test-CleanArchitectureTemplate.ps1; tcat -Name Acme.Shop -Keep
    #>
    [CmdletBinding()]
    param(
        [string] $Name = 'Acme.Sample',
        [string] $OutputRoot = (Join-Path ([IO.Path]::GetTempPath()) "dtk-smoke-$([guid]::NewGuid().ToString('N'))"),
        [switch] $Keep
    )

    Set-StrictMode -Version Latest
    $ErrorActionPreference = 'Stop'

    $templatePath = Join-Path $PSScriptRoot '..\templates\CleanArchitectureTemplate' | Resolve-Path
    $outputPath = Join-Path $OutputRoot $Name
    $failures = [System.Collections.Generic.List[string]]::new()

    function Invoke-Step([string] $Title, [scriptblock] $Action) {
        Write-Host "==> $Title" -ForegroundColor Cyan
        & $Action
        if ($LASTEXITCODE -ne 0) {
            throw "$Title failed (exit code $LASTEXITCODE)."
        }
    }

    # Reuse an existing installation (for example the repository folder
    # installed with `dotnet new install .`); only uninstall what this script
    # installed itself.
    $listing = dotnet new list dtk-clean-architecture 2>$null | Out-String
    $installedHere = $false

    try {
        if ($listing -notmatch 'dtk-clean-architecture') {
            Invoke-Step 'Install template' { dotnet new install $templatePath }
            $installedHere = $true
        }
        else {
            Write-Host '==> Template already installed; using it' -ForegroundColor Cyan
        }

        Invoke-Step 'Generate solution' {
            dotnet new dtk-clean-architecture --name $Name --output $outputPath
        }

        $solution = Get-ChildItem $outputPath -Filter '*.slnx' | Select-Object -First 1
        if (-not $solution) {
            throw 'No .slnx file was generated.'
        }

        Invoke-Step 'Build' { dotnet build $solution.FullName -c Release }

        $unitTests = Get-ChildItem $outputPath -Recurse -Filter '*.Tests.Unit.csproj' |
            Select-Object -First 1
        if ($unitTests) {
            Invoke-Step 'Unit tests' {
                dotnet run --no-build -c Release --project $unitTests.FullName
            }
        }

        Write-Host '==> Check for leftover template names' -ForegroundColor Cyan
        $leftovers = Get-ChildItem $outputPath -Recurse -File |
            Where-Object { $_.FullName -notmatch '[\\/](bin|obj|lib)[\\/]' } |
            Select-String -Pattern 'CleanArchitectureTemplate' -List
        foreach ($hit in $leftovers) {
            $failures.Add("Leftover name in $($hit.Path):$($hit.LineNumber)")
        }

        $leftoverPaths = Get-ChildItem $outputPath -Recurse |
            Where-Object { $_.Name -match 'CleanArchitectureTemplate' }
        foreach ($item in $leftoverPaths) {
            $failures.Add("Leftover name in path $($item.FullName)")
        }
    }
    finally {
        if ($installedHere -and -not $Keep) {
            dotnet new uninstall $templatePath | Out-Null
        }

        if (-not $Keep) {
            if (Test-Path $OutputRoot) {
                Remove-Item $OutputRoot -Recurse -Force
            }
        }
        else {
            Write-Host "Kept: $outputPath" -ForegroundColor Yellow
        }
    }

    if ($failures.Count -gt 0) {
        $failures | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        throw "$($failures.Count) problem(s) found."
    }

    Write-Host 'Template smoke test passed.' -ForegroundColor Green
}

Set-Alias -Name tcat -Value Test-CleanArchitectureTemplate

# Run directly when the script is executed (not dot-sourced).
if ($MyInvocation.InvocationName -ne '.') {
    Test-CleanArchitectureTemplate @args
}
