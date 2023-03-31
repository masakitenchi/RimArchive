@echo off

setlocal enabledelayedexpansion

for %%f in (*Equipment_Icon_*.png) do (
  set "file=%%~nf"
  set "new_file=!file:Equipment_Icon_=!"
  set "extension=%%~xf"
  set "new_name=!new_file!!extension!"
  ren "%%f" "!new_name!"
)

echo Done.
