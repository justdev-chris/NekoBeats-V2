!include "MUI2.nsh"

; ---------------------------------------------------------
; Banner + Header images (MUST be before MUI_PAGE macros)
; ---------------------------------------------------------
!define MUI_WELCOMEFINISHPAGE_BITMAP "NekoBeatsBanner.bmp"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "NekoBeatsHeader.bmp"
!define MUI_HEADERIMAGE_HEIGHT 57
!define MUI_HEADERIMAGE_RIGHT

; ---------------------------------------------------------
; Installer metadata
; ---------------------------------------------------------
Name "NekoBeats"
OutFile "NekoBeats-2.3-installer.exe"
InstallDir "$PROGRAMFILES\NekoBeats"

; ---------------------------------------------------------
; MUI Pages
; ---------------------------------------------------------
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_LANGUAGE "English"

; ---------------------------------------------------------
; Installer settings
; ---------------------------------------------------------
SetCompress force
SetDatablockOptimize on
CRCCheck on
XPStyle on

; ---------------------------------------------------------
; Install Section
; ---------------------------------------------------------
Section "Install"
  SetOutPath "$INSTDIR"

  ; Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

  ; App files
  File "..\bin\Release\net8.0-windows\win-x64\publish\NekoBeats.exe"
  File "..\bin\Release\net8.0-windows\win-x64\publish\*.dll"

  ; Icons
  File "NekoBeatsLogo.ico"
  File "NekoBeatsLogo.png"

  ; Shortcuts
  CreateDirectory "$SMPROGRAMS\NekoBeats"
  CreateShortCut "$SMPROGRAMS\NekoBeats\NekoBeats.lnk" "$INSTDIR\NekoBeats.exe" "" "$INSTDIR\NekoBeatsLogo.ico"
  CreateShortCut "$SMPROGRAMS\NekoBeats\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
  CreateShortCut "$DESKTOP\NekoBeats.lnk" "$INSTDIR\NekoBeats.exe" "" "$INSTDIR\NekoBeatsLogo.ico"
SectionEnd

; ---------------------------------------------------------
; Uninstall Section
; ---------------------------------------------------------
Section "Uninstall"
  Delete "$INSTDIR\NekoBeats.exe"
  Delete "$INSTDIR\*.dll"
  Delete "$INSTDIR\NekoBeatsLogo.ico"
  Delete "$INSTDIR\NekoBeatsLogo.png"

  ; Remove uninstaller
  Delete "$INSTDIR\Uninstall.exe"

  ; Remove install directory
  RMDir "$INSTDIR"

  ; Remove Start Menu shortcuts
  Delete "$SMPROGRAMS\NekoBeats\NekoBeats.lnk"
  Delete "$SMPROGRAMS\NekoBeats\Uninstall.lnk"
  RMDir "$SMPROGRAMS\NekoBeats"

  ; Remove Desktop shortcut
  Delete "$DESKTOP\NekoBeats.lnk"
SectionEnd
