using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFL.Debugger
{
    public static class DebugLog
    {

        public static void DotNetDebugAssert(bool condition, string? text = null, string? detail = null)
        {
            Debug.Assert(condition, text, detail);
        }

        public static void DotNetPrintDebugAssert( string text)
        {
            Console.WriteLine(text );
        }


        public static void MetodoEjemplo()
        {
            var stackTrace = new StackTrace();

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();

                Console.WriteLine($"Método: {method?.DeclaringType}.{method?.Name}");

                // Si hay símbolos PDB disponibles, podemos obtener archivo y línea
                var fileName = frame?.GetFileName();
                var lineNumber = frame?.GetFileLineNumber();

                if (!string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine($"  Archivo: {fileName}, Línea: {lineNumber}");
                }
            }
        }
        public static Action<bool, string?, string?> DebugAssertFnc = DotNetDebugAssert;
        public static Action< string> LogPrintFnc = DotNetPrintDebugAssert;

        public static void DebugAssert(bool condition,string? text = null, string? detail =null)
        {
            DebugAssertFnc?.Invoke(condition, text, detail);
        }


       
        

        private static bool newLine = true;
        private static int indent = 0;
        public static void LogIndent() {
            indent++;
         
        }
        public static void LogUnindent()
        {
            if ( indent> 0 ) 
            indent--;
        }

       
        public static void Log(string text="", ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            var bAnterior = Console.BackgroundColor;
            var fAnterior = Console.ForegroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
          //  LogPrintFnc?.Invoke($"{new string('\t', indent)}{text}");
            Console.WriteLine($"{new string(' ', indent * 2)}{text}");
            Console.BackgroundColor = bAnterior;
            Console.ForegroundColor = fAnterior;

        }
    }
}
