namespace Software9119;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Resources;
using System.Threading;

using static Resources;

[DiagnosticAnalyzer ( LanguageNames.CSharp )]
sealed public class ProxyNsAnalyzer : DiagnosticAnalyzer
{
  public const string NewNsKey = "ProxyNsAnalyzer:NewNs";
  public const string PNANSDIFF = "PNANSDIFF";

  const string cfgName = "proxy_namespace";
  // https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/categories
  const string cat = "Naming";

  static ResourceManager ResourceManager => Resources.ResourceManager;
  static Type TypeOfResources => typeof ( Resources );

  static readonly LocalizableResourceString title = new (nameof(AnalyzerTitle), ResourceManager, TypeOfResources);
  static readonly LocalizableResourceString format = new (nameof(AnalyzerMessageFormat), ResourceManager, TypeOfResources);
  static readonly LocalizableResourceString desc = new (nameof(AnalyzerDescription), ResourceManager, TypeOfResources);

  static readonly DiagnosticDescriptor Rule = new(
        PNANSDIFF,
        title: title,
        messageFormat: format,
        cat,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: desc);

  static readonly ImmutableArray<DiagnosticDescriptor> rules = [ Rule ];
  override public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => rules;

  override public void Initialize ( AnalysisContext context )
  {
    if (context == null)
      return;

    context.ConfigureGeneratedCodeAnalysis ( GeneratedCodeAnalysisFlags.ReportDiagnostics );
    context.EnableConcurrentExecution ();
    context.RegisterSyntaxNodeAction ( AnalyzeNamespace, SyntaxKind.NamespaceDeclaration );
    context.RegisterSyntaxNodeAction ( AnalyzeNamespace, SyntaxKind.FileScopedNamespaceDeclaration );
  }

  void AnalyzeNamespace ( SyntaxNodeAnalysisContext context )
  {
    try
    {
      AnalyzeNamespaceCore ( context );
    }
    catch (Exception e)
    {
      Debug.WriteLine ( e.ToString () );
    }
  }

  static void AnalyzeNamespaceCore ( SyntaxNodeAnalysisContext context )
  {
    CancellationToken ct = context.CancellationToken;
    if (context.Node is not BaseNamespaceDeclarationSyntax nsDeclaration || ct.IsCancellationRequested)
      return;

    string? ns = null;
    try
    {
      ISymbol? nsSymbol = context.SemanticModel.GetDeclaredSymbol(nsDeclaration, ct);
      ns = nsSymbol?.ToDisplayString ();
    }
    catch (Exception e)
    {
      Debug.WriteLine ( e.ToString () );
    }

    if (string.IsNullOrWhiteSpace ( ns ))
      ns = nsDeclaration.Name?.ToString ();

    if (ns == null || ct.IsCancellationRequested)
      return;

    string? proxyNs = ProxyNs ( context, nsDeclaration.SyntaxTree );
    if (proxyNs == null || ns == proxyNs || ct.IsCancellationRequested)
      return;

    Diagnostic diagnostic = CreateDiagnostic ( nsDeclaration.GetLocation(), ns, proxyNs );

    if (ct.IsCancellationRequested)
      return;

    context.ReportDiagnostic ( diagnostic );
  }

  static string? ProxyNs ( SyntaxNodeAnalysisContext context, SyntaxTree syntaxTree )
  {
    AnalyzerConfigOptions cfg = context.Options.AnalyzerConfigOptionsProvider.GetOptions ( syntaxTree );

    string? proxyNs = null;
    if (cfg.TryGetValue ( cfgName, out string? proxyValue ) == true)
      if (string.IsNullOrWhiteSpace ( proxyValue ) == false)
        proxyNs = proxyValue.Trim ();
    return proxyNs;
  }

  static Diagnostic CreateDiagnostic ( Location location, string ns, string proxyNs )
  {
    KeyValuePair<string, string?> [] keyValuePairs =
    [
      new(NewNsKey, proxyNs),
    ];

    ImmutableDictionary<string, string?> properties = ImmutableDictionary.CreateRange(keyValuePairs );
    Diagnostic diagnostic = Diagnostic.Create(Rule, location, properties, [ns, proxyNs]);

    return diagnostic;
  }
}

