#define AppName "GymAdmin"
#ifndef AppVersion
  #define AppVersion "1.0.4"
#endif

#define AppId "{{2D9A4723-B8E7-48F8-B02C-C6BAF326F03C}}"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
OutputBaseFilename={#AppName}-{#AppVersion}-Setup
OutputDir=output
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
DisableDirPage=no
DisableProgramGroupPage=yes
#if FileExists("..\GymAdmin\GymAdmin.Desktop\Resources\Images\gymadmin_icon.ico")
  SetupIconFile=..\GymAdmin\GymAdmin.Desktop\Resources\Images\gymadmin_icon.ico
#endif
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
; El workflow deja el publish en <repo_root>\publish
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\GymAdmin.Desktop.exe"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\GymAdmin.Desktop.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el Escritorio"; Flags: unchecked

[Run]
Filename: "{app}\GymAdmin.Desktop.exe"; Description: "Ejecutar {#AppName}"; Flags: nowait postinstall skipifsilent