using System.Web.Optimization;
using TypeScriptBundleTransform;

// ReSharper disable CheckNamespace

namespace System.Web.Mvc {

    public static class BundleHelper {

        // An extension method for the UrlHelper (just a shorter version of the build-in function)
        // i.e. Url.BundleUrl() instead of System.Web.Optimization.BundleTable.Bundles.ResolveBundleUrl()
        public static string BundleUrl(this UrlHelper url, string path) {
            return BundleTable.Bundles.ResolveBundleUrl(path);
        }

        // Called from Application_Start in Global.asax.cs
        public static void RegisterBundles() {
            var bundle = new Bundle("~/default", new TypeScriptTransformer());
            bundle.Include("~/content/test.ts");
            BundleTable.Bundles.Add(bundle);
        }

    }

}

// ReSharper restore CheckNamespace