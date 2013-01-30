
rem set msbuildexe2=%WINDIR%\Microsoft.NET\Framework\v2.0.50727\msbuild.exe
set msbuildexe3=%WINDIR%\Microsoft.NET\Framework\v3.5\msbuild.exe
rem set msbuildexe=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe

set builddir3=Release-NETv3.5CF
mkdir .\%builddir3% 
del /q /s .\%builddir3%\*.*

%msbuildexe3% /nologo /p:WarningLevel=0;Optimize=True;Platform=AnyCPU;TargetFrameworkVersion=v3.5 /clp:Verbosity=m; /t:Rebuild /p:Configuration=Release ..\Demo.WindowsMobile\Demo.WindowsMobile.csproj

del /q /s .\%builddir3%\*.application
del /q /s .\%builddir3%\*.exe.manifest

pause