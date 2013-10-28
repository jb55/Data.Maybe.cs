using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable CheckNamespace
namespace Data.Maybe
{
	/// <summary>
	/// The option type; explicitly represents nothing-or-thing nature of a value. 
	/// Supports some of the LINQ operators, such as SelectMany, Where and can be used 
	/// with linq syntax: 
	/// </summary>
	/// <example>
	/// // gets sum of the first and last elements, if they are present, orelse «-5»; 
	/// 
	/// Maybe&lt;int&gt; maybeA = list.FirstMaybe();
	/// Maybe&lt;int&gt; maybeB = list.LastMaybe();
	/// int result = (
	///		from a in maybeA
	///		from b in maybeB
	///		select a + b
	/// ).OrElse(-5);
	/// 
	/// // or shorter:
	/// var result = (from a in list.FirstMaybe() from b in list.LastMaybe() select a + b).OrElse(-5);
	/// </example>
	/// <typeparam name="T"></typeparam>
	public struct Maybe<T> 
	{
		/// <summary>
		/// Nothing value.
		/// </summary>
		public static readonly Maybe<T> Nothing = new Maybe<T>();

		/// <summary>
		/// The value, stored in the monad. Can be accessed only if is really present, otherwise throws
		/// </summary>
		/// <exception cref="InvalidOperationException"> is thrown if not value is present</exception>
		public T Value
		{
			get
			{
				if (!HasValue) throw new InvalidOperationException("value is not present");
				return _value;
			}
		}
		/// <summary>
		/// The flag of value presence
		/// </summary>
		public bool HasValue { get { return _hasValue; } }

		/// <inheritdoc />
		public override string ToString()
		{
			if (!HasValue)
			{
				return "<Nothing>";
			}

			return Value.ToString();
		}

		/// <summary>
		/// Automatical flattening of the monad-in-monad
		/// </summary>
		/// <param name="doubleMaybe"></param>
		/// <returns></returns>
		public static implicit operator Maybe<T>(Maybe<Maybe<T>> doubleMaybe)
		{
			return doubleMaybe.HasValue ? doubleMaybe.Value : Nothing;
		}

		internal Maybe(T value)
		{
			_value = value;
			_hasValue = true;
		}

		private readonly T _value;
		private readonly bool _hasValue;
	}

	/// <summary>
	/// fluent syntax extensions
	/// </summary>
	public static class Maybe 
	{
		#region Returns
		/// <summary>
		/// Returns <paramref name="a"/>.Value.ToString() or <paramref name="@default"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="default"></param>
		/// <returns></returns>
		public static string ReturnToString<T>(this Maybe<T> a, string @default)
		{
			return a.HasValue ? a.Value.ToString() : @default;
		}
		/// <summary>
		/// Returns <paramref name="a"/>.Value or throws <paramref name="e"/>()
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		public static T OrElse<T>(this Maybe<T> a, Func<Exception> e)
		{
			if (a.IsNothing())
			{
				throw e();
			}
			return a.Value;
		}
		/// <summary>
		/// Returns <paramref name="a"/>.Value or returns <paramref name="default"/>()
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="default"></param>
		/// <returns></returns>
		public static T OrElse<T>(this Maybe<T> a, Func<T> @default)
		{
			return a.HasValue ? a.Value : @default();
		}
		/// <summary>
		/// Returns <paramref name="a"/>.Value or returns default(<typeparamref name="T"/>)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static T OrElseDefault<T>(this Maybe<T> a)
		{
			return a.HasValue ? a.Value : default(T);
		}
		/// <summary>
		/// Returns <paramref name="a"/>.Value or returns <paramref name="default"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="default"></param>
		/// <returns></returns>
		public static T OrElse<T>(this Maybe<T> a, T @default)
		{
			return a.HasValue ? a.Value : @default;
		} 
		#endregion

