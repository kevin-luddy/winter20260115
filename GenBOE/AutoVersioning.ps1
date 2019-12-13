# Look for a 0.0 pattern in the build number. 
# Use it to version files that get cached within application.
#
# For example, if the 'Build number format' build process parameter 
# $(BuildDefinitionName)_$(Date:yyyyMMdd)$(Rev:.r)
# then your build numbers come out like this:
# "#BOEDevRMS_20161003.1"
# This script would then use version 20161003.1

# Enable -Verbose option
[CmdletBinding()]

# Regular expression pattern to find the version in the build number 
# and then apply it to the assemblies
$VersionRegex = "\d+\.\d+"

# If this script is not running on a build server, remind user to 
# set environment variables so that this script can be debugged
# $Env:BUILD_SOURCESDIRECTORY = "C:\Users\buckwalj\Source\Workspaces\genBOE\dev\GenBOE\GenBOE.Web"
# $Env:BUILD_BUILDNUMBER = "#BOEDevRMS_20161003.1"

if(-not ($Env:BUILD_SOURCESDIRECTORY -and $Env:BUILD_BUILDNUMBER))
{
    Write-Error "You must set the following environment variables"
    Write-Error "to test this script interactively."
    Write-Host '$Env:BUILD_SOURCESDIRECTORY - For example, enter something like:'
    Write-Host '$Env:BUILD_SOURCESDIRECTORY = "C:\code\FabrikamTFVC\HelloWorld"'
    Write-Host '$Env:BUILD_BUILDNUMBER - For example, enter something like:'
    Write-Host '$Env:BUILD_BUILDNUMBER = "Build HelloWorld_0000.00.00.0"'
    exit 1
}

# Make sure path to source code directory is available
if (-not $Env:BUILD_SOURCESDIRECTORY)
{
    Write-Error ("BUILD_SOURCESDIRECTORY environment variable is missing.")
    exit 1
}
elseif (-not (Test-Path $Env:BUILD_SOURCESDIRECTORY))
{
    Write-Error "BUILD_SOURCESDIRECTORY does not exist: $Env:BUILD_SOURCESDIRECTORY"
    exit 1
}
Write-Verbose "BUILD_SOURCESDIRECTORY: $Env:BUILD_SOURCESDIRECTORY"

# Make sure there is a build number
if (-not $Env:BUILD_BUILDNUMBER)
{
    Write-Error ("BUILD_BUILDNUMBER environment variable is missing.")
    exit 1
}
Write-Verbose "BUILD_BUILDNUMBER: $Env:BUILD_BUILDNUMBER"

# Get and validate the version data
$VersionData = [regex]::matches($Env:BUILD_BUILDNUMBER,$VersionRegex)
switch($VersionData.Count)
{
   0        
      { 
         Write-Error "Could not find version number data in BUILD_BUILDNUMBER."
         exit 1
      }
   1 {}
   default 
      { 
         Write-Warning "Found more than instance of version data in BUILD_BUILDNUMBER." 
         Write-Warning "Will assume first instance is version."
      }
}
$NewVersion = $VersionData[0]
Write-Verbose "Version: $NewVersion"


#Replaces all occurrences of --VERSION-- within each file 
get-childitem -path $Env:BUILD_SOURCESDIRECTORY -recurse -include *.csproj,*.js,*.Master,*.cs,*.aspx,*.ascx,*.css,*.htm,*.html,*.cshtml | 
    foreach-object {
        $filename=$_; 
        set-itemproperty $filename IsReadOnly -value $false;
        
        (Get-Content $filename).replace('--VERSION--', $NewVersion) | Set-Content $filename
    }

#Renames all files containing --VERSION--
get-childitem -path $Env:BUILD_SOURCESDIRECTORY -recurse -include "*--VERSION--*" | 
    foreach-object {
        $oldname = $_;
        $newname = $_ -replace '--VERSION--',$NewVersion;
        move-item -path $oldname -destination $newname;
    }