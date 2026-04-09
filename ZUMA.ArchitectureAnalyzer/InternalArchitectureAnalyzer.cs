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

        // LOG 1: Žiju a vidím namespace souboru
        context.ReportDiagnostic(Diagnostic.Create(Rule, namespaceDecl.Name.GetLocation(), "DEBUG-START", "Namespace souboru", currentNamespace));

        var nodes = root.DescendantNodes().OfType<IdentifierNameSyntax>();

        foreach (var node in nodes)
        {
            // Nechceme logovat úplně všechno (vynecháme primitivní typy jako string, long atd.)
            string nodeText = node.Identifier.Text;
            if (new[] { "string", "long", "Guid", "DateTime", "int", "bool" }.Contains(nodeText)) continue;

            var symbol = context.SemanticModel.GetSymbolInfo(node).Symbol;

            if (symbol == null)
            {
                // LOG 2: Našel jsem slovo, ale nevím, co to je za typ (Chybějící reference?)
                context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), "DEBUG-NULL", nodeText, "Symbol nebyl nalezen"));
                continue;
            }

            string targetNamespace = symbol.ContainingNamespace?.ToDisplayString() ?? "Global";

            // LOG 3: Tohle uvidíš u CommunicationDbContext - zjistíme, co v tom targetNamespace reálně je
            context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), "DEBUG-CHECK", nodeText, targetNamespace));

            // --- PŮVODNÍ LOGIKA (zatím nechaná, ale logy mají přednost) ---
            var currentParts = currentNamespace.Split('.');
            var targetParts = targetNamespace.Split('.');

            if (currentParts.Contains("Domain") && (targetParts.Contains("Infrastructure") || targetParts.Contains("Application")))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), "REAL-ERROR", targetNamespace, currentNamespace));
            }
        }
    }
}