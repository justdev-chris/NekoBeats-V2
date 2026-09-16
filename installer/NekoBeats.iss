[Setup]
AppName=NekoBeats
AppVersion=2.3.4
AppPublisher=justdev-chris
AppPublisherURL=https://github.com/justdev-chris/NekoBeats-V2
DefaultDirName={pf}\NekoBeats
DefaultGroupName=NekoBeats
OutputDir=installer
OutputBaseFilename=NekoBeats-2.3.4-installer
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\NekoBeats.exe
SetupIconFile=NekoBeatsLogo.ico
WizardImageFile=NekoBeatsBanner.bmp
WizardImageStretch=yes

; Language selection
ShowLanguageDialog=yes
LanguageDetectionMethod=uilanguage

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"
Name: "ja"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "zh"; MessagesFile: "..\lang\ChineseSimplified.isl"
Name: "ru"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "pt"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "it"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "ko"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "ar"; MessagesFile: "compiler:Languages\Arabic.isl"
Name: "traditional"; MessagesFile: "..\lang\ChineseTraditional.isl"

[Files]
; Ship the entire self-contained publish output (includes runtimes\, *.dll,
; *.json config, and the app's own lang\ folder if your .csproj copies it)
Source: "..\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Branding assets
Source: "NekoBeatsLogo.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "NekoBeatsLogo.png"; DestDir: "{app}"; Flags: ignoreversion

; App localization JSONs (only needed if NOT copied by the .csproj at build time)
Source: "..\lang\*.json"; DestDir: "{app}\lang"; Flags: ignoreversion

[Icons]
Name: "{group}\NekoBeats"; Filename: "{app}\NekoBeats.exe"; IconFilename: "{app}\NekoBeatsLogo.ico"
Name: "{group}\Uninstall NekoBeats"; Filename: "{uninstallexe}"
Name: "{commondesktop}\NekoBeats"; Filename: "{app}\NekoBeats.exe"; IconFilename: "{app}\NekoBeatsLogo.ico"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "DisplayName"; ValueData: "NekoBeats v2.3.4"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "DisplayVersion"; ValueData: "2.3.4"
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "InstallLocation"; ValueData: "{app}"
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "UninstallString"; ValueData: "{uninstallexe}"
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "DisplayIcon"; ValueData: "{app}\NekoBeatsLogo.ico"
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\NekoBeats"; ValueType: string; ValueName: "Publisher"; ValueData: "justdev-chris"
; Save selected language
Root: HKCU; Subkey: "Software\NekoBeats"; ValueType: string; ValueName: "InstallerLanguage"; ValueData: "{language}"; Flags: uninsdeletekey

[Run]
Filename: "{app}\NekoBeats.exe"; Description: "Launch NekoBeats"; Flags: nowait postinstall skipifsilent