		#region Convertions
		/// <summary>
		/// If <paramref name="a"/>.Value is present, returns a list of that single value, otherwise an empty list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static IEnumerable<T> ReturnList<T>(this Maybe<T> a)
		{
			if (a.IsSomething())
				yield return a.Value;
		}
		/// <summary>
		/// If <paramref name="a"/>.Value exists and can be successfully casted to <typeparamref name="TB"/>, returns the casted one, wrapped as Maybe&lt;TB&gt;, otherwise Nothing
		/// </summary>
		/// <typeparam name="TA"></typeparam>
		/// <typeparam name="TB"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static Maybe<TB> Cast<TA, TB>(this Maybe<TA> a) where TB : class
		{
			return from m in a
				   let t = m as TB
				   where t != null
				   select t;
		}
		/// <summary>
		/// If <paramref name="a"/> can be successfully casted to <typeparamref name="TR"/>, returns the casted one, wrapped as Maybe&lt;TR&gt;, otherwise Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TR"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static Maybe<TR> MaybeCast<T, TR>(this T a) where TR : T
		{
			return Catcher<T, TR, InvalidCastException>(o => (TR)o)(a);
		}
		/// <summary>
		/// Converts Maybe to corresponding Nullable
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static T? ToNullable<T>(this Maybe<T> a) where T : struct
		{
			return a.IsSomething() ? a.Value : new T?();
		}
		/// <summary>
		/// Converts Nullable to corresponding Maybe
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static Maybe<T> ToMaybe<T>(this T? a) where T : struct
		{
			if (!a.HasValue)
				return Maybe<T>.Nothing;
			return a.Value.ToMaybe();
		}
		/// <summary>
		/// Returns <paramref name="a"/> wrapped as Maybe
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static Maybe<T> ToMaybe<T>(this T a)
		{
			// ReSharper disable CompareNonConstrainedGenericWithNull
			if (a == null)
				return Maybe<T>.Nothing;
			return new Maybe<T>(a);
			// ReSharper restore CompareNonConstrainedGenericWithNull
		} 
		#endregion

		#region Compositions
		/// <summary>
		/// Returns <paramref name="a"/> if its Value exists or returns <paramref name="b"/>, wrapped as Maybe
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Maybe<T> Or<T>(this Maybe<T> a, T b)
		{
			if (a.IsSomething())
				return a;
			return b.ToMaybe();
		}
		/// <summary>
		/// Returns <paramref name="a"/> if its Value exists or returns <paramref name="b"/>()
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Maybe<T> Or<T>(this Maybe<T> a, Func<Maybe<T>> b)
		{
			if (a.IsSomething())
				return a;
			return b();
		}
		/// <summary>
		/// Returns <paramref name="a"/> if its Value exists or returns <paramref name="b"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Maybe<T> Or<T>(this Maybe<T> a, Maybe<T> b)
		{
			if (a.IsSomething())
				return a;
			return b;
		}
		/// <summary>
		/// Returns <paramref name="b"/> if <paramref name="a"/> has value, otherwise <see cref="Maybe&lt;T&gt;.Nothing"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Maybe<T2> Compose<T, T2>(this Maybe<T> a, Maybe<T2> b)
		{
			if (a.IsNothing())
				return Maybe<T2>.Nothing;
			return b;
		}
		/// <summary>
		/// Flattens nested maybes to a flat one
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Maybe<T> Collapse<T>(this Maybe<Maybe<T>> t)
		{
			// using implicit cast
			return t;
		}
		#endregion

		#region LINQ
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
			return a.HasValue ? new Maybe<TResult>(fn(a.Value)) : Maybe<TResult>.Nothing;
		}

