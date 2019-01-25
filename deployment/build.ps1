function ZipFile
{
	param(
		[String]$sourceFile,
		[String]$zipFile
	)

	set-alias sz "C:\Program Files (x86)\7-Zip\7z.exe"  
	sz a -tzip -r $zipFile $sourceFile | Out-Null
}

$root = $PSScriptRoot
$source = $root.Replace("deployment", "") + "\source"
$version = Read-Host -Prompt "What version are we building?"

# build for mongo db 
Write-Host "Building SqlServerReportRunner version $version"
$binfolder = "$source\SqlServerReportRunner\bin\Release"
[system.io.file]::Delete("$binfolder\Connections.config")
$zip = "$root\Output\SqlServerReportRunner_v$version.zip"
[system.io.file]::Delete($zip)
ZipFile -sourcefile "$binfolder\*.*" -zipfile $zip 

Write-Host "Done" -BackgroundColor Green -ForegroundColor White

