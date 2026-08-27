using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

using System.Threading;
using System.Threading.Tasks;

namespace Software9119.Test;

static public partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(string)"/>
  static public DiagnosticResult Diagnostic ( string diagnosticId )
      => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, MSTestVerifier>.Diagnostic ( diagnosticId );

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
  static async public Task VerifyAnalyzerAsync ( string source, params DiagnosticResult [] expected )
  {
    Test test = new ()
    {
      TestCode = source,
    };

    test.ExpectedDiagnostics.AddRange ( expected );
    await test.RunAsync ( CancellationToken.None );
  }

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult, string)"/>
  static async public Task VerifyCodeFixAsync ( string source, DiagnosticResult expected, string fixedSource, string editorConfig )
      => await VerifyCodeFixAsync ( source, [ expected ], fixedSource, editorConfig );

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)"/>
  static async public Task VerifyCodeFixAsync ( string source, DiagnosticResult [] expected, string fixedSource, string editorConfig )
  {
    Test test = new ()
    {
      TestCode = source,
      FixedCode = fixedSource,
    };

    test.TestState.AnalyzerConfigFiles.Add ( ("/.editorconfig", editorConfig) );

    test.ExpectedDiagnostics.AddRange ( expected );
    await test.RunAsync ( CancellationToken.None );
  }
}
