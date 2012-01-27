using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Maybe
{
    public struct Maybe<T>
    {
        public readonly static Maybe<T> Nothing = new Maybe<T>();
        public T Value;
        public bool HasValue;

        public Maybe(T value) {
            Value = value;
            HasValue = true;
        }

        public override string ToString() {
            if (!HasValue) {
                return "<Nothing>";
            }

            return Value.ToString();
        }
    }

    public static class Maybe
    {
        public static string FromMaybeS<T>(this Maybe<T> a, string @default) {
            return a.HasValue ? a.Value.ToString() : @default;
        }

        public static T FromMaybe<T>(this Maybe<T> a, Func<Exception> e) {
            if (a.IsNothing()) {
                throw e();
            }
            return a.Value;
        }

        public static T FromMaybe<T>(this Maybe<T> a, Func<T> @default) {
            return a.HasValue ? a.Value : @default();
        }

        public static IEnumerable<T> FromMaybeToList<T>(this Maybe<T> a) {
            if (a.IsSomething())
                yield return a.Value;
        }

        public static T FromMaybeOrNull<T>(this Maybe<T> a) where T : class {
            return a.IsNothing() ? null : a.Value;
        }

        public static T FromMaybeOrDefault<T>(this Maybe<T> a) {
            return a.FromMaybe(default(T));
        }

        public static T FromMaybe<T>(this Maybe<T> a, T @default) {
            return a.HasValue ? a.Value : @default;
        }

        public static T2 FromMaybe<T, T2>(this Maybe<T> a, Func<T, T2> fn, Func<T2> @default) {
            return a.HasValue ? fn(a.Value) : @default();
        }

        public static T2 FromMaybe<T, T2>(this Maybe<T> a, Func<T, T2> fn, T2 @default) {
            return a.HasValue ? fn(a.Value) : @default;
        }

        public static Maybe<T> Cast<T>(this object a) {
            try {
                var t = (T)a;
                return t.ToMaybe();
            }
            catch {
                return Maybe<T>.Nothing;
            }
        }

        public static T? ToNullable<T>(this Maybe<T> a) where T : struct {
            if (a.IsSomething())
                return new Nullable<T>(a.Value);
            else
                return new Nullable<T>();
        }

        public static Maybe<T> ToMaybe<T>(this Nullable<T> a) where T : struct {
            if (!a.HasValue)
                return Maybe<T>.Nothing;
            return a.Value.ToMaybe();
        }

        public static Maybe<T> ToMaybe<T>(this T a) {
            if (a == null)
                return Maybe<T>.Nothing;
            return new Maybe<T>(a);
        }

        public static void RunOrThrow<T>(this Maybe<T> m, Action<T> fn, Exception e = null) {
            if (!m.HasValue) {
                throw e ?? new InvalidOperationException("RunOrThrow on Maybe threw the default exception");
            }

            fn(m.Value);
        }

        public static void RunWhenTrue(this Maybe<bool> m, Action fn) {
            if (m.HasValue && m.Value)
                fn();
        }

        public static Maybe<T> Collapse<T>(this Maybe<Maybe<T>> t) {
            if (t.IsNothing() || t.Value.IsNothing())
                return Maybe<T>.Nothing;
            return t.Value;
        }

        public static bool IsSomething<T>(this Maybe<T> a) {
            return a.HasValue;
        }

        public static bool IsNothing<T>(this Maybe<T> a) {
            return !a.IsSomething();
        }

        public static Maybe<T2> Compose<T, T2>(this Maybe<T> a, Maybe<T2> b) {
            if (a.IsNothing())
                return Maybe<T2>.Nothing;
            return b;
        }

        public static Maybe<T> Or<T>(this Maybe<T> a, T b) {
            if (a.IsSomething())
                return a;
            return b.ToMaybe();
        }

        public static Maybe<T> Or<T>(this Maybe<T> a, Func<Maybe<T>> b) {
            if (a.IsSomething())
                return a;
            return b();
        }

        public static Maybe<T> Or<T>(this Maybe<T> a, Maybe<T> b) {
            if (a.IsSomething())
                return a;
            return b;
        }

        public static void Run<T, T2, T3>(this Maybe<T> m, Maybe<T2> m2, Maybe<T3> m3, Action<T, T2, T3> fn) {
            if (m.IsSomething() && m2.IsSomething() && m3.IsSomething())
                fn(m.Value, m2.Value, m3.Value);
        }

        public static void Run<T, T2>(this Maybe<T> m, Maybe<T2> m2, Action<T, T2> fn) {
            if (m.IsSomething() && m2.IsSomething())
                fn(m.Value, m2.Value);
        }

        public static void Run<T>(this Maybe<T> m, Action<T> fn) {
            if (m.IsSomething())
                fn(m.Value);
        }

        public static void Run<T>(this Maybe<T> m, Action<T> fn) {
            if (m.IsSomething())
                fn(m.Value);
        }

        public static Maybe<T> ToMaybeFromList<T>(this IEnumerable<T> xs) {
            foreach (var x in xs) {
                return x.ToMaybe();
            }

            return Maybe<T>.Nothing;
        }

        public static Maybe<TResult> Select<T, TResult>(this Maybe<T> m, Func<T, TResult> fn) {
            return m.HasValue ? new Maybe<TResult>(fn(m.Value)) : Maybe<TResult>.Nothing;
        }

        public static IEnumerable<Maybe<TResult>> Select<T, TResult>(this IEnumerable<Maybe<T>> maybes, Func<T, TResult> selector) {
            return maybes.Select(maybe => maybe.Select(selector));
        }

        public static IEnumerable<T> SelectValid<T>(this IEnumerable<Maybe<T>> maybes) {
            return SelectValid(maybes, m => m);
        }

        public static IEnumerable<TResult> SelectValid<T, TResult>(this IEnumerable<Maybe<T>> maybes, Func<T, TResult> fn) {
            foreach (var maybe in maybes) {
                if (maybe.HasValue)
                    yield return fn(maybe.Value);
            }
        }

        public static Maybe<IEnumerable<T>> WhereMaybe<T>(this IEnumerable<T> xs, Func<T, Maybe<bool>> pred) {
            var l = new List<T>();
            foreach (var x in xs) {
                var r = pred(x);
                if (!r.HasValue)
                    return Maybe<IEnumerable<T>>.Nothing;
                if (r.Value)
                    l.Add(x);
            }
            return new Maybe<IEnumerable<T>>(l);
        }

        public static Maybe<T> Where<T>(this Maybe<T> maybe, Func<T, bool> cond) {
            if (!maybe.HasValue)
                return maybe;

            if (cond(maybe.Value))
                return maybe;

            return Maybe<T>.Nothing;
        }

        public static bool AnyNothing<T>(this IEnumerable<Maybe<T>> maybes) {
            return maybes.Any(m => !m.HasValue);
        }

        public static Maybe<IEnumerable<T>> Sequence<T>(this IEnumerable<Maybe<T>> maybes) {
            // there has got to be a better way to do this
            if (maybes.AnyNothing())
                return Maybe<IEnumerable<T>>.Nothing;

            return maybes.Select(m => m.Value).ToMaybe();
        }

        public static Maybe<U> SelectMany<T, U>(this Maybe<T> m, Func<T, Maybe<U>> k) {
            if (!m.HasValue)
                return Maybe<U>.Nothing;
            return k(m.Value);
        }

        public static Maybe<V> SelectMany<T, U, V>(this Maybe<T> m, Func<T, Maybe<U>> k, Func<T, U, V> s) {
          return m.SelectMany(x => k(x).SelectMany(y => s(x, y).ToMaybe()));
        }

    }
}
