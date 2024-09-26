import * as vscode from 'vscode';
import * as path from 'path';
import * as childprocess from 'child_process';

const executablePath = 'C:\\appl\\vscode-plaincat\\vscode-plaincat\\bin\\Plaincat.exe';

export function activate(context: vscode.ExtensionContext) {

	console.log('The extension "vscode-plaincat" is now active!' +
				'\nUse use plaincat.decode to export a plcproj to plaintext' +
				'\nUse plaincode.encode to generate a plcproj from plaintext');


	let encodeCommand = vscode.commands.registerCommand('plaincat.encode', () => {
		encode();
	});

	let decodeCommand = vscode.commands.registerCommand('plaincat.decode', () => {
		decode();
	});	

	context.subscriptions.push(encodeCommand, decodeCommand);	
}

function decode() {
	vscode.window.showInformationMessage('Decoding plcproj to plain text ...');
    
	// asks for plcproj
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

			// ask for folder
			vscode.window.showOpenDialog({
				canSelectFiles: false,
				canSelectFolders: true,
				canSelectMany: false,
				openLabel: 'Select Target Folder for .st files'
			}).then(folderUri => {
				if (folderUri && folderUri.length > 0) {
					const targetFolder = folderUri[0].fsPath;

					let args: string[];
					args = ['decode', '--source', plcprojPath, '--target', targetFolder];
					vscode.window.showInformationMessage('Decoding ' + fileUri.toString() + " to " + folderUri.toString());

					childprocess.execFile(executablePath, args, (error, stdout, stderr) => {
						if (error) {
							vscode.window.showErrorMessage('Error executing Plaincat: ' + error.message);
							return;
						}

						vscode.commands.executeCommand('vscode.openFolder', vscode.Uri.file(targetFolder), false);
					});
				}
			});
		}
	});
}

function encode() {
	vscode.window.showInformationMessage('Encoding plain text to plcproj ...');

	if (!vscode.workspace.workspaceFolders || vscode.workspace.workspaceFolders.length === 0) {
		vscode.window.showErrorMessage('No workspace opened.');
		return;
	}

	const workspaceFolder = vscode.workspace.workspaceFolders[0].uri.fsPath;

	// ask for target folder
	vscode.window.showOpenDialog({
		canSelectFiles: false,
		canSelectFolders: true,
		canSelectMany: false,
		openLabel: 'Select Target Folder for .st files'
	}).then(folderUri => {
		if (folderUri && folderUri.length > 0) {
			const targetFolder = folderUri[0].fsPath;

			let args: string[];
			args = ['encode', '--source', workspaceFolder, '--target', targetFolder];
			vscode.window.showInformationMessage('Encoding ' + workspaceFolder.toString() + " to " + folderUri.toString());

			childprocess.execFile(executablePath, args, (error, stdout, stderr) => {
				if (error) {
					vscode.window.showErrorMessage('Error executing Plaincat: ' + error.message);
					return;
				}

				vscode.window.showInformationMessage('Successfully created plcproj');
			});
		}
	});
}


export function deactivate() {}
