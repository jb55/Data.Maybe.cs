using System;

namespace Functional.Maybe
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
}