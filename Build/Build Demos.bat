set netver=v4.0.30319
rem set netver=v3.5

set TargetFrameworkVersion4=v4.0
set TargetFrameworkVersion3=v3.5
set TargetFrameworkVersion2=v2.0

set msbuildexe=%WINDIR%\Microsoft.NET\Framework\%netver%\msbuild.exe

mkdir .\Release
 
del /q /s .\Release\*.*

%msbuildexe% /nologo /p:WarningLevel=0;Optimize=True;Platform=AnyCPU;TargetFrameworkVersion=%TargetFrameworkVersion2% /clp:Verbosity=m; /t:Rebuild /p:Configuration=Release ..\Demo.WindowsForms\Demo.WindowsForms.csproj

%msbuildexe% /nologo /p:WarningLevel=0;Optimize=True;Platform=AnyCPU;TargetFrameworkVersion=%TargetFrameworkVersion3% /clp:Verbosity=m; /t:Rebuild /p:Configuration=Release ..\Demo.WindowsPresentation\Demo.WindowsPresentation.csproj

del /q /s .\Release\*.application
del /q /s .\Release\*.exe.manifest

pause