# vscode-plaincat

Proof of concept for importing and exporting TwinCAT PLCs into [Visual Studio Code](https://code.visualstudio.com/)
such that is possible to view and edit code.

- The project implements minimal effort way to convert TwinCAT files (XML files) into plain text via regular expressions
- and a stripped down version of Zeugwerk's Structured Text Parser to make it possible to convert plain text into TwinCAT XMLs.
  While the full parser creates a full abstract syntax tree (AST) for Structured Text, the stripped down version
  only provides the bare minimum AST to convert plain text to TwinCAT XMLs.
  The full parser for now is only available on demand, [contact us](mailto:info@zeugwerk.at) if you are interested.

![plaincat](https://github.com/Zeugwerk/vscode-plaincat/assets/84121166/279447f6-6c67-4615-8502-dd9f9b7f6e04)


## How to use (Command Line Interface)

- Install the extension [Serhioromano.vscode-st](https://marketplace.visualstudio.com/items?itemName=Serhioromano.vscode-st) 
  so you get syntax highlighting for structured text in vscode.
- Build the CLI once
	```
	dotnet build Plaincat.sln -c Release
	```
	This produces `Plaincat/bin/Release/net6.0/Plaincat.dll` (run it with `dotnet Plaincat.dll ...`, or use the standalone
	`Plaincat.exe` on Windows). The tool works on Windows, Linux and macOS.
- To convert a TwinCAT plcproj file to plain text (.st files), run the following command
	```
	Plaincat decode --source <path_to_plcproj> --target <path_to_empty_folder>
	```
	
- To convert from plain text (.st files) back to TwinCAT, run the following command
	```
	Plaincat encode --source <path_to_folder_containing_st_files> --target <path_to_new_output_folder>
	```

- To do both in one step (e.g. to re-generate all GUIDs, or verify a project round-trips cleanly), run
	```
	Plaincat reencode --source <path_to_plcproj> --intermediate <path_to_tmp_folder> --target <path_to_new_output_folder>
	```

 ## How to use (vscode extension)

Note that this is not streamlined yet, so instead of just installing an extension from the marketplace you got to

 - Compile the C# project and copy the executable to `C:\appl\vscode-plaincat\vscode-plaincat\bin\` (if you want to use a different path modify `vscode-plaincat/src/extension.ts`)
 - Compile the vscode extension by opening the folder `vscode-plaincat`, running `npm install` and then `npx vsce package` in vscode's terminal
 - In vscode open 'Extensions' and install `vscode-plaincat-0.0.1.vsix` by clicking on 'Install from VSIX...'
 - Reload vscode
 - To convert an existing plcproj to plaintext open the command palette by pressing `Shift+Ctrl+P` and run the command `plaincat.decode` and follow the instructions
 - To convert plaintext back to a plcproj open the command palette by pressing `Shift+Ctrl+P` and run the command `plaincat.encode` and follow the instructions

 Note that the vscode extension itself only works on Windows, since it shells out to `Plaincat.exe` and TwinCAT projects only make sense on Windows anyway. The underlying CLI, however, can also be run on Linux/macOS (e.g. in CI) to convert projects headlessly.
