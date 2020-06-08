using System;


namespace JesHansen.Monads
{
  /// <summary>
  /// Represents a value that may or may not be available
  /// </summary>
  /// <typeparam name="T">The type of the value that may be available</typeparam>
  public struct Maybe<T> : IEquatable<Maybe<T>>
  {
    // There is only one empty Maybe (per type), so reuse it.
    private static readonly Lazy<Maybe<T>> EmptyInstance = new Lazy<Maybe<T>>(() => new Maybe<T>());

    /// <summary>
    /// TODO
    /// </summary>
    public static Maybe<T> Empty => EmptyInstance.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Maybe{T}"/> struct. If the value provided is null the maybe will be empty.
    /// </summary>
    /// <param name="input">The input value to wrap.</param>
    public Maybe(T input)
    {
      if (input == null)
      {
        HasValue = false;
        Value = default;
      }
      else
      {
        Value = input;
        HasValue = true;
      }
    }

    private T Value { get; }

    private bool HasValue { get; }

    /// <summary>
    /// Resolve the Maybe value to a value.
    /// </summary>
    /// <typeparam name="R">The return value type.</typeparam>
    /// <param name="fallbackValue">A fallback value that will be returned in lieu of a wrapped
    /// <typeparamref name="T"/> value that can be mapped by <paramref name="onValuePresent"/>.</param>
    /// <param name="onValuePresent">A function to map the contained <typeparamref name="T"/> value.</param>
    /// <returns>The value returned by applying <paramref name="onValuePresent"/> to the wrapped <typeparamref name="T"/> value.</returns>
    public R Resolve<R>(R fallbackValue, Func<T, R> onValuePresent)
    {
      if (onValuePresent == null)
      {
        throw new ArgumentNullException(nameof(onValuePresent));
      }

      return !HasValue ? fallbackValue : onValuePresent(Value);
    }

    /// <summary>
    /// Do something with the <see cref="Maybe{T}"/>.
    /// </summary>
    /// <param name="onValueMissing">An action to perform if the <see cref="Maybe{T}"/> is empty.</param>
    /// <param name="onValuePresent">An action to perform if the <see cref="Maybe{T}"/> contains a value of type <typeparamref name="T"/>.</param>
    public void Do(Action onValueMissing, Action<T> onValuePresent)
    {
      if (onValueMissing == null)
      {
        throw new ArgumentNullException(nameof(onValueMissing));
      }

      if (onValuePresent == null)
      {
        throw new ArgumentNullException(nameof(onValuePresent));
      }

      if (HasValue)
      {
        onValuePresent(Value);
      }
      else
      {
        onValueMissing();
      }
    }

    /// <summary>
    /// Map the inner value, if present.
    /// </summary>
    /// <typeparam name="R">The type of the new wrapped value.</typeparam>
    /// <param name="selector">A function that maps the wrapped <typeparamref name="T"/> value to an <typeparamref name="R"/> value.</param>
    /// <returns>A new <see cref="Maybe{T}"/>containing the new mapped value, or an empty <see cref="Maybe{T}"/> if this instance is empty.</returns>
    public Maybe<R> Map<R>(Func<T, R> selector)
    {
      if (selector == null)
      {
        throw new ArgumentNullException(nameof(selector));
      }

      return !HasValue ? new Maybe<R>() : new Maybe<R>(selector(Value));
    }

    /// <summary>
    /// LINQ query selector.
    /// </summary>
    /// <typeparam name="R">The type of the new wrapped value.</typeparam>
    /// <param name="selector">A function that maps the wrapped <typeparamref name="T"/> value to an <typeparamref name="R"/> value.</param>
    /// <returns>A new <see cref="Maybe{T}"/>containing the new mapped value, or an empty <see cref="Maybe{T}"/> if this instance is empty.</returns>
    public Maybe<R> Select<R>(Func<T, R> selector) => Map(selector);

