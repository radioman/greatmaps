
set msbuildexe=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe

set builddir=Release-NET%TargetFrameworkVersion4%
mkdir .\%builddir% 
del /q /s .\%builddir%\*.*

%msbuildexe% /fl /flp:LogFile=build.log;Append;errorsonly /nologo /p:WarningLevel=0;Optimize=True;Platform=AnyCPU;TargetFrameworkVersion=v4.0 /clp:Verbosity=m; /t:Rebuild /p:Configuration=Release ..\Demo.WindowsForms\Demo.WindowsForms.csproj

%msbuildexe% /fl /flp:LogFile=build.log;Append;errorsonly /nologo /p:WarningLevel=0;Optimize=True;Platform=AnyCPU;TargetFrameworkVersion=v4.0 /clp:Verbosity=m; /t:Rebuild /p:Configuration=Release ..\Demo.WindowsPresentation\Demo.WindowsPresentation.csproj

del /q /s .\%builddir%\*.application
del /q /s .\%builddir%\*.exe.manifest

if "%1"=="nopause" goto end
pause
:end