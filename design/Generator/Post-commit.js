/**
 * Svn post-commit 脚本, 用于在提交时执行配置更新.
 */

// 摘自TSVN官网,尼玛,网官打开缓慢,为了参考方便,直接放在这里.
// see: http://tortoisesvn.net/docs/release/TortoiseSVN_en/tsvn-dug-settings.html
//      Figure 4.86. The Settings Dialog, Configure Hook Scripts
//
// Start-commit
// PATH MESSAGEFILE CWD
// 
// Pre-commit
// PATH DEPTH MESSAGEFILE CWD
// 
// Post-commit
// PATH DEPTH MESSAGEFILE REVISION ERROR CWD
// 
// Start-update
// PATH CWD
// 
// Pre-update
// PATH DEPTH REVISION CWD
// 
// Post-update
// PATH DEPTH REVISION ERROR CWD
// 
// Pre-connect
// no parameters are passed to this script. You can pass a custom parameter by appending it to the script path.

var ForReading = 1;
var WSH = new ActiveXObject("WScript.Shell");
var FSO = new ActiveXObject("Scripting.FileSystemObject");

// Create replaceAll/trim method for String.
String.prototype.replaceAll = function (matchStr, repStr) {
    var re = new RegExp(matchStr, "g");
    return this.replace(re, repStr);
};

String.prototype.trim = function() {
    return this.replace("/(^\s*)|(\s*$)/g", "");
};

// Create filter method for Array.
if (!Array.prototype.filter) {
    Array.prototype.filter = function (fun) {
        if (typeof fun != "function")
            throw new TypeError();

        var res = [];
        var len = this.length;
        for (var i = 0; i < len; i++) {
            if (fun.call(this[i]))
                res.push(this[i]);
        }
    };
}

// Create log class.
function Log(fileName) {
    // this._file = FSO.CreateTextFile(fileName, true);

    this.out = function (msg) {
        // this._file.WriteLine(msg);
    };

    this.close = function () {
        // this._file.Close();
    };
}

// Create log obj.
log = new Log("post_commit.log");

// Get TSVN input datas.
var argv = WScript.Arguments;
var list = argv(0); // PATH
var cwd = argv(5); // CWD

// Open list file for get commited files.
var f = FSO.OpenTextFile(list, ForReading);
var lineData = f.ReadAll();
f.Close();


var unfilteredFiles = lineData.split("\r\n");

log.out("CWD: " + cwd);
log.out("DEPTH: " + argv(1));
log.out("MESSAGE: " + FSO.OpenTextFile(argv(2)).ReadAll());
log.out("REVISION: " + argv(3));
log.out("ERROR: " + argv(4));
log.out("CHANGED_FILES: " + unfilteredFiles);
 
// Filer-out files.
var filteredFiles = [];
for (var i = 0; i < unfilteredFiles.length; i++) {
    ext = FSO.GetExtensionName(unfilteredFiles[i]);
    if (ext != "xls" && ext != "xlsx" && ext != "xlsm")
        log.out(">>> File filter out: " + unfilteredFiles[i]);
    else
        filteredFiles.push("\"" + unfilteredFiles[i] + "\"");
}

if (filteredFiles.length == 0) {
    log.out("Not exist any files to gen. Done!");
} else {
    // Get run.py script file path.
    var runPath = cwd;
    var runFile = runPath + "\\run\\run.py";
    while (!FSO.FileExists(runFile)) {
        runPath = FSO.GetParentFolderName(runPath);
        runFile = runPath + "\\run\\run.py";
    }

    log.out("Searching run.py done, path: " + runFile);

    // Call python script to update server/client config code files & data files.
    var files = filteredFiles.join(" ");
    log.out("These config files wil gen: " + files);

    WSH.Run("python \"" + runFile + "\" " + files);
}

log.out("Done!");
log.close();

