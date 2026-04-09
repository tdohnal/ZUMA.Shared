using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InternalArchitectureAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ZUMA002";
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Porušení Clean Architecture (Internal)",
        "Složka '{0}' nesmí záviset na '{1}' (Namespace: {2})",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        // Sledujeme usingy v každém souboru
        context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
    }

    private void AnalyzeUsing(SyntaxNodeAnalysisContext context)
    {
        var usingDirective = (UsingDirectiveSyntax)context.Node;
        var importedNamespace = usingDirective.Name.ToString();

        // Zjistíme, kde se nachází aktuální soubor (podle jeho namespace)
        var model = context.SemanticModel;
        var originalNamespace = context.ContainingSymbol?.ContainingNamespace?.ToDisplayString() ?? "";

        // --- LOGIKA RESTRIKCÍ ---

        // 1. Pokud jsem v DOMAIN
        if (originalNamespace.Contains(".Domain"))
        {
            // Nesmím tahat Application ani Infrastructure
            if (importedNamespace.Contains(".Application") || importedNamespace.Contains(".Infrastructure"))
            {
                ReportError(context, usingDirective, "Domain", importedNamespace);
            }
        }

        // 2. Pokud jsem v APPLICATION
        if (originalNamespace.Contains(".Application"))
        {
            // Nesmím tahat Infrastructure (Application má být nezávislá na implementaci)
            if (importedNamespace.Contains(".Infrastructure"))
            {
                ReportError(context, usingDirective, "Application", importedNamespace);
            }
        }

        // 3. Mezi-modulová komunikace (Communication vs Customer)
        // Pokud jsem v modulu Communication, nesmím napřímo sahat do Customer.Infrastructure
        if (originalNamespace.Contains(".Communication") && importedNamespace.Contains(".Customer"))
        {
            if (importedNamespace.Contains(".Infrastructure") || importedNamespace.Contains(".Domain"))
            {
                ReportError(context, usingDirective, "Communication", importedNamespace);
            }
        }
    }

    private void ReportError(SyntaxNodeAnalysisContext context, UsingDirectiveSyntax node, string layer, string imported)
    {
        var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), layer, imported, imported);
        context.ReportDiagnostic(diagnostic);
    }
}