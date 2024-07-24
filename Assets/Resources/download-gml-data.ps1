# Showing progress bar slows down the download immensely
$ProgressPreference = 'SilentlyContinue'
Write-Host "Starting download... 0/6"
Invoke-WebRequest https://download1.bayernwolke.de/a/lod2/citygml/690_5334.gml -OutFile ./gml-data/690_5334.txt
Write-Host "1/6"
Invoke-WebRequest https://download1.bayernwolke.de/a/lod2/citygml/692_5334.gml -OutFile ./gml-data/692_5334.txt
Write-Host "2/6"
Invoke-WebRequest https://download1.bayernwolke.de/a/lod2/citygml/692_5336.gml -OutFile ./gml-data/692_5336.txt
Write-Host "3/6"
Invoke-WebRequest https://download1.bayernwolke.de/a/lod2/citygml/690_5336.gml -OutFile ./gml-data/690_5336.txt
Write-Host "4/6"
Invoke-WebRequest https://download1.bayernwolke.de/a/lod2/citygml/690_5338.gml -OutFile ./gml-data/690_5338.txt
Write-Host "5/6"
Invoke-WebRequest https://download1.bayernwolke.de/a/lod2/citygml/692_5338.gml -OutFile ./gml-data/692_5338.txt
Write-Host "6/6"