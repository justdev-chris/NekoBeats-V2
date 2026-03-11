[Setup]
AppName=NekoBeats
AppVersion=2.2
AppPublisher=justdev-chris
AppPublisherURL=https://github.com/justdev-chris/NekoBeats-V2
AppSupportURL=https://github.com/justdev-chris/NekoBeats-V2
AppUpdatesURL=https://github.com/justdev-chris/NekoBeats-V2/releases
DefaultDirName={autopf}\NekoBeats
DefaultGroupName=NekoBeats
AllowNoIcons=yes
OutputDir=output
OutputBaseFilename=NekoBeats-2.2-installer
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\NekoBeats.exe
LicenseFile=LICENSE.txt
SetupIconFile=NekoBeatsLogo.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1
Name: "startmenu"; Description: "Create Start Menu shortcut"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked


[Files]
Source: "bin\Release\net6.0-windows\win-x64\publish\NekoBeats.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net6.0-windows\win-x64\publish\*.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "bin\Release\net6.0-windows\win-x64\publish\*.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "NekoBeatsLogo.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "NekoBeatsLogo.png"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\NekoBeats"; Filename: "{app}\NekoBeats.exe"; IconFileName: "{app}\NekoBeatsLogo.ico"; Comment: "Audio Visualizer"
Name: "{group}\{cm:UninstallProgram,NekoBeats}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\NekoBeats"; Filename: "{app}\NekoBeats.exe"; Tasks: desktopicon; IconFileName: "{app}\NekoBeatsLogo.ico"; Comment: "Audio Visualizer"
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\NekoBeats"; Filename: "{app}\NekoBeats.exe"; Tasks: quicklaunchicon; IconFileName: "{app}\NekoBeatsLogo.ico"

[Run]
Filename: "{app}\NekoBeats.exe"; Description: "{cm:LaunchProgram,NekoBeats}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
