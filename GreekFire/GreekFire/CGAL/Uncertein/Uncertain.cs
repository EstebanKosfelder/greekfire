using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static CGAL.DebuggerInfo;

namespace CGAL
{



    /*
template <>
struct Minmax_traits <bool>
{
static const bool min = false;
static const bool max = true;
};

template <>
struct Minmax_traits <Sign>
{
static const Sign min = NEGATIVE;
static const Sign max = POSITIVE;
};

template <>
struct Minmax_traits <Bounded_side>
{
static const Bounded_side min = ON_UNBOUNDED_SIDE;
static const Bounded_side max = ON_BOUNDED_SIDE;
};

template <>
struct Minmax_traits <Angle>
{
static const Angle min = OBTUSE;
static const Angle max = ACUTE;
};
} // namespace internal

// Exception type for the automatic conversion.
class Uncertain_conversion_exception
: public std::range_error
{
public:
Uncertain_conversion_exception(const std::string &s)
: std::range_error(s) {}
};

// Encodes a range [inf,sup] of values of type T.
// T can be enums or bool.
*/

    public class UncertainConversionException : ArithmeticException
    {
        public UncertainConversionException() : base()
        {
        }

        public UncertainConversionException(string message) : base(message)
        {
        }

        public UncertainConversionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public interface IUncertain<T>
    {
        T inf();
        bool is_certain();
        bool is_same(Uncertain<T> other);
        T make_certain();
        T sup();
    }

    public abstract class Uncertain<T> : IUncertain<T>
    {
#pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.
        public static Func<T, T, int> Compare;
#pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.
        public T _i, _s;

        public Uncertain(T t)
        {
            _i = (t); _s = (t);
        }

        public Uncertain(T i, T s)
        {
            {
                CGAL_precondition(Compare(i, s) < 1);

                _i = (i); _s = (s);
            }
        }

        public virtual bool is_same(Uncertain<T> other) => EqualityComparer<T>.Default.Equals(_i, other._i) && EqualityComparer<T>.Default.Equals(_s, other._s);

        public virtual bool is_certain() => EqualityComparer<T>.Default.Equals(_i, _s);

        public virtual T make_certain()
        {
            if (is_certain())
                return _i;
            throw new UncertainConversionException($"Undecidable conversion of{nameof(Uncertain<T>)}");
        }

        public static explicit operator T(Uncertain<T> uncertain) => uncertain.make_certain();

        public T inf() => _i;

        public T sup() => _s;
    }

    public class UncertainBool : Uncertain<bool>
    {
        static UncertainBool()
        {
            Compare = (a, b) => (a ? 1 : 0).CompareTo(b ? 1 : 0);
        }

        public static explicit operator UncertainBool(bool value) => new UncertainBool(value);

        public UncertainBool() : base(false, true)
        {
        }

        public UncertainBool(bool v) : base(v)
        {
        }

        public UncertainBool(bool i, bool s) : base(i, s)
        {
        }

        public static UncertainBool indeterminate => new UncertainBool();

        public static UncertainBool operator ==(UncertainBool a, UncertainBool b)
        {
            if (Compare(a.sup(), b.inf()) == -1 || Compare(b.sup(), a.inf()) == -1)
                return (UncertainBool)false;
            if (a.is_certain() && b.is_certain()) // test above implies get_certain(a) == get_certain(b)
                return (UncertainBool)true;
            return indeterminate;
        }

        public static UncertainBool operator !=(UncertainBool a, UncertainBool b) => Mathex.Not(a == b);

        public static UncertainBool operator ==(UncertainBool a, bool b)
        {
            if (Compare(a.sup(), b) == -1 || Compare(b, a.inf()) == -1)
                return (UncertainBool) false;
            if (a.is_certain())
                return (UncertainBool) true;
            return UncertainBool.indeterminate;
        }



        public static UncertainBool operator ==(bool a, UncertainBool b) => b == a;

        public static UncertainBool operator !=(UncertainBool a, bool b) => Mathex.Not(a == b);

