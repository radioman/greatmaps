
cd .\Release-NETv3.5

"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsPresentation.zip Demo.WindowsPresentation.exe
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsPresentation.zip GMap.NET.Core.dll
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsPresentation.zip GMap.NET.Core.xml
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsPresentation.zip GMap.NET.WindowsPresentation.dll
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsPresentation.zip GMap.NET.WindowsPresentation.xml
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsPresentation.zip ..\..\Info\License.txt

"C:\Program Files\7-Zip\7z.exe" t ..\GMap.NET.WindowsPresentation.zip

pause

cd ..\Release-NETv2.0

"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsForms.zip Demo.WindowsForms.exe
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsForms.zip GMap.NET.Core.dll
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsForms.zip GMap.NET.Core.xml
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsForms.zip GMap.NET.WindowsForms.dll
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsForms.zip GMap.NET.WindowsForms.xml
"C:\Program Files\7-Zip\7z.exe" a ..\GMap.NET.WindowsForms.zip ..\..\Info\License.txt

"C:\Program Files\7-Zip\7z.exe" t ..\GMap.NET.WindowsForms.zip

pause
