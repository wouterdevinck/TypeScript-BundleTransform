TypeScript BundleTransform
==========================

A bundle transform for ASP.NET that takes a TypeScript bundle and compiles it to Javascript.

by Wouter Devinck (http://www.wouterdevinck.be)

Usage
-----
```C#
// Called from Application_Start in Global.asax.cs
public static void RegisterBundles() {
   var bundle = new Bundle("~/default", new TypeScriptTransformer());
   bundle.Include("~/content/test.ts");
   BundleTable.Bundles.Add(bundle);
}
```

With type checking (throw exception) and whitout minification:
```C#
new TypeScriptTransformer();
```
or
```C#
new TypeScriptTransformer(TypeScriptTransformer.ErrorLevel.ThrowException, false)
```

Without type checking and whitout minification:
```C#
new TypeScriptTransformer(TypeScriptTransformer.ErrorLevel.NoTypeChecking, false);
```

With type checking (log errors in Visual Studio output and as console.error lines in the Javascript output):
```C#
new TypeScriptTransformer(TypeScriptTransformer.ErrorLevel.LogError, false);
```

With minification:
```C#
new TypeScriptTransformer(true);
```
or any of the above with true as the second parameter.

How it works
------------
This uses the TypeScript compiler, which is written in TypeScript and compiled to Javascript and runs it in Google's V8 Javascript Engine on your server.
This is an implementation of the IBundleTransform interface, found in System.Web.Optimization.

NuGet
-----
Coming soon

License
-------
The following licenses apply:

1. TypeScript-BundleTransform        - Apache License 2.0   - https://github.com/wouterdevinck/TypeScript-BundleTransform
2. TypeScript                        - Apache License 2.0   - http://typescript.codeplex.com
3. Noesis Inovation Javascript.NET   - New BSD License      - http://javascriptdotnet.codeplex.com
4. Google V8                         - New BSD License      - http://code.google.com/p/v8/
5. Douglas Crockford's jsmin.c       - License below        - http://www.crockford.com
6. Inspired by: TypeScript-compile   - Apache License 2.0   - https://github.com/niutech/typescript-compile/

See [license.txt](https://github.com/wouterdevinck/TypeScript-BundleTransform/blob/master/license.txt) for more info.

More info
---------
* [The TypeScript website](http://www.typescriptlang.org/)
* [The TypeScript source code](http://typescript.codeplex.com/)