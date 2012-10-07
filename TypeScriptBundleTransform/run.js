/* by Wouter Devinck (www.wouterdevinck.be) */

// Handles compiler output
var outputmachine = {
    source: "",
    Write: function (s) {
        this.source += s;
    },
    WriteLine: function (s) {
        this.source += s + "\n";
    },
    Close: function () { }
};

// The TypeScript compiler (typescript.js)
var compiler = new TypeScript.TypeScriptCompiler(outputmachine);

// In case an error occurs, the error handler in TypeScriptTransformer.cs is 
// called and the error message is added as a new line to the error variable,
// which will also later be read from TypeScriptTransformer.cs.
var error = "";
compiler.parser.errorRecovery = true;
compiler.setErrorCallback(function (start, len, message, block) {
    var msg = 'Compilation error: ' + message + ' Code block: ' + block + ' Start position: ' + start + ' Length: ' + len;
    errorhandler.OnError(msg);
    error += "\n" + msg;
});

// Add the library file to the comiler input (lib.js)
compiler.addUnit(libfile, 'lib.d.ts');

// Add the TypeScript source
compiler.addUnit(source, '');

// If type checking must be performed, ask the compiler to do so
if (typeChecking) {
    compiler.typeCheck();
}

// Get the resulting javascript
compiler.emit(false, function createFile(fileName) {
    return outputmachine;
});

// Put the resulting javascript in a variable that can then be read from TypeScriptTransformer.cs
var result = outputmachine.source;