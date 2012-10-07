// by Wouter Devinck (www.wouterdevinck.be)

using System;

namespace TypeScriptBundleTransform {

    public class TypeScriptCompilerException : Exception {

        public TypeScriptCompilerException(string message)
            : base(message) {}

    }

}