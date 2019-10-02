@echo off
::set STEAMDIR="C:\Program Files (x86)\Steam\\steamapps\common\The Scroll Of Taiwu\"
set STEAMDIR="C:\Game\Steam\steamapps\common\The Scroll Of Taiwu"
if not exist "%~dp0\build" (mkdir "%~dp0\build")
set "PWD=%CD%"
cd "%~dp0\build"

if defined VisualStudioVersion goto :RunVCVars

set _VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist %_VSWHERE% (
    for /f "usebackq tokens=*" %%i in (`%_VSWHERE% -latest -property installationPath`) do set _VSCOMNTOOLS=%%i\Common7\Tools
)
if not exist "%_VSCOMNTOOLS%" set _VSCOMNTOOLS=%VS140COMNTOOLS%
if not exist "%_VSCOMNTOOLS%" goto :MissingVersion

call "%_VSCOMNTOOLS%\VsDevCmd.bat" > nul

:RunVCVars
if "%VisualStudioVersion%"=="16.0" (
    echo Visual Studio 2019
    cmake .. -G "Visual Studio 16 2019" -DSTEAMDIR=%STEAMDIR%
) else if "%VisualStudioVersion%"=="15.0" (
    echo Visual Studio 2017
    cmake .. -G "Visual Studio 15 2017" -DSTEAMDIR=%STEAMDIR%
) else (
:MissingVersion
    echo "cannot find Visual Studio 2017 or later version"
)

cd "%PWD%"
