using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Software9119;

[ExportCodeFixProvider ( LanguageNames.CSharp, Name = nameof ( ProxyNsAnalyzerCodeFixProvider ) ), Shared]
public class ProxyNsAnalyzerCodeFixProvider : CodeFixProvider
{
  static readonly ImmutableArray<string> fixableDiagnosticIds = [ ProxyNsAnalyzer.PNANSDIFF ];
  sealed override public ImmutableArray<string> FixableDiagnosticIds => fixableDiagnosticIds;

  sealed override public FixAllProvider GetFixAllProvider () => WellKnownFixAllProviders.BatchFixer;


  async sealed override public Task RegisterCodeFixesAsync ( CodeFixContext context )
  {
    try
    {
      await RegisterCodeFixesCoreAsync ( context );
    }
    catch (Exception e)
    {
      Debug.WriteLine ( e.ToString () );
    }
  }
  static async Task RegisterCodeFixesCoreAsync ( CodeFixContext context )
  {
    Document document = context.Document;
    SyntaxNode? root = await document.GetSyntaxRootAsync(context.CancellationToken);

    if (root == null)
      return;

    Diagnostic diagnostic = context.Diagnostics.First();
    TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

    BaseNamespaceDeclarationSyntax? declaration = root
      .FindToken(diagnosticSpan.Start)
      .Parent
      ?.AncestorsAndSelf()
      .OfType<BaseNamespaceDeclarationSyntax>()
      .First();

    if (declaration == null)
      return;

    Func<CancellationToken, Task<Solution>> changeSolution = c => UseProxyNs ( document, declaration, diagnostic, c );
    CodeAction codeAction = CodeAction.Create
    (
      title: CodeFixResources.CodeFixTitle,
      createChangedSolution: changeSolution,
      equivalenceKey: nameof ( CodeFixResources.CodeFixTitle )
    );
    context.RegisterCodeFix ( codeAction, diagnostic );
  }

  static async Task<Solution> UseProxyNs ( Document document, BaseNamespaceDeclarationSyntax ns, Diagnostic diagnostic, CancellationToken ct )
  {
    SemanticModel? semanticModel = await document.GetSemanticModelAsync(ct);

    Solution solution = Solution(document);
    if (semanticModel == null)
      return solution;

    ISymbol? typeSymbol = semanticModel.GetDeclaredSymbol(ns, ct);
    if (typeSymbol == null)
      return solution;

    string newNs = diagnostic.Properties[ProxyNsAnalyzer.NewNsKey]!;
    SyntaxNode? root = await document.GetSyntaxRootAsync(ct);
    if (root == null)
      return solution;

    NameSyntax nsSyntax = SyntaxFactory.ParseName(newNs);
    SyntaxNode newRoot = root.ReplaceNode(ns.Name, nsSyntax.WithTriviaFrom(ns.Name));

    document = document.WithSyntaxRoot ( newRoot );
    return Solution ( document );
  }

  static Solution Solution ( Document doc ) => doc.Project.Solution;
}