        public static UncertainBool operator !=(bool a, UncertainBool b) => Mathex.Not(b == a);

       

        //public static UncertainBool Not(UncertainBool a)
        //{
        //    return new UncertainBool(!a.sup(), !a.inf());
        //}
    }









 
  


    public class UncertainCompareResult : Uncertain<int> {
    
        static UncertainCompareResult()
        {
            Compare = (a, b) => (a).CompareTo(b);
        }

        public static explicit operator UncertainCompareResult(int value) => new UncertainCompareResult(value);

        public UncertainCompareResult() : base(-1, 1)
        {
        }

        public UncertainCompareResult(int v) : base(v)
        {
        }

        public UncertainCompareResult(int i, int s) : base(i, s)
        {
        }

        public static UncertainCompareResult indeterminate => new UncertainCompareResult();

        public static UncertainBool operator ==(UncertainCompareResult a, UncertainCompareResult b)
        {
            if (Compare(a.sup(), b.inf()) == -1 || Compare(b.sup(), a.inf()) == -1)
                return (UncertainBool)false;
            if (a.is_certain() && b.is_certain()) // test above implies get_certain(a) == get_certain(b)
                return (UncertainBool)true;
            return UncertainBool.indeterminate;
        }

        public static UncertainBool operator !=(UncertainCompareResult a, UncertainCompareResult b)
        {
            return Mathex.Not(a == b);
        }

        public static UncertainBool operator ==(UncertainCompareResult a, int b)
        {
            if (a.sup() < b || b < a.inf())
                return (UncertainBool)false;
            if (a.is_certain())
                return (UncertainBool)true;
            return UncertainBool.indeterminate;
        }

        public static UncertainBool operator ==(int a, UncertainCompareResult b) => b == a;

        public static UncertainBool operator !=(UncertainCompareResult a, int b) => Mathex.Not(a == b);

        public static UncertainBool operator !=(int a, UncertainCompareResult b) => Mathex.Not(b == a);

        public static UncertainBool operator <(UncertainCompareResult a, UncertainCompareResult b)
        {
            if (a.sup() < b.inf())
                return (UncertainBool)true;
            if (a.inf() >= b.sup())
                return (UncertainBool)false;
            return UncertainBool.indeterminate;
        }

        public static UncertainBool operator <(UncertainCompareResult a, int b)
        {
            if (a.sup() < b)
                return (UncertainBool)true;
            if (a.inf() >= b)
                return (UncertainBool)false;
            return UncertainBool.indeterminate;
        }

        public static UncertainBool operator <(int a, UncertainCompareResult b)
        {
            if (a < b.inf())
                return (UncertainBool)true;
            if (a >= b.sup())
                return (UncertainBool)false;
            return UncertainBool.indeterminate;
        }

        public static UncertainBool operator >(UncertainCompareResult a, UncertainCompareResult b)=>b < a;
        

        public static UncertainBool operator >(UncertainCompareResult a, int b)=>b < a;
        

        public static UncertainBool operator >(int a, UncertainCompareResult b)=>b < a;
        

        public static UncertainBool operator <=(UncertainCompareResult a, UncertainCompareResult b)=>Mathex.Not(b < a);
        

        public static UncertainBool operator <=(UncertainCompareResult a, int b)=>Mathex.Not(b < a);
        

        public static UncertainBool operator <=(int a, UncertainCompareResult b)=>Mathex.Not(b < a);
        

        public static UncertainBool operator >=(UncertainCompareResult a, UncertainCompareResult b)=>Mathex.Not(a < b);
        

        public static UncertainBool operator >=(UncertainCompareResult a, int b)=>Mathex.Not(a < b);
        

        public static UncertainBool operator >=(int a, UncertainCompareResult b)=>Mathex.Not(a < b);


        public static UncertainCompareResult operator -(UncertainCompareResult u) =>new UncertainCompareResult(-u.sup(), -u.inf());


