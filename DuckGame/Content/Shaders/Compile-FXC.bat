@echo off
for /r "%~dp0Source" %%i in (*.fx) do cmd /c "fxc.exe /nologo /Gec /T fx_2_0 /Fo Compiled\%%~ni.xnb %%i"