#define AppName "GymAdmin"
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

[Setup]
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
SetupIconFile=..\GymAdmin-\GymAdmin\GymAdmin.Desktop\Resources\Images\gymadmin_icon.ico

[Files]
; Toma todo lo que dejó dotnet publish en /publish (carpeta al lado de /installer)
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\GymAdmin.Desktop.exe"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\GymAdmin.Desktop.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el Escritorio"; Flags: unchecked

[Run]
Filename: "{app}\GymAdmin.Desktop.exe"; Description: "Ejecutar {#AppName}"; Flags: nowait postinstall skipifsilent