
rem set msbuildexe=%WINDIR%\Microsoft.NET\Framework\v2.0.50727\msbuild.exe
rem set msbuildexe=%WINDIR%\Microsoft.NET\Framework\v3.5\msbuild.exe
set msbuildexe=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe

set builddir2=Release-MONOv2.0
mkdir .\%builddir2% 
del /q /s .\%builddir2%\*.*

set builddir4=Release-MONOv4.0
mkdir .\%builddir4% 
del /q /s .\%builddir4%\*.*

%msbuildexe% /fl /flp:LogFile=build-mono.log;errorsonly /nologo /p:WarningLevel=0;Optimize=True;OutputPath="..\Build\%builddir2%";Platform=AnyCPU;DefineConstants="MONO;SQLite";TargetFrameworkVersion=v2.0 /clp:Verbosity=m; /t:Rebuild /p:Configuration=Release ..\Demo.WindowsForms\Demo.WindowsForms.csproj

%msbuildexe% /fl /flp:LogFile=build-mono.log;Append;errorsonly /nologo /p:WarningLevel=0;Optimize=True;OutputPath="..\Build\%builddir4%";Platform=AnyCPU;DefineConstants="MONO;SQLite";TargetFrameworkVersion=v4.0 /clp:Verbosity=m; /t:Rebuild /p:Configuration=Release ..\Demo.WindowsForms\Demo.WindowsForms.csproj

del /q /s .\%builddir2%\*.application
del /q /s .\%builddir2%\*.exe.manifest

del /q /s .\%builddir4%\*.application
del /q /s .\%builddir4%\*.exe.manifest

copy /b ..\Info\License.txt .\%builddir2%\License.txt
copy /b ..\Info\License.txt .\%builddir4%\License.txt

if "%1"=="nopause" goto end
pause
:end