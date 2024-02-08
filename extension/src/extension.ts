import * as vscode from 'vscode';
import * as path from 'path';
import * as child_process from 'child_process';

export function activate(context: vscode.ExtensionContext) {

	console.log('The extension "vscode-plaincat" is now active!' +
				'\nUse use plaincat.decode to export a plcproj to plaintext' +
				'\nUse plaincode.encode to generate a plcproj from plaintext');


	let encodeCommand = vscode.commands.registerCommand('plaincat.encode', () => {
		
	});

	let decodeCommand = vscode.commands.registerCommand('plaincat.decode', () => {
		decode();
	});	

	context.subscriptions.push(encodeCommand, decodeCommand);
}

function decode() {
	vscode.window.showInformationMessage('Decoding plcproj to plain text ...');

    const executablePath = 'C:\\appl\\vscode-plaincat\\vscode-plaincat\\bin\\Plaincat.exe';
    
	vscode.window.showOpenDialog({
		canSelectFiles: true,
		canSelectFolders: false,
		canSelectMany: false,
		filters: {
			'PLC project files': ['plcproj']
		}
	}).then(fileUri => {
		if (fileUri && fileUri.length > 0) {
			const plcprojPath = fileUri[0].fsPath;

			// Ask user to select target folder
			vscode.window.showOpenDialog({
				canSelectFiles: false,
				canSelectFolders: true,
				canSelectMany: false,
				openLabel: 'Select Target Folder'
			}).then(folderUri => {
				if (folderUri && folderUri.length > 0) {
					const targetFolder = folderUri[0].fsPath;

					let args: string[];
					args = ['decode', '--source', plcprojPath, '--target', targetFolder];
					vscode.window.showInformationMessage('Decoding ' + fileUri.toString() + " to " + folderUri.toString());

					// Run the executable with the specified action
					child_process.execFile(executablePath, args, (error, stdout, stderr) => {
						if (error) {
							vscode.window.showErrorMessage('Error executing Plaincat: ' + error.message);
							return;
						}

						// Handle stdout and stderr if needed
						console.log('Output: ' + stdout);
						console.error('Error: ' + stderr);

						vscode.commands.executeCommand('vscode.openFolder', vscode.Uri.file(targetFolder), false);
					});
				}
			});
		}
	});
}	

export function deactivate() {}
