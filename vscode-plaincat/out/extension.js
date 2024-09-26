"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.deactivate = exports.activate = void 0;
const vscode = __importStar(require("vscode"));
const childprocess = __importStar(require("child_process"));
const executablePath = 'C:\\appl\\vscode-plaincat\\vscode-plaincat\\bin\\Plaincat.exe';
function activate(context) {
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
exports.activate = activate;
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
                    let args;
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
            let args;
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
function deactivate() { }
exports.deactivate = deactivate;
//# sourceMappingURL=extension.js.map