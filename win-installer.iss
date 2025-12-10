[Setup]
AppName=Auto Insert
AppVersion=1.0.0
DefaultDirName={pf}\AutoInsert
DefaultGroupName=Auto Insert
OutputDir=.
OutputBaseFilename=AutoInsertSetup
Compression=lzma2
SolidCompression=yes

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\Auto Insert"; Filename: "{app}\AutoInsert.UI.exe"
Name: "{commondesktop}\Auto Insert"; Filename: "{app}\AutoInsert.UI.exe"

[Run]
Filename: "{app}\AutoInsert.UI.exe"; Description: "Launch Auto Insert"; Flags: postinstall nowait skipifsilent