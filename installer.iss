; Inno Setup Script for Auto-Insert
#define MyAppName "Auto-Insert"
#define MyAppVersion "1.0.0"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
OutputDir=installer-output
OutputBaseFilename=AutoInsert-Setup-{#MyAppVersion}

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs
