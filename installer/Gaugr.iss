#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif

[Setup]
AppId={{B7A9D8F2-4D15-4D6B-9B31-6A6C8C0F4A21}
AppName=Gaugr
AppVersion={#MyAppVersion}
AppPublisher=Gaugr
DefaultDirName={autopf}\Gaugr
DefaultGroupName=Gaugr
DisableProgramGroupPage=yes
OutputDir=..\publish
OutputBaseFilename=Gaugr-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
UninstallDisplayIcon={app}\Gaugr.exe
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\publish\Gaugr.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Gaugr"; Filename: "{app}\Gaugr.exe"
Name: "{autodesktop}\Gaugr"; Filename: "{app}\Gaugr.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Gaugr.exe"; Description: "Launch Gaugr"; Flags: postinstall nowait skipifsilent
