using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace Analyzer1
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Analyzer1Analyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    Descriptors.ClassTriviaDescriptor,
                    Descriptors.MethidPublicTriviaDescriptor,
                    Descriptors.MethodTaskDescriptor,
                    Descriptors.MethodTaskGenericDescriptor
                );
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            Debugger.Launch();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(SyntaxNodeAnalysisContext, SyntaxKind.ClassDeclaration);
        }

        private static void SyntaxNodeAnalysisContext(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is ClassDeclarationSyntax classDeclaration)
            {
                var classTriviaList = classDeclaration.GetLeadingTrivia();

                //检测实体是否有注释
                if (!classTriviaList.Any())
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.ClassTriviaDescriptor, classDeclaration.GetLocation()));

                var methods = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
                var publicToken = SyntaxFactory.Token(SyntaxKind.PublicKeyword);

                foreach (var method in methods)
                {
                    //检测Public方法是否有注释
                    if (method.Modifiers.Any(_modifier => _modifier.ValueText == publicToken.ValueText))
                    {
                        var methodTriviaList = method.GetLeadingTrivia();

                        if (!methodTriviaList.Any())
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.MethidPublicTriviaDescriptor, method.GetLocation(), method.Identifier.ValueText));
                    }

                    if (method.ReturnType is SimpleNameSyntax simple)
                    {
                        //检测Task方法是否有异常注释
                        if (simple.Identifier.ValueText == "Task")
                        {
                            var taskSymbol = context.SemanticModel.GetSymbolInfo(simple);

                            var methodTriviaList = method.GetLeadingTrivia();
                            var xmlStr = methodTriviaList.ToFullComment();
                            var xmlDoc = new XmlDocument();

                            xmlDoc.LoadXml(xmlStr);

                            var nodes = xmlDoc.SelectNodes("/Root/exception");

                            if (nodes.Count == 0)
                            {
                                if (simple is IdentifierNameSyntax)
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.MethodTaskGenericDescriptor, method.GetLocation(), method.Identifier.ValueText));

                                if (simple is GenericNameSyntax)
                                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.MethodTaskDescriptor, method.GetLocation(), method.Identifier.ValueText));
                            }
                            else
                            {
                                foreach(XmlNode node in nodes)
                                {
                                    if (string.IsNullOrWhiteSpace(node.InnerText))
                                    {
                                        if (simple is IdentifierNameSyntax)
                                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.MethodTaskGenericDescriptor, method.GetLocation(), method.Identifier.ValueText));

                                        if (simple is GenericNameSyntax)
                                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.MethodTaskDescriptor, method.GetLocation(), method.Identifier.ValueText));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    internal static class SyntaxTriviaListExtension
    {
        public static string ToFullComment(this SyntaxTriviaList trivias)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("<Root>");

            foreach (var trivia in trivias.Where(_trivia => _trivia.Kind() == SyntaxKind.SingleLineCommentTrivia))
            {
                var triviaStr = trivia.ToFullString();

                if (triviaStr.Contains("/// "))
                    stringBuilder.AppendLine($"{triviaStr.Substring(4)}");
            }

            stringBuilder.Append("</Root>");

            return stringBuilder.ToString();
        }
    }
}
