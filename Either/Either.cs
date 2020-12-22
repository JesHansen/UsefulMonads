using System;

namespace UsefulMonads
{

  /// <summary>
  /// A class that represents two disjoint cases.
  /// </summary>
  /// <typeparam name="TError">The type of the first (or 'left') case. Often used to represent an error.</typeparam>
  /// <typeparam name="TOk">The type of the second (or 'right' (pun intended)) case. Often used to represent the value of a computation.</typeparam>
  public class Either<TError, TOk> : IEquatable<Either<TError, TOk>>
  {
    private readonly bool isError;

    /// <summary>
    /// Initializes a new instance of the <see cref="Either{TError,TOk}" /> class with a value of type <typeparamref name="TOk"/>.
    /// </summary>
    /// <param name="ok">The value to be wrapped. Must be non-null.</param>
    public Either(TOk ok)
    {
      if (ok == null)
      {
        throw new ArgumentNullException(nameof(ok));
      }

      Ok = new Lazy<TOk>(() => ok);
      Failure = new Lazy<TError>(() => throw new InvalidOperationException("Cannot access the failure value."));
      isError = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Either{TError,TOk}" /> class with a value of type <typeparamref name="TError"/>.
    /// </summary>
    /// <param name="failure">The value to be wrapped. Must be non-null.</param>
    public Either(TError failure)
    {
      if (failure == null)
      {
        throw new ArgumentNullException(nameof(failure));
      }

      Ok = new Lazy<TOk>(() => throw new InvalidOperationException("Cannot access the Ok value."));
      Failure = new Lazy<TError>(() => failure);
      isError = true;
    }

    private Lazy<TError> Failure { get; }

    private Lazy<TOk> Ok { get; }

    /// <summary>
    /// Resolve the two cases. To access the wrapped values, you must supply two callback functions, one for each case.
    /// If the wrapped value is of type <typeparamref name="TError"/>, <paramref name="onError"/> will be called, otherwise
    /// <paramref name="onSuccess"/> is called.
    /// </summary>
    /// <typeparam name="TFinal">The type of the value that <paramref name="onSuccess"/> maps the value to.</typeparam>
    /// <param name="onError">The function callback to be invoked if the wrapped value is of type <typeparamref name="TError"/>.</param>
    /// <param name="onSuccess">The function callback to be invoked if the wrapped value is of type <typeparamref name="TOk"/>.</param>
    /// <returns>The value produced by calling <paramref name="onSuccess"/> with the wrapped <typeparamref name="TOk"/> value,
    /// or by calling <paramref name="onError"/> with the wrapped <typeparamref name="TError"/> value.</returns>
    public TFinal Resolve<TFinal>(Func<TError, TFinal> onError, Func<TOk, TFinal> onSuccess)
    {
      if (onError == null)
      {
        throw new ArgumentNullException(nameof(onError));
      }

      if (onSuccess == null)
      {
        throw new ArgumentNullException(nameof(onSuccess));
      }

      return isError ? onError(Failure.Value) : onSuccess(Ok.Value);
    }

    /// <summary>
    /// Resolve the two cases. To access the wrapped values, you must supply two callback functions, one for each case.
    /// If the wrapped value is of type <typeparamref name="TError"/>, <paramref name="onError"/> will be called, otherwise
    /// <paramref name="onSuccess"/> is called.
    /// </summary>
    /// <param name="onError">The action callback to be invoked if the wrapped value is of type <typeparamref name="TError"/>.</param>
    /// <param name="onSuccess">The action callback to be invoked if the wrapped value is of type <typeparamref name="TOk"/>.</param>
    public void Resolve(Action<TError> onError, Action<TOk> onSuccess)
    {
      if (onError == null)
      {
        throw new ArgumentNullException(nameof(onError));
      }

      if (onSuccess == null)
      {
        throw new ArgumentNullException(nameof(onSuccess));
      }

      if (isError)
      {
        onError(Failure.Value);
      }
      else
      {
        onSuccess(Ok.Value);
      }
    }

    /// <summary>
    /// Transform a wrapped <typeparamref name="TOk"/> value to another value.
    /// </summary>
    /// <typeparam name="TMapped">The type of the new value.</typeparam>
    /// <param name="mapSucceeded">If the wrapped value has type <typeparamref name="TOk"/>, this function will be invoked,
    /// and the result will be wrapped in a new <see cref="Either{TError,TOk}"/>. Must not be null.</param>
    /// <returns>A new wrapped value, mapped by <paramref name="mapSucceeded"/>, if the currently wrapped value
    /// is of type <typeparamref name="TOk"/>. Otherwise a new <see cref="Either{TError,TOk}"/> with the existing
    /// <typeparamref name="TError"/> value is returned.</returns>
    public Either<TError, TMapped> MapOk<TMapped>(Func<TOk, TMapped> mapSucceeded)
    {
      if (mapSucceeded == null)
      {
        throw new ArgumentNullException(nameof(mapSucceeded));
      }

      return isError ?
        new Either<TError, TMapped>(Failure.Value) :
        new Either<TError, TMapped>(mapSucceeded(Ok.Value));
    }

    /// <summary>
    /// Transform a wrapped <typeparamref name="TError"/> value to another value.
    /// </summary>
    /// <typeparam name="TNewError">The type of the new error.</typeparam>
    /// <param name="mapFailed">The function used to map the existing <typeparamref name="TError"/> value to the new value.
    /// Must not be null.</param>
    /// <returns>A new <see cref="Either{TError,TOk}"/> with a mapped error.</returns>
    public Either<TNewError, TOk> MapError<TNewError>(Func<TError, TNewError> mapFailed)
    {
      if (mapFailed == null)
      {
        throw new ArgumentNullException(nameof(mapFailed));
      }

      return isError ?
        new Either<TNewError, TOk>(mapFailed(Failure.Value)) :
        new Either<TNewError, TOk>(Ok.Value);
    }

    /// <summary>
    /// Transform the inner <typeparamref name="TOk"/> value to a new <see cref="Either{TError,TOk}"/>.
    /// </summary>
    /// <typeparam name="TMapped">The return type of the new inner transformed value.</typeparam>
    /// <param name="mapSucceeded">This callback is invoked if the wrapped value is a <typeparamref name="TOk"/> value. Must not be null.</param>
    /// <returns>A new <see cref="Either{TError,TOk}"/> containing the transformed value.</returns>
    public Either<TError, TMapped> Bind<TMapped>(Func<TOk, Either<TError, TMapped>> mapSucceeded)
    {
      if (mapSucceeded == null)
      {
        throw new ArgumentNullException(nameof(mapSucceeded));
      }

      return isError ?
        new Either<TError, TMapped>(Failure.Value) :
        mapSucceeded(Ok.Value);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Either{TError,TOk}"/> is equal to the current <see cref="Either{TError,TOk}"/>.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public bool Equals(Either<TError, TOk>? other)
    {
      if (other is null)
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      if (isError != other.isError)
      {
        return false;
      }

      return isError
                 ? Failure.Value!.Equals(other.Failure.Value)
                 : Ok.Value!.Equals(other.Ok.Value);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Either{TError,TOk}"/> object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
      if (obj is null)
      {
        return false;
      }

      if (ReferenceEquals(this, obj))
      {
        return true;
      }

      return obj.GetType() == GetType() && Equals((Either<TError, TOk>)obj);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A hash code for the current <see cref="Either{TError,TOk}"/>.</returns>
    public override int GetHashCode()
    {
      return isError ? HashCode.Combine(isError, Failure.Value) : HashCode.Combine(isError, Ok.Value);
    }

    /// <summary>
    /// Equality operator overload. Delegates to <see cref="Equals(Either{TError,TOk})"/>.
    /// </summary>
    /// <param name="left">The first <see cref="Either{TError,TOk}"/> instance.</param>
    /// <param name="right">The second <see cref="Either{TError,TOk}"/> instance.</param>
    /// <returns>true if the two <see cref="Either{TError,TOk}"/> instances are equal; otherwise, false.</returns>
    public static bool operator ==(Either<TError, TOk> left, Either<TError, TOk> right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Not Equals operator overload. Delegates to <see cref="Equals(Either{TError,TOk})"/>.
    /// </summary>
    /// <param name="left">The first <see cref="Either{TError,TOk}"/> instance.</param>
    /// <param name="right">The second <see cref="Either{TError,TOk}"/> instance.</param>
    /// <returns>true if the two <see cref="Either{TError,TOk}"/> instances are not equal; false if they are equal.</returns>
    public static bool operator !=(Either<TError, TOk> left, Either<TError, TOk> right)
    {
      return !Equals(left, right);
    }
  }
}
