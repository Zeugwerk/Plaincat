# vscode-plaincat

Proof of concept for importing and exporting TwinCAT PLCs into [Visual Studio Code](Visual Studio Code)
such that is possible to view and edit code.

- The project implements minimal effort way to convert TwinCAT files (XML files) into plain text via regular expressions
- and a stripped down version of Zeugwerk's Structured Text Parser to make it possible to convert plain text into TwinCAT XMLs.
  While the full parser creates a full abstract syntax tree (AST) for Structured Text, the stripped down version
  only provides the bare minimum AST to convert plain text to TwinCAT XMLs.
  The full parser for now is only available on demand, [contact us](mailto:contact us) if you are interested.
