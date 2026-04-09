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
        "Vrstva '{0}' nesmí záviset na '{1}' (Aktuální namespace: {2})",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsing, SyntaxKind.UsingDirective);
    }

    private void AnalyzeUsing(SyntaxNodeAnalysisContext context)
    {
        var usingDirective = (UsingDirectiveSyntax)context.Node;
        var importedNamespace = usingDirective.Name.ToString().Trim();

        // Zjistíme namespace aktuálního souboru
        var namespaceDeclaration = usingDirective.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        string currentNamespace = namespaceDeclaration?.Name.ToString() ?? "";

        if (string.IsNullOrEmpty(currentNamespace))
        {
            var symbol = context.SemanticModel.GetEnclosingSymbol(usingDirective.SpanStart);
            currentNamespace = symbol?.ContainingNamespace?.ToDisplayString() ?? "";
        }

        var currentParts = currentNamespace.Split('.');
        var importedParts = importedNamespace.Split('.');

        // --- UNIVERZÁLNÍ ARCHITEKTONICKÁ POLICIE ---

        // 1. Pravidlo: DOMAIN je střed vesmíru. Nesmí znát nic "venkovního".
        if (currentParts.Contains("Domain"))
        {
            if (importedParts.Contains("Application") || importedParts.Contains("Infrastructure") || importedParts.Contains("API"))
            {
                ReportError(context, usingDirective, "Domain", importedNamespace, currentNamespace);
                return;
            }
        }

        // 2. Pravidlo: APPLICATION smí jen do Domain. Nesmí do Infrastructure ani API.
        if (currentParts.Contains("Application"))
        {
            if (importedParts.Contains("Infrastructure") || importedParts.Contains("API"))
            {
                ReportError(context, usingDirective, "Application", importedNamespace, currentNamespace);
                return;
            }
        }

        // 3. Pravidlo: INFRASTRUCTURE smí do Domain a Application, ale nesmí do API (kruhová závislost).
        if (currentParts.Contains("Infrastructure"))
        {
            if (importedParts.Contains("API"))
            {
                ReportError(context, usingDirective, "Infrastructure", importedNamespace, currentNamespace);
                return;
            }
        }
    }

    private void ReportError(SyntaxNodeAnalysisContext context, UsingDirectiveSyntax node, string layer, string imported, string currentNs)
    {
        var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), layer, imported, currentNs);
        context.ReportDiagnostic(diagnostic);
    }
}