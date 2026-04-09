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
        var importedNamespace = usingDirective.Name.ToString().Trim();

        // Takhle vytáhneme namespace spolehlivěji přímo z místa, kde je ten using
        var semanticModel = context.SemanticModel;
        var enclosingNamespace = semanticModel.GetEnclosingNamespace(usingDirective.SpanStart);
        var originalNamespace = enclosingNamespace?.ToDisplayString() ?? "";

        // Pro debugování (pokud by to pořád nešlo, tohle ti v Error Listu ukáže, co analyzátor vidí)
        // context.ReportDiagnostic(Diagnostic.Create(Rule, usingDirective.GetLocation(), "DEBUG", importedNamespace, originalNamespace));

        // Použijeme tvůj nápad s detekcí klíčových slov v namespace
        bool isInDomain = originalNamespace.Split('.').Contains("Domain");
        bool isInApplication = originalNamespace.Split('.').Contains("Application");
        bool isInCommunication = originalNamespace.Split('.').Contains("Communication");

        // 1. Restrikce pro DOMAIN
        if (isInDomain)
        {
            if (importedNamespace.Contains(".Application") || importedNamespace.Contains(".Infrastructure"))
            {
                ReportError(context, usingDirective, "Domain", importedNamespace, originalNamespace);
            }
        }

        // 2. Restrikce pro APPLICATION
        if (isInApplication)
        {
            if (importedNamespace.Contains(".Infrastructure"))
            {
                ReportError(context, usingDirective, "Application", importedNamespace, originalNamespace);
            }
        }

        // 3. Mezi-modulová komunikace
        if (isInCommunication && importedNamespace.Contains(".CustomerService"))
        {
            if (importedNamespace.Contains(".Infrastructure") || importedNamespace.Contains(".Domain"))
            {
                ReportError(context, usingDirective, "Communication", importedNamespace, originalNamespace);
            }
        }
    }

    private void ReportError(SyntaxNodeAnalysisContext context, UsingDirectiveSyntax node, string layer, string imported)
    {
        var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), layer, imported, imported);
        context.ReportDiagnostic(diagnostic);
    }
}