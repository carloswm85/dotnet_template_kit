function Test-CleanArchitectureTemplate {
    <#
    @desc   Smoke-tests the DTK Clean Architecture template: installs it from
            this repository and, for each host mode (API, Web, Both),
            generates a solution under a different name, builds it, runs the
            unit and integration tests (with `dotnet run` and with
            `dotnet test`), checks that only the selected host
            projects exist, and checks that no "CleanArchitectureTemplate"
            text survived the rename.
    @alias  tcat
    .PARAMETER Name
        Solution name for the generated project (replaces the sourceName).
    .PARAMETER Hosts
        Host modes to test. Defaults to all three: API, Web and Both.
    .PARAMETER OutputRoot
        Folder for the generated solutions. Defaults to a new temp folder.
    .PARAMETER SkipIntegrationTests
        Skip the integration tests (they need LocalDB on Windows, or Docker).
    .PARAMETER Keep
        Keep the generated solutions and the template installation.
    .EXAMPLE
        ./scripts/Test-CleanArchitectureTemplate.ps1
    .EXAMPLE
        . ./scripts/Test-CleanArchitectureTemplate.ps1
        tcat -Hosts Web -Name Acme.Shop -Keep
    #>
    [CmdletBinding()]
    param(
        [string] $Name = 'Acme.Sample',
        [ValidateSet('API', 'Web', 'Both')]
        [string[]] $Hosts = @('API', 'Web', 'Both'),
        [string] $OutputRoot = (Join-Path ([IO.Path]::GetTempPath()) `
                "dtk-smoke-$([guid]::NewGuid().ToString('N'))"),
        [switch] $SkipIntegrationTests,
        [switch] $Keep
    )

    Set-StrictMode -Version Latest
    $ErrorActionPreference = 'Stop'

    $templatePath = Join-Path $PSScriptRoot '..\templates\CleanArchitectureTemplate' |
        Resolve-Path
    $failures = [System.Collections.Generic.List[string]]::new()

    function Invoke-Step([string] $Title, [scriptblock] $Action) {
        Write-Host "==> $Title" -ForegroundColor Cyan
        & $Action
        if ($LASTEXITCODE -ne 0) {
            throw "$Title failed (exit code $LASTEXITCODE)."
        }
    }

    function Test-HostLayout([string] $Mode, [string] $OutputPath) {
        $expected = @{
            API  = @{ API = $true; Web = $false }
            Web  = @{ API = $false; Web = $true }
            Both = @{ API = $true; Web = $true }
        }[$Mode]

        $solutionText = Get-Content (Join-Path $OutputPath "$Name.slnx") -Raw

        foreach ($hostName in 'API', 'Web') {
            $project = "$Name.$hostName"
            $exists = Test-Path (Join-Path $OutputPath "solution\$project")
            $inSolution = $solutionText -match [regex]::Escape("$project.csproj")

            if ($exists -ne $expected[$hostName]) {
                $failures.Add("[$Mode] Project folder $project exists: $exists")
            }
            if ($inSolution -ne $expected[$hostName]) {
                $failures.Add("[$Mode] $project in solution: $inSolution")
            }
        }

        # C# files are skipped: `#if` there is a compiler directive.
        $leftoverDirectives = Get-ChildItem $OutputPath -Recurse -File |
            Where-Object {
                $_.FullName -notmatch '[\\/](bin|obj|lib)[\\/]' -and
                $_.Extension -ne '.cs'
            } |
            Select-String -Pattern '^\s*(//|<!--)?\s*#(if|elseif|else|endif)\b' -List
        foreach ($hit in $leftoverDirectives) {
            $failures.Add(
                "[$Mode] Template directive left in $($hit.Path):$($hit.LineNumber)")
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

        foreach ($mode in $Hosts) {
            Write-Host "`n### Hosts = $mode" -ForegroundColor Magenta
            $outputPath = Join-Path $OutputRoot "$mode\$Name"

            Invoke-Step "[$mode] Generate solution" {
                dotnet new dtk-clean-architecture --name $Name `
                    --output $outputPath --Hosts $mode
            }

            $solution = Get-ChildItem $outputPath -Filter '*.slnx' |
                Select-Object -First 1
            if (-not $solution) {
                throw "[$mode] No .slnx file was generated."
            }

            Write-Host "==> [$mode] Check host projects" -ForegroundColor Cyan
            Test-HostLayout $mode $outputPath

            Invoke-Step "[$mode] Build" { dotnet build $solution.FullName -c Release }

            # xUnit v3 test projects are executables; `dotnet run` executes
            # every test and fails when one fails.
            $testProjects = @(Get-ChildItem $outputPath -Recurse -Filter '*.Tests.Unit.csproj')
            if (-not $SkipIntegrationTests) {
                $testProjects += Get-ChildItem $outputPath -Recurse `
                    -Filter '*.Tests.Integration.csproj'
            }
            foreach ($project in $testProjects) {
                Invoke-Step "[$mode] $($project.BaseName)" {
                    dotnet run --no-build -c Release --project $project.FullName
                }
            }

            # `dotnet test` must discover and run the same tests through the
            # VSTest adapter; finding none is an error, not a silent pass.
            foreach ($project in $testProjects) {
                Invoke-Step "[$mode] dotnet test $($project.BaseName)" {
                    dotnet test $project.FullName --no-build -c Release `
                        -- RunConfiguration.TreatNoTestsAsError=true
                }
            }

            Write-Host "==> [$mode] Check for leftover template names" `
                -ForegroundColor Cyan
            $leftovers = Get-ChildItem $outputPath -Recurse -File |
                Where-Object { $_.FullName -notmatch '[\\/](bin|obj|lib)[\\/]' } |
                Select-String -Pattern 'CleanArchitectureTemplate' -List
            foreach ($hit in $leftovers) {
                $failures.Add("[$mode] Leftover name in $($hit.Path):$($hit.LineNumber)")
            }

            $leftoverPaths = Get-ChildItem $outputPath -Recurse |
                Where-Object { $_.Name -match 'CleanArchitectureTemplate' }
            foreach ($item in $leftoverPaths) {
                $failures.Add("[$mode] Leftover name in path $($item.FullName)")
            }
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
            Write-Host "Kept: $OutputRoot" -ForegroundColor Yellow
        }
    }

    if ($failures.Count -gt 0) {
        $failures | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        throw "$($failures.Count) problem(s) found."
    }

    Write-Host "Template smoke test passed for: $($Hosts -join ', ')." `
        -ForegroundColor Green
}

Set-Alias -Name tcat -Value Test-CleanArchitectureTemplate

# Run directly when the script is executed (not dot-sourced).
if ($MyInvocation.InvocationName -ne '.') {
    Test-CleanArchitectureTemplate @args
}
