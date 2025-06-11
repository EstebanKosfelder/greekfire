using System;
using System.Collections.Generic;
using System.Diagnostics;
using FT = System.Double;

namespace CGAL
{
    public static class Extensions
    {
    

        public static void Precondition(bool value, string? msg = null)
        {
            if (!value)
                throw new Exception(msg);
        }

        public static void postcondition(bool value, string? msg = null)
        {
            if (!value)
                throw new Exception(msg);
        }

        public static void assertion(bool value, string? msg = null)
        {
            if (!value)
                throw new Exception(msg);
        }
        
        public static void assert(bool value, string msg)
        {
            if (!value)
                throw new Exception(msg);
        }


        public static void push_back<T>(this List<T> list, T item) => list.Add(item);

        public static T Last<T>(this List<T> list) => list[list.Count - 1];

        public static T First<T>(this List<T> list) => list[0];


      


    }
}