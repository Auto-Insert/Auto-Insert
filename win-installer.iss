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
Source: "publish\*"; DestDir: "{app}"

[Icons]
Name: "{group}\Auto Insert"
Name: "{commondesktop}\Auto Insert"

[Run]
Filename: "{app}\AutoInsert.UI.exe"