    /// <summary>
    /// Monadic Bind. Allows LINQ query syntax via SelectMany.
    /// </summary>
    /// <typeparam name="R">The type of the new wrapped value.</typeparam>
    /// <param name="f">A function that maps the wrapped <typeparamref name="T"/> value to a new <see cref="Maybe{T}"/> of type <typeparamref name="R"/>.</param>
    /// <returns>The <see cref="Maybe{T}"/> returned by <paramref name="f"/>, or an empty <see cref="Maybe{T}"/> if this instance is empty.</returns>
    public Maybe<R> Bind<R>(Func<T, Maybe<R>> f)
    {
      if (f == null)
      {
        throw new ArgumentNullException(nameof(f));
      }

      return !HasValue ? new Maybe<R>() : f(Value);
    }

    /// <summary>
    /// LINQ query syntax.
    /// </summary>
    /// <typeparam name="R">The type of the intermediary value.</typeparam>
    /// <typeparam name="F">The type wrapped by the final returned <see cref="Maybe{T}"/>.</typeparam>
    /// <param name="maybeSelector">A function that maps the wrapped <typeparamref name="T"/> value to a new <see cref="Maybe{T}"/> of type <typeparamref name="R"/>.</param>
    /// <param name="resultSelector">A function that maps the wrapped <typeparamref name="T"/> value and the value from the
    /// intermediary value to a new <see cref="Maybe{T}"/> of type <typeparamref name="F"/>.</param>
    /// <returns>A new <see cref="Maybe{T}"/>containing the new mapped value.</returns>
    public Maybe<F> SelectMany<R, F>(Func<T, Maybe<R>> maybeSelector, Func<T, R, F> resultSelector)
    {
      if (maybeSelector == null)
      {
        throw new ArgumentNullException(nameof(maybeSelector));
      }

      if (resultSelector == null)
      {
        throw new ArgumentNullException(nameof(resultSelector));
      }

      // Lambdas inside structs do not have access to this.variableName,
      // so we make a reference to it here instead.
      var value = Value;
      return !HasValue
                 ? new Maybe<F>()
                 : maybeSelector(value).Bind(r => new Maybe<F>(resultSelector(value, r)));
    }


    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      switch (obj)
      {
        case null:
          return !HasValue;
        case Maybe<T> maybe:
          return Equals(maybe);
        case T valueType:
          return Equals(valueType);
        default:
          return false;
      }
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return HasValue ? Value.GetHashCode() : 0;
    }

    /// <inheritdoc />
    public bool Equals(Maybe<T> other)
    {
      if (HasValue && other.HasValue)
      {
        return Value.Equals(other.Value);
      }
      return !HasValue && !other.HasValue;
    }

    /// <summary>
    /// Determines if two <see cref="Maybe{T}"/> instances are equal
    /// </summary>
    /// <param name="lhs">First instance</param>
    /// <param name="rhs">Second instance</param>
    /// <returns><c>true</c> if the two instances are equal</returns>
    public static bool operator ==(Maybe<T> lhs, Maybe<T> rhs) => lhs.Equals(rhs);

    /// <summary>
    /// Determines if two <see cref="Maybe{T}"/> instances are not equal
    /// </summary>
    /// <param name="lhs">First instance</param>
    /// <param name="rhs">Second instance</param>
    /// <returns><c>true</c> if the two instances are not equal</returns>
    public static bool operator !=(Maybe<T> lhs, Maybe<T> rhs) => !lhs.Equals(rhs);

    /// <summary>
    /// Unwrap a nested <see cref="Maybe{T}"/>
    /// </summary>
    /// <param name="doubleWrapped">A nested <see cref="Maybe{T}"/></param>
    /// <returns>The innermost <see cref="Maybe{T}"/></returns>
    public static Maybe<T> Unwrap(Maybe<Maybe<T>> doubleWrapped)
    {
      return !doubleWrapped.HasValue ? Empty : doubleWrapped.Value;
    }

  }

  /// <summary>
  /// TODO
  /// </summary>
  public static class MaybeExtensions
  {
    /// <summary>
    /// Wraps the given value in a Maybe for that type
    /// </summary>
    public static Maybe<T> ToMaybe<T>(this T input) => new Maybe<T>(input);

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="input"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Maybe<T> Clone<T>(this Maybe<T> input) => input.Map(x => x);
  }
}