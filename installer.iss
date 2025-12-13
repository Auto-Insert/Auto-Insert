; Inno Setup Script for Auto-Insert
#define MyAppName "Auto-Insert"
#define MyAppVersion "1.0.0"
#define MyAppExeName "AutoInsert.UI.exe"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
OutputDir=installer-output
OutputBaseFilename=AutoInsert-Setup-{#MyAppVersion}

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
