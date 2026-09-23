@echo off
setlocal
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

"%CSC%" /nologo /target:winexe /optimize+ /codepage:65001 /out:BatchRenamer.exe ^
  /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll ^
  RenameTool.cs

if errorlevel 1 (
    echo Compile failed.
    exit /b 1
)
echo Build OK: BatchRenamer.exe
