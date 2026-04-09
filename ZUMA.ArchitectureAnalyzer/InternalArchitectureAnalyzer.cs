using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

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

        // Získáme namespace jako pole (např. ["ZUMA", "CommunicationService", "Domain", "Entities"])
        var symbol = context.ContainingSymbol;
        var originalNamespace = symbol?.ContainingNamespace?.ToDisplayString() ?? "";
        var namespaceParts = originalNamespace.Split('.');

        // --- LOGIKA RESTRIKCÍ POMOCÍ POLE ---

        // 1. Pokud jsem KDEKOLIV v Domain (např. ZUMA.Module.Domain nebo ZUMA.Domain.Entities)
        if (namespaceParts.Contains("Domain"))
        {
            // V Domain nesmí být žádný using na Application nebo Infrastructure
            // Použijeme opět Split, aby to bylo přesné
            var importedParts = importedNamespace.Split('.');

            if (importedParts.Contains("Application") || importedParts.Contains("Infrastructure"))
            {
                ReportError(context, usingDirective, "Domain", importedNamespace);
            }
        }

        // 2. Pokud jsem v Application
        if (namespaceParts.Contains("Application"))
        {
            if (importedNamespace.Split('.').Contains("Infrastructure"))
            {
                ReportError(context, usingDirective, "Application", importedNamespace);
            }
        }
    }

    private void ReportError(SyntaxNodeAnalysisContext context, UsingDirectiveSyntax node, string layer, string imported)
    {
        var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), layer, imported, imported);
        context.ReportDiagnostic(diagnostic);
    }
}