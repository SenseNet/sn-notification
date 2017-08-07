$srcPath = [System.IO.Path]::GetFullPath(($PSScriptRoot + '\..\..\src'))
$installPackageFolder = "$srcPath\nuget\content\Admin\tools"
$installPackagePath = "$installPackageFolder\install-notification.zip"
$scriptsSourcePath = "$srcPath\Notification\Data\Scripts"
$scriptsTargetPath = "$srcPath\nuget\snadmin\install-notification\scripts"

# delete existing packages
Remove-Item $PSScriptRoot\*.nupkg

if (!(Test-Path $installPackageFolder))
{
	New-Item $installPackageFolder -Force -ItemType Directory
}

New-Item $scriptsTargetPath -ItemType directory -Force

Copy-Item $scriptsSourcePath\Install_Notification.sql $scriptsTargetPath -Force

Compress-Archive -Path "$srcPath\nuget\snadmin\install-notification\*" -Force -CompressionLevel Optimal -DestinationPath $installPackagePath

nuget pack $srcPath\Notification\Notification.nuspec -properties Configuration=Release -OutputDirectory $PSScriptRoot
nuget pack $srcPath\Notification\Notification.Install.nuspec -properties Configuration=Release -OutputDirectory $PSScriptRoot
nuget pack $srcPath\Notification.Portlets\Notification.Portlets.nuspec -properties Configuration=Release -OutputDirectory $PSScriptRoot