        // "sign" multiplication.
        // Should be constrained only for "sign" enums, useless for bool.
        public static UncertainCompareResult operator *(UncertainCompareResult a, UncertainCompareResult b)
        {
            if (a.inf() >= 0)                                   // e>=0
            {
                // b>=0     [a.inf()*b.inf(); a.sup()*b.sup()]
                // b<=0     [a.sup()*b.inf(); a.inf()*b.sup()]
                // b~=0     [a.sup()*b.inf(); a.sup()*b.sup()]
                int aa = a.inf(), bb = a.sup();
                if (b.inf() < 0)
                {
                    aa = bb;
                    if (b.sup() < 0)
                        bb = a.inf();
                }
                return new UncertainCompareResult(aa * b.inf(), bb * b.sup());
            }
            else if (a.sup() <= 0)                                // e<=0
            {
                // b>=0     [a.inf()*b.sup(); a.sup()*b.inf()]
                // b<=0     [a.sup()*b.sup(); a.inf()*b.inf()]
                // b~=0     [a.inf()*b.sup(); a.inf()*b.inf()]
                int aa = a.sup(), bb = a.inf();
                if (b.inf() < 0)
                {
                    aa = bb;
                    if (b.sup() < 0)
                        bb = a.sup();
                }
                return new UncertainCompareResult(bb * b.sup(), aa * b.inf());
            }
            else                                          // 0 \in [inf();sup()]
            {
                if (b.inf() >= 0)                           // d>=0
                    return new UncertainCompareResult(a.inf() * b.sup(), a.sup() * b.sup());
                if (b.sup() <= 0)                           // d<=0
                    return new UncertainCompareResult(a.sup() * b.inf(), a.inf() * b.inf());
                // 0 \in d
                int tmp1 = a.inf() * b.sup();
                int tmp2 = a.sup() * b.inf();
                int tmp3 = a.inf() * b.inf();
                int tmp4 = a.sup() * b.sup();
                return new UncertainCompareResult( Math.Min(tmp1, tmp2), Math.Max(tmp3, tmp4));
            }
        }


        public static UncertainCompareResult operator *(int a, UncertainCompareResult b)=>new UncertainCompareResult(a) * b;
        

        public static UncertainCompareResult operator *(UncertainCompareResult a, int b) =>a * new UncertainCompareResult(b);


        
    }




    public interface UncertainCompareResult<E> : IUncertain<E>
    {

    }


    public static partial class Mathex
    {
        public static bool is_indeterminate<T>(T a)
        {
            return false;
        }

        public static bool is_indeterminate<T>(Uncertain<T> a)
        {
            return !a.is_certain();
        }

        public static bool certainly(bool b)
        { return b; }

        public static bool possibly(bool b)
        { return b; }

        public static bool certainly_not(bool b)
        { return !b; }

        public static bool possibly_not(bool b)
        { return !b; }

        public static bool is_certain<T>(T v)
        {
            return true;
        }

        public static bool is_certain(UncertainBool? v)
        {
            return v?.is_certain()??false;
        }
     
        public static bool is_certain(UncertainCompareResult? v)
        {
            
            return v?.is_certain()??false;
        }


        public static T get_certain<T>(T t)
        {
            return t;
        }

        public static bool is_certain<T>(Uncertain<T> a)
        {
            return a.is_certain();
        }

        public static T get_certain<T>(Uncertain<T> a)
        {
            CGAL_assertion(is_certain(a));
            return a.inf();
        }

        public static bool certainly(Uncertain<bool> c)
        {
            return is_certain(c) && get_certain(c);
        }

        public static bool possibly(Uncertain<bool> c)
        {
            return !is_certain(c) || get_certain(c);
        }

        public static bool certainly_not(Uncertain<bool> c)
        {
            return is_certain(c) && !get_certain(c);
        }

        public static bool possibly_not(Uncertain<bool> c)
        {
            return !is_certain(c) || !get_certain(c);
        }

        // Boolean operations for Uncertain<bool>
        // --------------------------------------

        public static UncertainBool Not(UncertainBool a)
        {
            return new UncertainBool(!a.sup(), !a.inf());
        }

