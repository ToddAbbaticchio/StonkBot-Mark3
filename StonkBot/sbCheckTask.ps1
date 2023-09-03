$sbPath = "C:\users\tabba\desktop\StonkBot-Mark3.exe"

$sbProcess = Get-Process | Where { $_.Name -eq "StonkBot.exe" }

if (!$sbProcess) {
    Start-Process -FilePath $sbPath
}