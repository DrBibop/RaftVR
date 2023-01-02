:: RML Mod Build Script by TeKGameR. Adaptation by DrBibop.

:: Disabling echoing
@echo off
:: Defining the window title
title RML Mod Build Script
:: Copying the mod build into the loader's directory
if exist "%~dp0..\RaftVRMod\RaftVRMod\obj\Release\RaftVRMod.dll" ( copy "%~dp0..\RaftVRMod\RaftVRMod\obj\Release\RaftVRMod.dll" "%~dp0.\RaftVRLoader\Assemblies\RaftVRMod.dll" )
:: Retrieving the current folder name
for %%* in (.) do set foldername=%%~n*
:: Creating a folder to contain temporary files for the build
mkdir "build"
:: Copying the solution directory in the "build" folder except ".csproj, .rmod" files and "bin, obj" folders.
robocopy "%foldername%" "build" /E /XF *.csproj *.rmod /XD bin obj
:: Checking if a .rmod with the same name already exists and if it does, delete it.
if exist "RaftVR.rmod" ( del "RaftVR.rmod" )
:: Zipping the "build" folder. (.rmod are just zipped files)
powershell "[System.Reflection.Assembly]::LoadWithPartialName('System.IO.Compression.FileSystem');[System.IO.Compression.ZipFile]::CreateFromDirectory(\"build\", \"RaftVR.rmod\", 0, 0)"
:: Deleting the "build" folder
rmdir /s /q "build"
:: Build succeeded!
:: Copying .rmod file into mods directory
copy "%~dp0.\RaftVR.rmod" "%1\RaftVR.rmod"
EXIT