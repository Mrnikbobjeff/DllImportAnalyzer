using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using DllImportDefaultsAnalyzer;
using System.Runtime.InteropServices;

namespace DllImportDefaultsAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void EmptyTest_NoResults()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }
        [TestMethod]
        public void DllImport_BoolParameterSingleFix()
        {
            var test = @"
    using System;
    using System.Runtime.InteropServices;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            [DllImport(""abc"", BestFitMapping = false)]
            public static extern void Test();
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "DllImportDefaultsAnalyzer",
                Message = String.Format("DllImport Parameter '{0}' is default value", "BestFitMapping"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 31)
                        }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void DllImportSingleFix()
        {
            var test = @"
    using System;
    using System.Runtime.InteropServices;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            [DllImport(""abc"", BestFitMapping = false)]
            public static extern void Test();
        }
    }";
            var expected = @"
    using System;
    using System.Runtime.InteropServices;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            [DllImport(""abc"")]
            public static extern void Test();
        }
    }";
            VerifyCSharpFix(test, expected);
        }

        [TestMethod]
        public void DllImportSingleFix_TwoParameter()
        {
            var test = @"
    using System;
    using System.Runtime.InteropServices;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            [DllImport(""abc"", CharSet = CharSet.Unicode, BestFitMapping = false)]
            public static extern void Test();
        }
    }";
            var expected = @"
    using System;
    using System.Runtime.InteropServices;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            [DllImport(""abc"", CharSet = CharSet.Unicode)]
            public static extern void Test();
        }
    }";
            VerifyCSharpFix(test, expected);
        }

        [TestMethod]
        public void DllImportSingleFix_CallingConvention()
        {
            var test = @"
    using System;
    using System.Runtime.InteropServices;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            [DllImport(""abc"", CallingConvention = CallingConvention.WinApi)]
            public static extern void Test();
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "DllImportDefaultsAnalyzer",
                Message = String.Format("DllImport Parameter '{0}' is default value", "CallingConvention"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 31)
                        }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void DllImportSingleFix_CharsetParameter()
        {
            var test = @"
    using System;
    using System.Runtime.InteropServices;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            [DllImport(""abc"", CharSet = CharSet.Ansi)]
            public static extern void Test();
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "DllImportDefaultsAnalyzer",
                Message = String.Format("DllImport Parameter '{0}' is default value", "CharSet"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 31)
                        }
            };
            VerifyCSharpDiagnostic(test, expected);
        }
        [TestMethod]
        public void DllImportSingleFix_StringParameter()
        {
            var test = @"
    using System;
    using System.Runtime.InteropServices;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            [DllImport(""abc"", EntryPoint = null)]
            public static extern void Test();
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "DllImportDefaultsAnalyzer",
                Message = String.Format("DllImport Parameter '{0}' is default value", "EntryPoint"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 31)
                        }
            };
            VerifyCSharpDiagnostic(test, expected);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DllImportDefaultsAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DllImportDefaultsAnalyzerAnalyzer();
        }
    }
}
