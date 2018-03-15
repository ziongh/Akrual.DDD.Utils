Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }

#Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { echo $_.fullname}