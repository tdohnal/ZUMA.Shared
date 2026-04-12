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
        var namespaceDecl = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
        if (namespaceDecl == null) return;

        string currentNamespace = namespaceDecl.Name.ToString();
        var currentParts = currentNamespace.Split('.');

        var nodes = root.DescendantNodes().OfType<IdentifierNameSyntax>();

        foreach (var node in nodes)
        {
            string identifierText = node.Identifier.Text;
            if (new[] { "string", "long", "Guid", "DateTime", "int", "bool", "var" }.Contains(identifierText)) continue;

            var symbol = context.SemanticModel.GetSymbolInfo(node).Symbol;
            string targetNamespace = symbol?.ContainingNamespace?.ToDisplayString() ?? "";

            // --- TESTOVACÍ KRITÉRIA ---

            bool isViolation = false;
            string targetName = targetNamespace;

            // Pokud jsme v DOMAIN
            if (currentParts.Contains("Domain"))
            {
                // A) Podle symbolu (přesné)
                if (targetNamespace.Contains("Infrastructure") || targetNamespace.Contains("Application"))
                {
                    isViolation = true;
                }
                // B) Podle textu (pokud symbol selže - např. natvrdo napsaný DbContext)
                else if (identifierText.Contains("DbContext") || identifierText.Contains("Repository"))
                {
                    isViolation = true;
                    targetName = "Podezřelý text: " + identifierText;
                }
            }

            // Pokud jsme v APPLICATION
            if (currentParts.Contains("Application"))
            {
                if (targetNamespace.Contains("Infrastructure") || identifierText.Contains("DbContext"))
                {
                    isViolation = true;
                    targetName = string.IsNullOrEmpty(targetNamespace) ? "Infrastructure (odhad)" : targetNamespace;
                }
            }

            if (isViolation)
            {
                // Tady hlásíme skutečnou chybu
                context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(),
                    currentNamespace.Split('.').Last(), // Vrstva (např. Domain)
                    targetName,                         // Co porušil
                    currentNamespace));                 // Celý náš namespace
            }
        }

    }
}