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
        "Porušení Clean Architecture",
        "Vrstva '{0}' nesmí používat typ z '{1}' (Aktuální namespace: {2})",
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

        // Získáme namespace aktuálního souboru
        var namespaceDecl = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
        if (namespaceDecl == null) return;

        string currentNamespace = namespaceDecl.Name.ToString();
        var currentParts = currentNamespace.Split('.');

        // Projdeme všechny identifikátory (typy, vlastnosti, atd.)
        var nodes = root.DescendantNodes().OfType<IdentifierNameSyntax>();

        foreach (var node in nodes)
        {
            var symbol = context.SemanticModel.GetSymbolInfo(node).Symbol;
            if (symbol == null) continue;

            // Získáme namespace typu, na který uzel odkazuje
            string targetNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? "";
            var targetParts = targetNamespace.Split('.');

            // --- LOGIKA ZÁVISLOSTÍ ---

            // 1. Jsem v DOMAIN (Nesmí nikam ven)
            if (currentParts.Contains("Domain"))
            {
                if (targetParts.Contains("Application") || targetParts.Contains("Infrastructure") || targetParts.Contains("API"))
                {
                    // Výjimka: Povolit System, Microsoft a SharedKernel namespaces
                    if (!targetNamespace.StartsWith("System") && !targetNamespace.StartsWith("Microsoft") && !targetNamespace.Contains("SharedKernel"))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), "Domain", targetNamespace, currentNamespace));
                    }
                }
            }

            // 2. Jsem v APPLICATION (Smí jen do Domain)
            if (currentParts.Contains("Application"))
            {
                if (targetParts.Contains("Infrastructure") || targetParts.Contains("API"))
                {
                    if (!targetNamespace.StartsWith("System") && !targetNamespace.StartsWith("Microsoft") && !targetNamespace.Contains("SharedKernel"))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), "Application", targetNamespace, currentNamespace));
                    }
                }
            }
        }
    }
}