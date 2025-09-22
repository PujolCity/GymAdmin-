#define AppName "GymAdmin"
#ifndef AppVersion
  #define AppVersion "1.0.3"
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
SetupIconFile=..\GymAdmin\GymAdmin.Desktop\Resources\Images\gymadmin_icon.ico
ArchitecturesInstallIn64BitMode=x64

[Files]
; Toma todo lo que dejó dotnet publish en /publish (carpeta al lado de /installer)
Source: "..\GymAdmin\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\GymAdmin.Desktop.exe"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\GymAdmin.Desktop.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el Escritorio"; Flags: unchecked

[Run]
Filename: "{app}\GymAdmin.Desktop.exe"; Description: "Ejecutar {#AppName}"; Flags: nowait postinstall skipifsilent