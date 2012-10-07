// by Wouter Devinck (www.wouterdevinck.be)

using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Web.Optimization;
using Noesis.Javascript;

namespace TypeScriptBundleTransform {

    /// <summary>
    /// A bundle transformer that takes TypeScript and outputs JavaScript.
    /// </summary>
    public class TypeScriptTransformer : IBundleTransform {

        public enum ErrorLevel {
            /// <summary>
            /// Do not run type checking.
            /// </summary>
            NoTypeChecking,
            /// <summary>
            /// Run type checking and log the errors, both in the Visual Studio output window 
            /// and as console.error in the JavaScript output.
            /// </summary>
            LogError,
            /// <summary>
            /// Run type checking and throw a TypeScriptCompilerException if there are any errors.
            /// </summary>
            ThrowException
        }

        // Resource: the compile is written in TypeScript, we use the compiled (javascript) version
        private const string Compiler = "TypeScriptBundleTransform.typescript.js";

        // Resource: a packed version of lib.d.ts
        private const string Library = "TypeScriptBundleTransform.lib.js";

        // Resource: a script that runs the compiler (see run.js)
        private const string Run = "TypeScriptBundleTransform.run.js";

        // Indicates whether the TypeScript compiler should run type checking and 
        // whether error should be logged or exceptions should be thrown.
        private readonly ErrorLevel _errorLevel;

        // Indicates whether the result should be minified.
        private readonly bool _minify;

        /// <summary>
        /// A bundle transformer that takes TypeScript and outputs JavaScript.
        /// </summary>
        /// <param name="errorLevel">Indicates whether the TypeScript compiler should run type checking and 
        /// whether error should be logged or exceptions should be thrown.</param>
        /// <param name="minify">Indicates whether the result should be minified.</param>
        public TypeScriptTransformer(ErrorLevel errorLevel, bool minify) {
            _errorLevel = errorLevel;
            _minify = minify;
        }

        /// <summary>
        /// A bundle transformer that takes TypeScript and outputs JavaScript.
        /// Type checking will be performed.
        /// </summary>
        /// <param name="minify">Indicates whether the result should be minified.</param>
        /// <exception cref="TypeScriptCompilerException">Type checking found an error.</exception>
        public TypeScriptTransformer(bool minify) {
            _errorLevel = ErrorLevel.ThrowException;
            _minify = minify;
        }

        /// <summary>
        /// A bundle transformer that takes TypeScript and outputs JavaScript.
        /// Type checking will be performed.
        /// No minification will be done.
        /// </summary>
        /// <exception cref="TypeScriptCompilerException">Type checking found an error.</exception>
        public TypeScriptTransformer() {
            _errorLevel = ErrorLevel.ThrowException;
            _minify = false;
        }

        // We use the V8 javascript engine to execute the compiler, which is platform specific.
        // A post-build step deletes Noesis.Javascript.dll, so the AssemblyResolve event is fired.
        // We use this to load the right assembly (either x86 or x64).
        static TypeScriptTransformer() {
            AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
                if (e.Name.StartsWith("Noesis.Javascript", StringComparison.OrdinalIgnoreCase)) {
                    // Determine the ISA
                    var isa = (IntPtr.Size == 4) ? "x86" : "x64";
                    // Get the path to the bin folder
                    var binDir = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
                    // Get the file name
                    var assemblyName = string.Format("v8\\Noesis.Javascript.{0}.dll", isa);
                    var fileName = Path.Combine(binDir, assemblyName);
                    // Load the assembly
                    return Assembly.LoadFile(fileName);
                }
                return null;
            };
        }

        // Takes the TypeScript input, runs the compiler script (using the V8 javascript engine) and returns the javascript output.
        public void Process(BundleContext context, BundleResponse response) {
            // Get the TypeScript input
            var input = response.Content;
            var output = string.Empty;
            // Use the javascript engine
            using (var v8 = new JavascriptContext()) {
                // Set a handler that can be called from javascript in case the compiler found an error (see run.js)
                v8.SetParameter("errorhandler", new ErrorHandler(_errorLevel == ErrorLevel.ThrowException));
                // Pass the TypeScript input to run.js
                v8.SetParameter("source", input);
                // Pass a boolean that indicates whether the compiler should run type checking to run.js
                v8.SetParameter("typeChecking", _errorLevel != ErrorLevel.NoTypeChecking);
                // Load the scripts from text file resources and concatenate them into one big javascript script
                var compiler = GetScript(Compiler); // the compiler (typescript.js)
                var lib = GetScript(Library); // lib.d.ts
                var run = GetScript(Run); // our run.js script
                var script = compiler + "\n" + lib + "\n" + run;
                // Politely ask V8 to run these 20 000+ lines of javascript
                v8.Run(script);
                // Check if run.js has written anything into a variable called "error"
                var errors = v8.GetParameter("error").ToString();
                if (!string.IsNullOrEmpty(errors)) {
                    output = GetErrorScript(errors);
                }
                // Add the resulting javascript to the output
                output += v8.GetParameter("result").ToString();
                // If we have been told to minify, then minify the output
                if(_minify) {
                    output = (new JavaScriptMinifier()).Minify(output);
                }
            }
            // Spit out the result and set the content type
            response.Content = output;
            response.ContentType = "text/javascript"; 
        }

        // Writes type checking errors to the javascript result as console.error()
        private static string GetErrorScript(string errors) {
            var outp = string.Empty;
            foreach (var error in errors.Split("\n".ToCharArray()[0])) {
                if(!string.IsNullOrEmpty(error)) {
                    outp += string.Format("console.error(\"{0}\");\n", error);
                }
            }
            return outp;
        }

        // Gets a text resource by name, used to load the three scripts
        private static string GetScript(string filename) {
            var script = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(filename)) {
                if (stream != null) {
                    using (var reader = new StreamReader(stream)) {
                        script = reader.ReadToEnd();
                    }
                }
            }
            return script;
        }

        // Handles errors that may occur in run.js
        private class ErrorHandler {

            private readonly bool _throwException;

            // Errors can either throw an exception or be written to the output
            public ErrorHandler(bool throwException) {
                _throwException = throwException;
            }

            // Called from run.js
            public void OnError(string message) {
                if (_throwException) {
                    throw new TypeScriptCompilerException(message);
                }
                Debug.WriteLine(message);
            }
        }

    }

}