		/// <summary>
		/// If <paramref name="a"/> has value, applies <paramref name="fn"/> to it and returns the result as Maybe, otherwise returns <paramref name="else"/>.ToMaybe()
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="a"></param>
		/// <param name="fn"></param>
		/// <param name="else"></param>
		/// <returns></returns>
		public static Maybe<TResult> SelectOrElse<T, TResult>(this Maybe<T> a, Func<T, TResult> fn, Func<TResult> @else)
		{
			return (a.HasValue ? fn(a.Value) : @else()).ToMaybe();
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
		#endregion

		#region Maybe+IEnumerable=Love
		/// <summary>
		/// First item or Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <returns></returns>
		public static Maybe<T> FirstMaybe<T>(this IEnumerable<T> items)
		{
			return FirstMaybe(items, arg => true);
		}
		/// <summary>
		/// First item matching <paramref name="predicate"/> or Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static Maybe<T> FirstMaybe<T>(this IEnumerable<T> items, Func<T, bool> predicate)
		{
			return items.FirstOrDefault(predicate).ToMaybe();
		}
		/// <summary>
		/// Single item or Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <returns></returns>
		public static Maybe<T> SingleMaybe<T>(this IEnumerable<T> items)
		{
			return SingleMaybe(items, arg => true);
		}
		/// <summary>
		/// Single item matching <paramref name="predicate"/> or Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static Maybe<T> SingleMaybe<T>(this IEnumerable<T> items, Func<T, bool> predicate)
		{
			items = items.ToArray();
			return (items.Count(predicate) == 1)
				? items.Single(predicate).ToMaybe()
				: Maybe<T>.Nothing;
		}
		/// <summary>
		/// Last item or Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <returns></returns>
		public static Maybe<T> LastMaybe<T>(this IEnumerable<T> items)
		{
			return LastMaybe(items, arg => true);
		}
		/// <summary>
		/// Last item matching <paramref name="predicate"/> or Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static Maybe<T> LastMaybe<T>(this IEnumerable<T> items, Func<T, bool> predicate)
		{
			return items.LastOrDefault(predicate).ToMaybe();
		}
		/// <summary>
		/// Returns the value of <paramref name="maybeCollection"/> if exists orlse an empty collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="maybeCollection"></param>
		/// <returns></returns>
		public static IEnumerable<T> FromMaybe<T>(this Maybe<IEnumerable<T>> maybeCollection)
		{
			return maybeCollection.HasValue ? maybeCollection.Value : Enumerable.Empty<T>();
		}
		/// <summary>
		/// If <paramref name="xs"/> contains any items, returns first one wrapped as Maybe, elsewhere returns Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="xs"></param>
		/// <returns></returns>
		public static Maybe<T> ToMaybeFromList<T>(this IEnumerable<T> xs)
		{
			return xs.FirstMaybe();
		}
		/// <summary>
		/// For each items that has value, applies <paramref name="selector"/> to it and wraps back as Maybe, for each otherwise remains Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="maybes"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static IEnumerable<Maybe<TResult>> Select<T, TResult>(this IEnumerable<Maybe<T>> maybes, Func<T, TResult> selector)
		{
			return maybes.Select(maybe => maybe.Select(selector));
		}
		/// <summary>
		/// If all the items have value, unwraps all and returns the whole sequence of <typeparamref name="T"/>, wrapping the whole as Maybe, otherwise returns Nothing 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="maybes"></param>
		/// <returns></returns>
		public static Maybe<IEnumerable<T>> WholeSequenceOfValues<T>(this IEnumerable<Maybe<T>> maybes)
		{
			maybes = maybes.ToArray();
			// there has got to be a better way to do this
			if (maybes.AnyNothing())
				return Maybe<IEnumerable<T>>.Nothing;

			return maybes.Select(m => m.Value).ToMaybe();
		}
		/// <summary>
		/// Filters out all the Nothings, unwrapping the rest to just type <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="maybes"></param>
		/// <returns></returns>
		public static IEnumerable<T> WhereValueExist<T>(this IEnumerable<Maybe<T>> maybes)
		{
			return SelectWhereValueExist(maybes, m => m);
		}
		/// <summary>
		/// Filters out all the Nothings, unwrapping the rest to just type <typeparamref name="T"/> and then applies <paramref name="fn"/> to each
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="maybes"></param>
		/// <param name="fn"></param>
		/// <returns></returns>
		public static IEnumerable<TResult> SelectWhereValueExist<T, TResult>(this IEnumerable<Maybe<T>> maybes, Func<T, TResult> fn)
		{
			return from maybe in maybes
				   where maybe.HasValue
				   select fn(maybe.Value);
		}
		/// <summary>
		/// Checks if any item is Nothing 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="maybes"></param>
		/// <returns></returns>
		public static bool AnyNothing<T>(this IEnumerable<Maybe<T>> maybes)
		{
			return maybes.Any(m => !m.HasValue);
		}
		/// <summary>
		/// If ALL calls to <paramref name="pred"/> returned a value, filters out the <paramref name="xs"/> based on that values, otherwise returns Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="xs"></param>
		/// <param name="pred"></param>
		/// <returns></returns>
		public static Maybe<IEnumerable<T>> WhereAll<T>(this IEnumerable<T> xs, Func<T, Maybe<bool>> pred)
		{
			var l = new List<T>();
			foreach (var x in xs)
			{
				var r = pred(x);
				if (!r.HasValue)
					return Maybe<IEnumerable<T>>.Nothing;
				if (r.Value)
					l.Add(x);
			}
			return new Maybe<IEnumerable<T>>(l);
		}
		/// <summary>
		/// Filters out <paramref name="xs"/> based on <paramref name="pred"/> resuls; Nothing considered as False
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="xs"></param>
		/// <param name="pred"></param>
		/// <returns></returns>
		public static IEnumerable<T> Where<T>(this IEnumerable<T> xs, Func<T, Maybe<bool>> pred)
		{
			return from x in xs
				   let b = pred(x)
				   where b.HasValue && b.Value
				   select x;
		}
		#endregion

		#region Maybe+Boolean=Love
		/// <summary>
		/// If <paramref name="condition"/> returns <paramref name="f"/>() as Maybe, otherwise Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="condition"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static Maybe<T> Then<T>(this bool condition, Func<T> f)
		{
			// ReSharper disable CompareNonConstrainedGenericWithNull
			return condition ? f().ToMaybe() : Maybe<T>.Nothing;
			// ReSharper restore CompareNonConstrainedGenericWithNull
		}
		/// <summary>
		/// If <paramref name="condition"/> returns <paramref name="t"/> as Maybe, otherwise Nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="condition"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Maybe<T> Then<T>(this bool condition, T t)
		{
			// ReSharper disable CompareNonConstrainedGenericWithNull
			return condition ? t.ToMaybe() : Maybe<T>.Nothing;
			// ReSharper restore CompareNonConstrainedGenericWithNull
		}
		/// <summary>
		/// Calls <paramref name="fn"/> if <paramref name="m"/> is true.ToMaybe()
		/// </summary>
		/// <param name="m"></param>
		/// <param name="fn"></param>
		public static void DoWhenTrue(this Maybe<bool> m, Action fn)
		{
			if (m.HasValue && m.Value)
				fn();
		} 
		#endregion

		#region Side effects
		/// <summary>
		/// Calls <paramref name="fn"/> if <paramref name="m"/> has value, otherwise does nothing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="m"></param>
		/// <param name="fn"></param>
		/// <returns></returns>
		public static Maybe<T> Do<T>(this Maybe<T> m, Action<T> fn)
		{
			if (m.IsSomething())
				fn(m.Value);
			return m;
		}
		/// <summary>
		/// Calls <paramref name="fn"/> if <paramref name="m"/> has value, otherwise calls <paramref name="else"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="m"></param>
		/// <param name="fn"></param>
		/// <param name="else"></param>
		/// <returns></returns>
		public static Maybe<T> Match<T>(this Maybe<T> m, Action<T> fn, Action @else)
		{
			if (m.IsSomething())
				fn(m.Value);
			else
				@else();
			return m;
		} 
		#endregion

		#region Misc helpers

		/// <summary>
		/// Delegate matching usual form of the TryParse methods, such as int.TryParse
		/// </summary>
		/// <typeparam name="TR"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public delegate bool TryGet<in T, TR>(T key, out TR val);
		/// <summary>
		/// Converts a stardard tryer function (like int.TryParse, Dictionary.TryGetValue etc.) to a function, returning Maybe
		/// </summary>
		/// <typeparam name="TR"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="tryer"></param>
		/// <returns></returns>
		public static Func<string, Maybe<TR>> Wrap<T, TR>(TryGet<T, TR> tryer)
		{
			return s =>
			{
				TR result;
				return tryer(s, out result)
					? result.ToMaybe()
					: Maybe<TR>.Nothing;
			};
		}
		/// <summary>
		/// Returns a function which calls <paramref name="f"/>, wrapped inside a try-catch clause with <typeparamref name="TEx"/> catched. 
		/// That new function returns Nothing in the case of the <typeparamref name="TEx"/> thrown inside <paramref name="f"/>, otherwise it returns the f-result as Maybe
		/// </summary>
		/// <typeparam name="TA"></typeparam>
		/// <typeparam name="TR"></typeparam>
		/// <typeparam name="TEx"></typeparam>
		/// <param name="f"></param>
		/// <returns></returns>
		public static Func<TA, Maybe<TR>> Catcher<TA, TR, TEx>(Func<TA, TR> f) where TEx : Exception
		{
			return a =>
			{
				try
				{
					return f(a).ToMaybe();
				}
				catch (TEx)
				{
					return Maybe<TR>.Nothing;
				}
			};
		} 
		#endregion

		/// <summary>
		/// Has a value inside
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool IsSomething<T>(this Maybe<T> a)
		{
			return a.HasValue;
		}
		/// <summary>
		/// Has no value inside
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <returns></returns>
		public static bool IsNothing<T>(this Maybe<T> a)
		{
			return !a.IsSomething();
		}

		/// <summary>
		/// Создает "ничто" такого же типа
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_"></param>
		/// <returns></returns>
		public static Maybe<T> NothingOf<T>(this Maybe<T> _)
		{
			return Maybe<T>.Nothing;
		}
		/// <summary>
		/// Создает "ничто" такого же типа
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_"></param>
		/// <returns></returns>
		public static Maybe<T> NothingOf<T>(this T _)
		{
			return Maybe<T>.Nothing;
		}
	}
}