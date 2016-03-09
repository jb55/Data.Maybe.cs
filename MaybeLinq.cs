using System;
using System.Diagnostics.Contracts;

namespace Functional.Maybe
{
	/// <summary>
	/// Providing necessary methods to enable linq syntax with Maybes themselves
	/// </summary>
	public static class MaybeLinq
	{
		/// <summary>
		/// If <paramref name="a"/> has value, applies <paramref name="fn"/> to it and returns the result as Maybe, otherwise returns Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="a"></param>
		/// <param name="fn"></param>
		/// <returns></returns>
		public static Maybe<TResult> Select<T, TResult>(this Maybe<T> a, Func<T, TResult> fn)
		{
			Contract.Requires(fn != null);

			if (a.HasValue)
			{
				var result = fn(a.Value);

				return result != null
					? new Maybe<TResult>(result)
					: Maybe<TResult>.Nothing;
			}
			else
				return Maybe<TResult>.Nothing;
		}
		/// <summary>
		/// If <paramref name="a"/> has value, applies <paramref name="fn"/> to it and returns the result, otherwise returns <paramref name="else"/>()
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="a"></param>
		/// <param name="fn"></param>
		/// <param name="else"></param>
		/// <returns></returns>
		public static TResult SelectOrElse<T, TResult>(this Maybe<T> a, Func<T, TResult> fn, Func<TResult> @else)
		{
			Contract.Requires(fn != null);
			Contract.Requires(@else != null);

			return a.HasValue ? fn(a.Value) : @else();
		}
		/// <summary>
		/// If <paramref name="a"/> has value, and it fulfills the <paramref name="predicate"/>, returns <paramref name="a"/>, otherwise returns Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static Maybe<T> Where<T>(this Maybe<T> a, Func<T, bool> predicate)
		{
			Contract.Requires(predicate != null);

			if (!a.HasValue)
				return a;

			if (predicate(a.Value))
				return a;

			return Maybe<T>.Nothing;
		}
		/// <summary>
		/// If <paramref name="a"/> has value, applies <paramref name="fn"/> to it and returns, otherwise returns Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TR"></typeparam>
		/// <param name="a"></param>
		/// <param name="fn"></param>
		/// <returns></returns>
		public static Maybe<TR> SelectMany<T, TR>(this Maybe<T> a, Func<T, Maybe<TR>> fn)
		{
			Contract.Requires(fn != null);

			if (!a.HasValue)
				return Maybe<TR>.Nothing;
			return fn(a.Value);
		}

		/// <summary>
		/// If <paramref name="a"/> has value, applies <paramref name="fn"/> to it, and if the result also has value, calls <paramref name="composer"/> on both values 
		/// (original and fn-call-resulted), and returns the <paramref name="composer"/>-call result, wrapped in Maybe. Otherwise returns nothing.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TTempResult"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="a"></param>
		/// <param name="fn"></param>
		/// <param name="composer"></param>
		/// <returns></returns>
		public static Maybe<TResult> SelectMany<T, TTempResult, TResult>(this Maybe<T> a, Func<T, Maybe<TTempResult>> fn, Func<T, TTempResult, TResult> composer)
		{
			return a.SelectMany(x => fn(x).SelectMany(y => composer(x, y).ToMaybe()));
		}

		/// <summary>
		/// If <paramref name="a"/> has value, applies <paramref name="fn"/> to it and returns, otherwise returns Nothing. Alias for SelectMany.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TR"></typeparam>
		/// <param name="a"></param>
		/// <param name="fn"></param>
		/// <returns></returns>
		public static Maybe<TR> SelectMaybe<T, TR>(this Maybe<T> a, Func<T, Maybe<TR>> fn)
		{
			return a.SelectMany(fn);
		}

		/// <summary>
		/// If <paramref name="a"/> has value, applies <paramref name="fn"/> to it, and if the result also has value, calls <paramref name="composer"/> on both values 
		/// (original and fn-call-resulted), and returns the <paramref name="composer"/>-call result, wrapped in Maybe. Otherwise returns nothing.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TTempResult"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="a"></param>
		/// <param name="fn"></param>
		/// <param name="composer"></param>
		/// <returns></returns>
		public static Maybe<TResult> SelectMaybe<T, TTempResult, TResult>(this Maybe<T> a, Func<T, Maybe<TTempResult>> fn, Func<T, TTempResult, TResult> composer)
		{
			return a.SelectMany(fn, composer);
		}
	}
}