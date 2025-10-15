; ==============================================================
; GymAdmin Installer - Canal PROD / TEST
; ==============================================================
; Compila con:
;   ISCC.exe /DAppVersion=1.0.0 /DChannel=prod Installer\GymAdmin.iss
;   ISCC.exe /DAppVersion=0.0.5 /DChannel=test Installer\GymAdmin.iss
; ==============================================================

; ====== Canal (prod/test) pasado por /DChannel en línea de comando ======
#ifndef Channel
  #define Channel "prod"
#endif

#if Channel == "prod"
  #define AppName "GymAdmin"
  #define AppId   "{{2D9A4723-B8E7-48F8-B02C-C6BAF326F03C}}"   ; GUID PROD (fijo)
  #define SetupSuffix ""
  #define InstallDir  "{autopf}\GymAdmin"
#else
  #define AppName "GymAdmin Test"
  #define AppId   "{{A7F2B9EF-3C3F-4D34-9A7D-6B3E9E3B1234}}"   ; GUID TEST (fijo)
  #define SetupSuffix "-Test"
  #define InstallDir  "{autopf}\GymAdmin Test"
#endif

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

; ==============================================================
; CONFIGURACIÓN DE INSTALACIÓN
; ==============================================================

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={#InstallDir}
DefaultGroupName={#AppName}
OutputBaseFilename={#AppName}-{#AppVersion}-Setup{#SetupSuffix}
; output folder relativo al .iss
OutputDir={#SourcePath}\output
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
DisableDirPage=no
DisableProgramGroupPage=yes
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern

; Icono si existe
#if FileExists("{#SourcePath}\..\GymAdmin\GymAdmin.Desktop\Resources\Images\gymadmin_icon.ico")
  SetupIconFile={#SourcePath}\..\GymAdmin\GymAdmin.Desktop\Resources\Images\gymadmin_icon.ico
#endif

; ==============================================================
; ARCHIVOS
; ==============================================================

[Files]
; El workflow deja el publish en <repo_root>\publish
Source: "{#SourcePath}\..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

; ==============================================================
; ACCESOS DIRECTOS
; ==============================================================

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\GymAdmin.Desktop.exe"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\GymAdmin.Desktop.exe"; Tasks: desktopicon

; ==============================================================
; TAREAS OPCIONALES
; ==============================================================

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el Escritorio"; Flags: unchecked

; ==============================================================
; EJECUCIÓN TRAS INSTALACIÓN
; ==============================================================

[Run]
Filename: "{app}\GymAdmin.Desktop.exe"; Description: "Ejecutar {#AppName}"; Flags: nowait postinstall skipifsilent