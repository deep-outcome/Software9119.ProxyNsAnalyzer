using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Threading.Tasks;

using VerifyCS = Software9119.Test.CSharpCodeFixVerifier<
    Software9119.ProxyNsAnalyzer,
    Software9119.ProxyNsAnalyzerCodeFixProvider>;

namespace Software9119;

[TestClass]
public class Software9119ProxyNsAnalyzerUnitTest
{
  const string PNANSDIFF = ProxyNsAnalyzer.PNANSDIFF;
  const string ProxyNsAnalyzerTestNamespace = "ProxyNsAnalyzer.UnitTest.Namespace";
  
  static string ProxyEditorConfig => $"[*.cs]{Environment.NewLine}proxy_namespace = {ProxyNsAnalyzerTestNamespace}";
  static string EmptyEditorConfig => "[*.cs]";

  static string NonObidientNamespace ( string ns )
  {
    return $@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace {ns}
    {{
        class MyClass
        {{
        }}
    }}";
  }

  const string expectation = $@"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace {ProxyNsAnalyzerTestNamespace}
    {{
        class MyClass
        {{
        }}
    }}";

  [TestMethod]
  async public Task AnalyzerTest ()
  {
    string test = @"";
    await VerifyCS.VerifyAnalyzerAsync ( test );
  }

  [TestMethod]
  [DataRow ( "SimpleNamespace" )]
  [DataRow ( "Qualified.Namespace" )]
  async public Task CodeFixTest ( string ns )
  {
    string test = NonObidientNamespace(ns);
    DiagnosticResult diagnostic = VerifyCS.Diagnostic(PNANSDIFF)
      .WithSpan(9,5,14,6)
      .WithArguments(ns, ProxyNsAnalyzerTestNamespace);

    await VerifyCS.VerifyCodeFixAsync ( test, diagnostic, expectation, ProxyEditorConfig );
  }

  [TestMethod]
  [DataRow ( "SimpleNamespace" )]
  [DataRow ( "Qualified.Namespace" )]
  async public Task CodeFixTest_NoProxyDeclared ( string ns )
  {
    string test = NonObidientNamespace(ns);
    await VerifyCS.VerifyCodeFixAsync ( test, [], test, EmptyEditorConfig );
  }
}
