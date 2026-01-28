@echo off
forfiles /P %~dp0Source /M *.fx /C "cmd /C %~dp0fxc.exe /nologo /Gec /T fx_2_0 /Fo %1\@fname.xnb @file"
EXIT /B 0