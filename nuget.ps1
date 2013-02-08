del TsSoft.OpenAuth.*.nupkg
del *.nuspec
del .\TsSoft.OpenAuth\bin\Release\*.nuspec

function GetNodeValue([xml]$xml, [string]$xpath)
{
	return $xml.SelectSingleNode($xpath).'#text'
}

function SetNodeValue([xml]$xml, [string]$xpath, [string]$value)
{
	$node = $xml.SelectSingleNode($xpath)
	if ($node) {
		$node.'#text' = $value
	}
}

Remove-Item .\TsSoft.OpenAuth\bin -Recurse 
Remove-Item .\TsSoft.OpenAuth\obj -Recurse 

$build = "c:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe ""TsSoft.OpenAuth\TsSoft.OpenAuth.csproj"" /p:Configuration=Release" 
Invoke-Expression $build

$Artifact = (resolve-path ".\TsSoft.OpenAuth\bin\Release\TsSoft.OpenAuth.dll").path

nuget spec -F -A $Artifact

Copy-Item .\TsSoft.OpenAuth.nuspec.xml .\TsSoft.OpenAuth\bin\Release\TsSoft.OpenAuth.nuspec

$GeneratedSpecification = (resolve-path ".\TsSoft.OpenAuth.nuspec").path
$TargetSpecification = (resolve-path ".\TsSoft.OpenAuth\bin\Release\TsSoft.OpenAuth.nuspec").path

[xml]$srcxml = Get-Content $GeneratedSpecification
[xml]$destxml = Get-Content $TargetSpecification
$value = GetNodeValue $srcxml "//version"
SetNodeValue $destxml "//version" $value;
$value = GetNodeValue $srcxml "//description"
SetNodeValue $destxml "//description" $value;
$value = GetNodeValue $srcxml "//copyright"
SetNodeValue $destxml "//copyright" $value;
$destxml.Save($TargetSpecification)


nuget pack $TargetSpecification

del *.nuspec
del .\TsSoft.OpenAuth\bin\Release\*.nuspec

# exit
