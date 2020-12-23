using System;


namespace UsefulMonads.Maybe
{
  /// <summary>
  /// Represents a value that may or may not be available
  /// </summary>
  /// <typeparam name="T">The type of the value that may be available</typeparam>
  public class Maybe<T> : IEquatable<Maybe<T>>
  {
    // There is only one empty Maybe (per type), so reuse it.
    private static readonly Lazy<Maybe<T>> EmptyInstance = new(() => new Maybe<T>(default) {hasValue = false});

    /// <summary>
    /// TODO
    /// </summary>
    private static Maybe<T> Empty => EmptyInstance.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Maybe{T}"/> struct. If the value provided is null the maybe will be empty.
    /// </summary>
    /// <param name="input">The input value to wrap.</param>
    private Maybe(T? input)
    {
      if (input is null)
      {
        hasValue = false;
        Value = default;
      }
      else
      {
        hasValue = true;
        Value = input;
      }
    }

    public static Maybe<T> Create(T? item)
    {
      return item == null ? Empty : new Maybe<T>(item);
    }

    public static Maybe<T> Create()
    {
      return Empty;
    }

    private T? Value { get; }
    private bool hasValue = false;

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

      return hasValue ? onValuePresent(Value!) : fallbackValue;
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

      if (hasValue)
      {
        onValuePresent(Value!);
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

      return hasValue ? new Maybe<R>(selector(Value!)) : Maybe<R>.Empty;
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

      return hasValue ? f(Value!) : Maybe<R>.Empty;
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

      return hasValue
            ? maybeSelector(Value!).Bind(r => new Maybe<F>(resultSelector(Value!, r)))
            : Maybe<F>.Empty;
    }


    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
      return obj switch
      {
        null => false,
        Maybe<T> maybe => Equals(maybe),
        _ => false
      };
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      return HashCode.Combine(Value);
    }

    /// <inheritdoc />
    public bool Equals(Maybe<T>? other)
    {
      if (null == other)
        return false;
      if (hasValue && other.hasValue)
        return Value!.Equals(other.Value);
      return hasValue == other.hasValue;
    }

    public static bool operator ==(Maybe<T>? left, Maybe<T>? right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Maybe<T>? left, Maybe<T>? right)
    {
      return !Equals(left, right);
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
    public static Maybe<T> ToMaybe<T>(this T input) => Maybe<T>.Create(input);
  }
}