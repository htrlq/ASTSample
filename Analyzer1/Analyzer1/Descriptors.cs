using Microsoft.CodeAnalysis;

namespace Analyzer1
{
    static class Descriptors
    {
        public static DiagnosticDescriptor ClassTriviaDescriptor { get; }
           = Create("C1001", "Class", "实体需要编写备注", DiagnosticSeverity.Error);

        public static DiagnosticDescriptor MethidPublicTriviaDescriptor { get; }
            = Create("C1002", "Method", "公开方法:{0} 需要编写备注", DiagnosticSeverity.Error);

        public static DiagnosticDescriptor MethodTaskDescriptor { get; }
           = Create("C1003", "Method", "Task方法:{0} 上需要写异常描述", DiagnosticSeverity.Error);

        public static DiagnosticDescriptor MethodTaskGenericDescriptor { get; }
            = Create("C1004", "Method", "Task泛型方法:{0} 上需要写异常描述", DiagnosticSeverity.Error);

        /// <summary>
        /// 创建诊断描述器
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="helpLinkUri"></param>
        /// <returns></returns>
        private static DiagnosticDescriptor Create(string id, string title, string message, DiagnosticSeverity level = DiagnosticSeverity.Error)
        {
            var category = level.ToString();
            return new DiagnosticDescriptor(id, title, message, category, level, true, null);
        }
    }
}
