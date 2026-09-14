# vscode-plaincat

Proof of concept for importing and exporting TwinCAT PLCs into [Visual Studio Code](https://code.visualstudio.com/)
so you can view and edit the Structured Text as plain files.

> **This repository is archived and unmaintained.**
>
> Plaincat still works as a standalone tool. Build it, run `decode` / `encode`, use the vscode extension. Nothing
> here depends on our other products.
>
> We are not developing it further. The conversion is a proof of concept: TwinCAT XML is pulled apart with regular
> expressions and written back with a stripped-down Structured Text grammar. Object GUIDs are regenerated on encode,
> so a round trip rewrites the project. That is fine for looking at code. It is a poor basis for a real workflow.
>
> The work that grew out of this lives in **[PlcSense](https://plcsense.com)** (language support, navigation and
> editing for TwinCAT / Structured Text) and **`zkplaincat`**, a later converter from the same line of work that
> round-trips a project without throwing away GUIDs and non-ST objects.
>
> - PlcSense: [https://plcsense.com](https://plcsense.com)
> - Zeugwerk: [https://zeugwerk.dev](https://zeugwerk.dev)
>
> You are welcome to keep using Plaincat. If you came here to edit TwinCAT in vscode, PlcSense is what we actually
> maintain.

![plaincat](https://github.com/Zeugwerk/vscode-plaincat/assets/84121166/279447f6-6c67-4615-8502-dd9f9b7f6e04)

## How this converter works

- TwinCAT files (XML) are turned into plain text with regular expressions.
- Plain text is turned back into TwinCAT XML with a stripped-down version of Zeugwerk's Structured Text parser.
  The full parser builds a complete abstract syntax tree. This copy only has the minimum needed to rebuild the XML.

## How to use (Command Line Interface)

- Install the extension [Serhioromano.vscode-st](https://marketplace.visualstudio.com/items?itemName=Serhioromano.vscode-st)
  so you get syntax highlighting for Structured Text in vscode.
- Build the CLI once

	```
	dotnet build Plaincat.sln -c Release
	```

	This produces `Plaincat/bin/Release/net6.0/Plaincat.dll` (run it with `dotnet Plaincat.dll ...`, or use the standalone
	`Plaincat.exe` on Windows). The tool works on Windows, Linux and macOS.

- To convert a TwinCAT plcproj file to plain text (.st files), run

	```
	Plaincat decode --source <path_to_plcproj> --target <path_to_empty_folder>
	```

- To convert from plain text (.st files) back to TwinCAT, run

	```
	Plaincat encode --source <path_to_folder_containing_st_files> --target <path_to_new_output_folder>
	```

- To do both in one step (for example to re-generate all GUIDs, or to check that a project round-trips), run

	```
	Plaincat reencode --source <path_to_plcproj> --intermediate <path_to_tmp_folder> --target <path_to_new_output_folder>
	```

## How to use (vscode extension)

This is not streamlined yet, so instead of installing from the marketplace you have to:

- Compile the C# project and copy the executable to `C:\appl\vscode-plaincat\vscode-plaincat\bin\` (or set
  `plaincat.executablePath` to your `Plaincat.exe`)
- Compile the extension by opening the folder `vscode-plaincat`, then running `npm install` and `npx vsce package`
  in vscode's terminal
- In vscode open 'Extensions' and install `vscode-plaincat-0.0.1.vsix` via 'Install from VSIX...'
- Reload vscode
- Run `plaincat.decode` from the command palette (`Shift+Ctrl+P`) to turn a `.plcproj` into plain text
- Run `plaincat.encode` to turn the plain text back into a `.plcproj`

The extension only works on Windows, because it shells out to `Plaincat.exe` and TwinCAT projects only make sense
on Windows. The CLI itself also runs on Linux and macOS, so a pipeline can convert projects headlessly.
