# vscode-plaincat

Proof of concept for importing and exporting TwinCAT PLCs into [Visual Studio Code](Visual Studio Code)
such that is possible to view and edit code.

- The project implements minimal effort way to convert TwinCAT files (XML files) into plain text via regular expressions
- and a stripped down version of Zeugwerk's Structured Text Parser to make it possible to convert plain text into TwinCAT XMLs.
  While the full parser creates a full abstract syntax tree (AST) for Structured Text, the stripped down version
  only provides the bare minimum AST to convert plain text to TwinCAT XMLs.
  The full parser for now is only available on demand, [contact us](mailto:contact us) if you are interested.

![plaincat](https://github.com/Zeugwerk/vscode-plaincat/assets/84121166/279447f6-6c67-4615-8502-dd9f9b7f6e04)


## How to use (Command Line Interface)

- Install the extension [Serhioromano.vscode-st](https://marketplace.visualstudio.com/items?itemName=Serhioromano.vscode-st) 
  so you get syntax highlighting for structured text in vscode.
- To convert a TwinCAT plcproj file to plain text (.st files), run the following command
	```
	Plaincat encode --source <path_to_plcproj> --target <path_to_folder>
	```
	
- To convert from plain text (.st files) back to TwinCAT, run the following command
	```
	Plaincat decode --target <path_to_empty_folder_for_new_plcproj> --source <path_to_folder_containing_st_files>
	```
