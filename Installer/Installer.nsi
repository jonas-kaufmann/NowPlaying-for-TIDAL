Unicode True

!include "FileFunc.nsh"

# constants
!define APPNAME "NowPlaying-for-TIDAL"

# configure installer
OutFile "${APPNAME} Setup.exe"
SetCompressor lzma

InstallDir "$PROGRAMFILES64\${APPNAME}"

# pages to show
Page directory
Page instfiles

Section "Install"
 
# define output path
SetOutPath $INSTDIR

# specify files to go in output path
File /r "..\${APPNAME}\bin\publish\*"

# define uninstaller name
WriteUninstaller $INSTDIR\uninstall.exe

# create shortcut in start menu
CreateShortcut "$SMPROGRAMS\${APPNAME}.lnk" "$INSTDIR\${APPNAME}.exe"

# add entry to programs
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "QuietUninstallString" "$\"$INSTDIR\uninstall.exe$\" /S"

# compute install folder size to registry
${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
IntFmt $0 "0x%08X" $0
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "EstimatedSize" "$0"

# autostart program with windows
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Run" "${APPNAME}" '"$InstDir\${APPNAME}.exe"'

SectionEnd



Section "Uninstall"

# delete start menu shortcut
Delete "$SMPROGRAMS\${APPNAME}.lnk"

# remove program from autostart
DeleteRegValue HKLM "Software\Microsoft\Windows\CurrentVersion\Run" "${APPNAME}"

# remove entry from programs
DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
 
# Delete the directory
RMDir /r $INSTDIR

SectionEnd