        public static bool Not(bool a)
        {
            return !a;
        }

        public static UncertainBool make_uncertain( bool value) => new UncertainBool(value);
        public static UncertainCompareResult make_uncertain( int value) => new UncertainCompareResult(value);

    }

    public static class UncertainExtensions
    {
        public static UncertainBool make_uncertain(this bool value) => new UncertainBool(value);
        public static UncertainCompareResult make_uncertain(this int value) => new UncertainCompareResult(value);

        public static bool Or(this bool a, bool b)
        {
            return a || b;
        }

        public static bool And(this bool a, bool b)
        {
            return a || b;
        }

        public static UncertainBool Or(this UncertainBool a, UncertainBool b)=>new UncertainBool(a.inf() | b.inf(), a.sup() | b.sup());
        

        public static UncertainBool Or(this bool a, UncertainBool b)=>new UncertainBool(a | b.inf(), a | b.sup());
        

        public static UncertainBool Or(this UncertainBool a, bool b) =>new UncertainBool(a.inf() | b, a.sup() | b);
        

        public static UncertainBool And(this UncertainBool a, UncertainBool b)=>new UncertainBool(a.inf() & b.inf(), a.sup() & b.sup());
        

        public static UncertainBool And(this bool a, UncertainBool b)=>new UncertainBool(a & b.inf(), a & b.sup());
        

        public static UncertainBool And(this UncertainBool a, bool b)=>new UncertainBool(a.inf() & b, a.sup() & b);
        
    }

    /*

// certainly/possibly
// ------------------

// Equality operators

template < typename T >
inline
Uncertain<T> make_uncertain(Uncertain<T> t)
{
  return t;
}

// operator&& and operator|| are not provided because, unless their bool counterpart,
// they lack the "short-circuiting" property.
// We provide macros CGAL_AND and CGAL_OR, which attempt to emulate their behavior.
// Key things : do not evaluate expressions twice, and evaluate the right hand side
// expression only when needed. This is done by using the second value of the macro
// only when required thanks to a lambda that will consume the second expression on demand.

namespace internal{
  template <class F_B>
  inline Uncertain<bool> cgal_and_impl(const Uncertain<bool>& a, F_B&& f_b)
  {
    return certainly_not(a) ? make_uncertain(false)
                            : a & make_uncertain(f_b());
  }

  template <class F_B>
  inline Uncertain<bool> cgal_or_impl(const Uncertain<bool>& a, F_B&& f_b)
  {
    return certainly(a) ? make_uncertain(true)
                        : a | make_uncertain(f_b());
  }
}

#  define CGAL_AND(X, Y) \
  ::CGAL::internal::cgal_and_impl((X), [&]() { return (Y); })

#  define CGAL_OR(X, Y) \
  ::CGAL::internal::cgal_or_impl((X), [&]() { return (Y); })

#define CGAL_AND_3(X, Y, Z)  CGAL_AND(X, CGAL_AND(Y, Z))
#define CGAL_AND_6(A, B, C, D, E, F)  CGAL_AND(CGAL_AND_3(A, B, C), CGAL_AND_3(D, E,F))
#define CGAL_OR_3(X, Y, Z)   CGAL_OR(X, CGAL_OR(Y, Z))

// make_certain() : Forcing a cast to certain (possibly throwing).
// This is meant to be used only in cases where we cannot easily propagate the
// uncertainty, such as when used in a switch statement (code may later be
// revisited to do things in a better way).

template < typename T >
inline
T make_certain(T t)
{
  return t;
}

template < typename T >
inline
T make_certain(Uncertain<T> t)
{
  return t.make_certain();
}

// opposite
template < typename T > // should be constrained only for enums.
inline


// enum_cast overload

template < typename T, typename U >
inline
Uncertain<T> enum_cast(Uncertain<U> u)
{
  return Uncertain<T>(static_cast<T>(u.inf()), static_cast<T>(u.sup()));
}
} //namespace CGAL

*/
}