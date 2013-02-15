
rem set msbuildexe2=%WINDIR%\Microsoft.NET\Framework\v2.0.50727\msbuild.exe
set msbuildexe3=%WINDIR%\Microsoft.NET\Framework\v3.5\msbuild.exe
rem set msbuildexe=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe

set builddir2=Release-NETv2.0
mkdir .\%builddir2% 
del /q /s .\%builddir2%\*.*

set builddir3=Release-NETv3.5
mkdir .\%builddir3% 
del /q /s .\%builddir3%\*.*

%msbuildexe3% /nologo /p:WarningLevel=0;Optimize=True;Platform=AnyCPU;TargetFrameworkVersion=v2.0 /clp:Verbosity=m; /t:Rebuild /p:Configuration=Release ..\Demo.WindowsForms\Demo.WindowsForms.csproj

%msbuildexe3% /nologo /p:WarningLevel=0;Optimize=True;Platform=AnyCPU;TargetFrameworkVersion=v3.5 /clp:Verbosity=m; /t:Rebuild /p:Configuration=Release ..\Demo.WindowsPresentation\Demo.WindowsPresentation.csproj

del /q /s .\%builddir2%\*.application
del /q /s .\%builddir2%\*.exe.manifest

del /q /s .\%builddir3%\*.application
del /q /s .\%builddir3%\*.exe.manifest

if "%1"=="nopause" goto end
pause
:end