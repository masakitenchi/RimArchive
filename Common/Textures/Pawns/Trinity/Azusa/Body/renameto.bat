@echo off
set arg1 = %1
for %%i in (*.dds *.png) do echo %%i >>names.txt
for /F "delims=_ tokens=1-3" %%i in (names.txt) do ren %%i_%%j_%%k %%i_%1_%%k
del names.txt