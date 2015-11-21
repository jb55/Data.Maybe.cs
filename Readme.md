
# Functional.Maybe

Option types for C# with LINQ support and rich fluent syntax for many popular uses

## Examples

All these examples require that you have the following using statement:

```cs
using Functional.Maybe
```

### Computing on maybe types

```cs
Maybe<string> maybeGood = "hello".ToMaybe();
Maybe<string> maybeJunk = Maybe<string>.Nothing;

var concat = from good in maybeGood
             from junk in maybeJunk
             select good + junk;

if (concat.IsNothing())
  Console.WriteLine("One of the strings was bad, could not concat");
```

LINQ will terminate the computation if there is a Nothing at any point in the
computation.

### Running a computation with a maybe type:

```cs
string nullString = null;

nullString.ToMaybe().Do(str => {
  // str will never be null, ToMaybe guards against null and Do unwraps the value
});
```

### Guarding

You can check a condition on a maybe type and guard against them:

```cs
string name = "Bill Casarin";
Maybe<string> maybeName = from n in name.ToMaybe()
                          where n.StartsWith("Bill")
                          select n;
```

If the name didn't start with Bill, `maybeName` would be `Maybe<string>.Nothing`

### Maybe coalescing

Maybe has an operator similar to the null coalescing operator `??`. We achieve
optional short-circuit evaluation with lambdas:

```cs
Maybe<string> name1 = Maybe<string>.Nothing;
Maybe<string> name2 = "Some Name".ToMaybe();

Maybe<string> goodNameLazy = name1.Or(() => name2);
// this works too:
Maybe<string> goodName = name1.Or(name2);
// and this:
Maybe<string> goodName = name1.Or("goodName");
```

You can also convert value-kinded maybe types to Nullable<T>s:

```cs
Maybe<int> maybeNumber = Maybe<int>.Nothing;
Maybe<int> maybeAnotherNumber = (4).ToMaybe();

int? ok = maybeNumber.ToNullable() ?? maybeAnotherNumber.ToNullable();
```

### Extracting values

Sometime you want to pull out a value with a default value in case of `Nothing`:

```cs
Maybe<string> possibleString = Maybe<string>.Nothing;
string goodString = possibleString.OrElse("default");
```

The default parameter can also be lazy:

```cs
string goodString = possibleString.OrElse(() => doHeavyComputationForString());
```

Or you can throw an exception instead:

```cs
string val = null;
try {
  val = (Maybe<string>.Nothing).OrElse(() => new Exception("no value"));
} catch (Exception) {
  // exception will be thrown
}
```

Or, finally, you can just get the default value for that type:

```cs
string val = maybeString.OrElseDefault();
```

### Why not use Nullable<T> instead?

Nullable<T> only works on value types. Maybe<T> works on both value and
reference types. It also has LINQ support. 

## More interesting examples

### Getting the first element of a list

```cs
public static Maybe<T> Head<T>(this IEnumerable<T> xs) {
  foreach(var x in xs)
    return x.ToMaybe();
  return Maybe<T>.Nothing;
}
```

Now lets get a bunch of heads!

```cs
var result = from h1 in list1.Head()
             from h2 in list2.Head()
             from h3 in list3.Head()
             select ConsumeHeads(h1, h2, h3);
```

ConsumeHeads will never run unless all Head() calls return valid results.

### Lookups

Here's a function for getting a value out of a dictionary:

```cs
public static Maybe<T2> Lookup<T, T2>(this IDictionary<T, T2> d, T key) {
  var getter = MaybeFunctionalWrappers.Wrap(d.TryGetValue);
  return getter(key);
}
```

### Parsing

```cs

public static Maybe<int> ParseInt(string s) {
  var parser = MaybeFunctionalWrappers.Wrap(int.TryParse);
  return parser(s);
}
```

### Lookup + Parsing!

```cs
var parsedFromDict = from val in d.Lookup("key")
                     from parsedVal in ParseInt(val)
                     select parsedVal;
```

