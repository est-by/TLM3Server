@echo off
rem ������ ��� eMaxToole
call env.bat
msbuild.exe EmeraDrv.csproj /t:Rebuild /p:Client=None /p:OS=Win /verbosity:d 
