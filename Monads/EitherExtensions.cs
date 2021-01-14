using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UsefulMonads.Either
{
  /// <summary>
  /// Extension methods for the <see cref="Either{TError,TOk}"/> type.
  /// </summary>
  public static class EitherExtensions
  {
    /// <summary>
    /// Resolve the two cases. This call is often used at the end of a call chain to collapse the two cases to a single type
    /// that can be used as a return value.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="source">An <see cref="Either{TError,TOk}"/> with identical types, i.e. TError and TOK are identical.</param>
    /// <returns>The value contained. No mapping is done to the value.</returns>
    public static TResult Resolve<TResult>(this Either<TResult, TResult> source) => source.Resolve(x => x, x => x);

    /// <summary>
    /// Select wrapper for LINQ query syntax.
    /// </summary>
    /// <typeparam name="TError">The 'left' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TOk">The 'right' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TMapped">The type of the transformed value.</typeparam>
    /// <param name="source">An instance of <see cref="Either{TError,TOk}"/>.</param>
    /// <param name="selector">A callback which will be called with the wrapped <typeparamref name="TOk"/> value,
    /// if the wrapped value is of that type. Must not be null.</param>
    /// <returns>A new <see cref="Either{TError,TOk}"/> instance where the <typeparamref name="TOk"/> value has been
    /// mapped to a <typeparamref name="TMapped"/> by the <paramref name="selector"/> function.</returns>
    public static Either<TError, TMapped> Select<TError, TOk, TMapped>(this Either<TError, TOk> source, Func<TOk, TMapped> selector)
    {
      if (selector == null)
      {
        throw new ArgumentNullException(nameof(selector));
      }

      return source.MapOk(selector);
    }

    /// <summary>
    /// Select wrapper for LINQ query syntax.
    /// </summary>
    /// <typeparam name="TError">The 'left' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TOk">The 'right' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TMapped">The type of the transformed value.</typeparam>
    /// <param name="source">An instance of a <see cref="Task"/> containing a <see cref="Either{TError,TOk}"/>.</param>
    /// <param name="selector">A callback which will be called with the wrapped <typeparamref name="TOk"/> value,
    /// if the wrapped value is of that type. Must not be null.</param>
    /// <returns>A new <see cref="Task"/> containing a <see cref="Either{TError,TOk}"/> instance where the <typeparamref name="TOk"/>
    /// value has been mapped to a <typeparamref name="TMapped"/> by the <paramref name="selector"/> function.</returns>
    public static async Task<Either<TError, TMapped>> Select<TError, TOk, TMapped>(this Task<Either<TError, TOk>> source, Func<TOk, TMapped> selector)
    {
      if (selector == null)
      {
        throw new ArgumentNullException(nameof(selector));
      }

      return await (await source)
        .BindAsync(ok => Task.FromResult(new Either<TError, TMapped>(selector(ok))));
    }

    /// <summary>
    /// SelectMany wrapper for LINQ query syntax.
    /// </summary>
    /// <typeparam name="TError">The 'left' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TOk">The 'right' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TMapped">The type of the transformed value.</typeparam>
    /// <param name="source">An instance of <see cref="Either{TError,TOk}"/>.</param>
    /// <param name="selector">A callback which will be called with the wrapped <typeparamref name="TOk"/> value,
    /// if the wrapped value is of that type. Must not be null.</param>
    /// <returns>A new <see cref="Either{TError,TOk}"/> instance where the <typeparamref name="TOk"/> value has been
    /// mapped to a <typeparamref name="TMapped"/> by the <paramref name="selector"/> function.</returns>
    public static Either<TError, TMapped> SelectMany<TError, TOk, TMapped>(
      this Either<TError, TOk> source,
      Func<TOk, Either<TError, TMapped>> selector)
    {
      if (selector == null)
      {
        throw new ArgumentNullException(nameof(selector));
      }

      return source.Bind(selector);
    }

    /// <summary>
    /// SelectMany wrapper for LINQ query syntax.
    /// </summary>
    /// <typeparam name="TError">The 'left' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TOk">The 'right' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TMapped">The type of the transformed value.</typeparam>
    /// <typeparam name="TResult">The 'right' type inside the returned <see cref="Either{TError,TResult}"/>.</typeparam>
    /// <param name="source">An instance of <see cref="Either{TError,TOk}"/>.</param>
    /// <param name="eitherSelector">Callback that will be invoked with the inner <typeparamref name="TOk"/> value and maps it
    /// to a <typeparamref name="TMapped"/> value. Must not be null.</param>
    /// <param name="resultSelector">This method is used to 'chain' the inner values of <see cref="Either{TError,TOk}"/> values:
    /// From a <typeparamref name="TOk"/> and a value just transformed by <paramref name="eitherSelector"/> to a value
    /// of type <typeparamref name="TMapped"/>, the final value is constructed and wrapped up in an <see cref="Either{TError,TOk}"/>.
    /// Must not be null.</param>
    /// <returns>A new <see cref="Either{TError,TOk}"/> with the 'combined' value.</returns>
    /// <remarks>This is the equivalent implementation of how <see cref="IEnumerable{T}"/> makes
    /// <code>var x = from t in A from y in B select t.q + y.w</code> possible.</remarks>
    public static Either<TError, TResult> SelectMany<TError, TOk, TMapped, TResult>(
      this Either<TError, TOk> source,
      Func<TOk, Either<TError, TMapped>> eitherSelector,
      Func<TOk, TMapped, TResult> resultSelector)
    {
      if (eitherSelector == null)
      {
        throw new ArgumentNullException(nameof(eitherSelector));
      }

      if (resultSelector == null)
      {
        throw new ArgumentNullException(nameof(resultSelector));
      }

      return source
        .Bind(x => eitherSelector(x)
        .Bind(y => new Either<TError, TResult>(resultSelector(x, y))));
    }

    /// <summary>
    /// SelectMany wrapper for LINQ query syntax supporting asynchronous operations, which is a better fit for things like API interactions.
    /// </summary>
    /// <typeparam name="TError">The 'left' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TOk">The 'right' type of the <see cref="Either{TError,TOk}"/>.</typeparam>
    /// <typeparam name="TMapped">The type of the transformed value.</typeparam>
    /// <typeparam name="TResult">The 'right' type inside the returned <see cref="Task"/> containing a <see cref="Either{TError,TResult}"/>.</typeparam>
    /// <param name="source">An instance of a <see cref="Task"/> containing a <see cref="Either{TError,TOk}"/>. </param>
    /// <param name="eitherSelector">Callback that will be invoked with the inner <typeparamref name="TOk"/> value and maps it
    /// to a <typeparamref name="TMapped"/> value. Must not be null.</param>
    /// <param name="resultSelector">An analogous version of the non-<see cref="Task"/> based version, see <see cref="SelectMany{TError,TOk,TMapped}"/>.</param>
    /// <returns>A new <see cref="Task"/> containing <see cref="Either{TError,TOk}"/> with the 'combined' value.</returns>
    public static async Task<Either<TError, TResult>> SelectMany<TError, TOk, TMapped, TResult>(
      this Task<Either<TError, TOk>> source,
      Func<TOk, Task<Either<TError, TMapped>>> eitherSelector,
      Func<TOk, TMapped, TResult> resultSelector)
    {
      return await (await source)
        .BindAsync(async x => await (await eitherSelector(x))
        .BindAsync(y =>Task.FromResult(new Either<TError, TResult>(resultSelector(x, y)))));
    }
  }
}
