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
        "ZUMA ARCHITEKTURA",
        "Chyba: Vrstva '{0}' nesmí používat '{1}'",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction(AnalyzeModel);
    }

    private void AnalyzeModel(SemanticModelAnalysisContext context)
    {
        var root = context.SemanticModel.SyntaxTree.GetRoot();
        var firstNamespace = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();

        if (firstNamespace == null) return;

        string currentNamespace = firstNamespace.Name.ToString();

        // --- TOTÁLNÍ TEST ---
        // Tohle MUSÍ vyhodit chybu v každém souboru, co má namespace.
        // Pokud to neuvidíš, analyzátor se vůbec nespustil.
        context.ReportDiagnostic(Diagnostic.Create(Rule, firstNamespace.Name.GetLocation(), "TEST", "BĚŽÍM", currentNamespace));

        // --- REÁLNÁ LOGIKA ---
        var typeNodes = root.DescendantNodes().OfType<IdentifierNameSyntax>();
        foreach (var node in typeNodes)
        {
            var symbol = context.SemanticModel.GetSymbolInfo(node).Symbol;
            if (symbol == null) continue;

            string targetNs = symbol.ContainingNamespace?.ToDisplayString() ?? "";

            // Domain -> Infrastructure
            if (currentNamespace.Contains("Domain") && targetNs.Contains("Infrastructure"))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), "Domain", targetNs, currentNamespace));
            }
        }
    }
}