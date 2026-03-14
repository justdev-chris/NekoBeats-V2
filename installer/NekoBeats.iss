[Setup]
AppName=NekoBeats
AppVersion=2.3
AppPublisher=justdev-chris
AppPublisherURL=https://github.com/justdev-chris/NekoBeats-V2
DefaultDirName={pf}\NekoBeats
DefaultGroupName=NekoBeats
OutputDir=installer
OutputBaseFilename=NekoBeats-2.3-installer
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\NekoBeats.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net8.0-windows\win-x64\publish\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "installer\NekoBeatsLogo.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "installer\NekoBeatsLogo.png"; DestDir: "{app}"; Flags: ignoreversion
Source: "installer\NekoBeatsBanner.bmp"; DestDir: "{app}"; Flags: ignoreversion
Source: "installer\NekoBeatsHeader.bmp"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\NekoBeats"; Filename: "{app}\NekoBeats.exe"; IconFilename: "{app}\NekoBeatsLogo.ico"
Name: "{group}\Uninstall NekoBeats"; Filename: "{uninstallexe}"
Name: "{commondesktop}\NekoBeats"; Filename: "{app}\NekoBeats.exe"; IconFilename: "{app}\NekoBeatsLogo.ico"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "DisplayName"; ValueData: "NekoBeats v2.3"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "DisplayVersion"; ValueData: "2.3"
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "InstallLocation"; ValueData: "{app}"
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "UninstallString"; ValueData: "{uninstallexe}"
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "DisplayIcon"; ValueData: "{app}\NekoBeatsLogo.ico"
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "Publisher"; ValueData: "justdev-chris"

[Run]
Filename: "{app}\NekoBeats.exe"; Description: "Launch NekoBeats"; Flags: nowait postinstall skipifsilent
