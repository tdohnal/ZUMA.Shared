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
    public const string InfoId = "ZUMA_INFO";

    // Hlavní pravidlo pro chybu
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Porušení Clean Architecture",
        "Vrstva '{0}' nesmí používat typ z '{1}' (Zdroj: {2})",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // Pomocné pravidlo pro potvrzení, že analyzátor běží
    private static readonly DiagnosticDescriptor InfoRule = new DiagnosticDescriptor(
        InfoId,
        "ZUMA Analyzer Status",
        "Analyzátor běží nad souborem v namespace: {0}",
        "Debug",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, InfoRule);

    public override void Initialize(AnalysisContext context)
    {
        // Důležité: Analyzujeme i generovaný kód, abychom měli jistotu, že uvidíme výsledky všude
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSemanticModelAction(AnalyzeModel);
    }

    private void AnalyzeModel(SemanticModelAnalysisContext context)
    {
        var model = context.SemanticModel;
        var root = context.SemanticModel.SyntaxTree.GetRoot();

        // 1. Zjistíme namespace aktuálního souboru
        var firstDeclaration = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
        string currentNamespace = firstDeclaration?.Name.ToString() ?? "Global/Unknown";

        // --- LOG PRO POTVRZENÍ BĚHU ---
        // Vyhodíme informaci na začátku souboru
        context.ReportDiagnostic(Diagnostic.Create(InfoRule, firstDeclaration?.GetLocation() ?? root.GetLocation(), currentNamespace));

        if (string.IsNullOrEmpty(currentNamespace) || currentNamespace == "Global/Unknown") return;

        var currentParts = currentNamespace.Split('.');

        // 2. Projdeme všechny identifikátory typů v souboru
        var typeNodes = root.DescendantNodes().OfType<IdentifierNameSyntax>();

        foreach (var node in typeNodes)
        {
            var symbol = model.GetSymbolInfo(node).Symbol;
            if (symbol == null) continue;

            // Získáme namespace toho typu, na který se díváme
            string targetNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? "";
            var targetParts = targetNamespace.Split('.');

            // --- LOGIKA ARCHITEKTURY ---

            // Pokud jsem v DOMAIN
            if (currentParts.Contains("Domain"))
            {
                // Nesmím používat nic z Infrastructure nebo Application
                if (targetParts.Contains("Infrastructure") || targetParts.Contains("Application"))
                {
                    var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), "Domain", targetNamespace, currentNamespace);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // Pokud jsem v APPLICATION
            if (currentParts.Contains("Application"))
            {
                // Nesmím do Infrastructure
                if (targetParts.Contains("Infrastructure"))
                {
                    var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), "Application", targetNamespace, currentNamespace);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}