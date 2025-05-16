[Setup]
AppName=Realsonnet AI Chat
AppVersion=1.0
DefaultDirName={pf}\RealsonnetAIChat
DefaultGroupName=Realsonnet AI Chat
OutputDir=.
OutputBaseFilename=RealsonnetInstaller

[Files]
Source: "bin\Release\net6.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Realsonnet AI Chat"; Filename: "{app}\RealsonnetApp.exe"
Name: "{commondesktop}\Realsonnet AI Chat"; Filename: "{app}\RealsonnetApp.exe"