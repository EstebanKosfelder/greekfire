
using System.Diagnostics;


namespace CGAL
{
    public static class DebuggerInfo
    {

        public static int CGAL_STRAIGHT_SKELETON_ENABLE_TRACE = 4;
        public static bool CGAL_STRAIGHT_SKELETON_TRAITS_ENABLE_TRACE = true;
        public static int CGAL_POLYGON_OFFSET_ENABLE_TRACE = 4;



        public static void ExceptionHandler(ArithmeticException ex)
        {
            Console.Error.WriteLine(ex);
            Debug.WriteLine(ex);
        }



        public static void CGAL_STSKEL_VALIDITY_TRACE(params string[] m)=> CGAL_STSKEL_TRACE(string.Join("\n", m));

        public static void CGAL_STSKEL_VALIDITY_TRACE_IF( bool value, params string[] m)
        {
            if ( value ) CGAL_STSKEL_VALIDITY_TRACE(m);
        }

        public static void CGAL_STSKEL_TRAITS_TRACE( params string[] m) => CGAL_STSKEL_TRACE( string.Join("\n", m));
        public static void CGAL_STSKEL_BUILDER_TRACE(int l, params string[] m) => CGAL_STSKEL_BUILDER_TRACE(l,string.Join("\n",m));
        public static void CGAL_STSKEL_BUILDER_TRACE(int l, string m) { if (l <= CGAL_STRAIGHT_SKELETON_ENABLE_TRACE) CGAL_STSKEL_TRACE(m); }


        public static void CGAL_STSKEL_BUILDER_TRACE_IF(bool c, int l, string m)
        {
            if ((c) && l <= CGAL_STRAIGHT_SKELETON_ENABLE_TRACE) CGAL_STSKEL_TRACE(m);
        }

        public static void CGAL_STSKEL_TRACE(string m)
        {
            Console.WriteLine(m);
            Debug.WriteLine(m);
        }
        public static void CGAL_precondition(bool value, string? msg = null)
        {
            CGAL.Extensions.Precondition(value, msg);
        }
        public static void CGAL_postcondition(bool value, string? msg = null) => CGAL.Extensions.postcondition(value, msg);
        public static void CGAL_kernel_precondition(bool value, string? msg = null) => CGAL.Extensions.Precondition(value, msg);

        public static void CGAL_assertion(bool value,string? msg=null) => CGAL.Extensions.assertion(value,msg);

        public static void CGAL_kernel_assertion(bool value) => CGAL.Extensions.assertion(value);
       


    }
}
