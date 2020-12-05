﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetResults
{
	/// <summary>
	/// Base Result class encapsulating either a success value of TOk or error value of TError
	/// </summary>
	public class Result<TOk, TError>
	{
		internal Result() { }

		public static implicit operator Result<TOk, TError>(TOk obj) => Result.Ok<TOk, TError>(obj);
		public static implicit operator Result<TOk, TError>(TError obj) => Result.Error<TOk, TError>(obj);

		public override bool Equals(object obj)
		{
			return obj switch
			{
				Ok<TOk, TError> a when this is Ok<TOk, TError> b => a.Value.Equals(b.Value),
				Error<TOk, TError> c when this is Error<TOk, TError> d => c.ErrorValue.Equals(d.ErrorValue),
				_ => false
			};
		}

		public static bool operator ==(Result<TOk, TError> a, Result<TOk, TError> b) => b.Equals(a);
		public static bool operator !=(Result<TOk, TError> a, Result<TOk, TError> b) => !b.Equals(a);

		/// <summary>
		/// Guard method for Results
		/// </summary>
		/// <param name="ok">The success result. 'default' if the result is an error. </param>
		/// <param name="err">The error result. 'default' if the result is a success.</param>
		/// <returns>True if the result is a success, false if the result is an error. </returns>
		public bool IsOk(out TOk ok, out TError err)
		{
			(ok, err) = this switch
			{
				Ok<TOk, TError> okres => (okres.Value, default(TError)),
				Error<TOk, TError> errres => (default(TOk), errres.ErrorValue)
			};

			return this is Ok<TOk, TError>;
		}

		/// <summary>
		/// Guard method for Results
		/// </summary>
		/// <param name="ok">The success result. 'default' if the result is an error. </param>
		/// <param name="err">The error result. 'default' if the result is a success.</param>
		/// <returns>True if the result is an error, false if the result is a success. </returns>
		public bool IsError(out TOk ok, out TError err) => !IsOk(out ok, out err);
		
		/// <summary>
		/// Guard method for Results
		/// </summary>
		/// <param name="ok">The success result. 'default' if the result is an error. </param>
		/// <returns>True if the result is a success, false if the result is an error. </returns>
		public bool IsOk(out TOk ok) => IsOk(out ok, out _);

		/// <summary>
		/// Guard method for Results
		/// </summary>
		/// <param name="err">The error result. 'default' if the result is a success.</param>
		/// <returns>True if the result is an error, false if the result is a success. </returns>
		public bool IsError(out TError err) => !IsOk(out _, out err);
		
		#region Higher Order Functions
		
		#region Bind
		
		/// <summary>
		/// Binds the result value to a new result value asynchronously 
		/// </summary>
		public async Task<Result<TOutput, TError>> BindAsync<TOutput>(
			Func<TOk, Task<Result<TOutput, TError>>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => await pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err)
			};
		}
		
		/// <summary>
		/// Binds the result value to a new result value
		/// </summary>
		public Result<TOutput, TError> Bind<TOutput>(Func<TOk, Result<TOutput, TError>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err.ErrorValue)
			};
		}

		/// <summary>
		/// Binds the possible error value with a new error value
		/// </summary>
		public Result<TOk, TOutError> BindError<TOutError>(Func<TError, Result<TOk, TOutError>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => Result.Ok<TOk, TOutError>(ok.Value),
				Error<TOk, TError> err => pred(err.ErrorValue)
			};
		}

		/// <summary>
		/// Binds the result to a new error value asynchronously if the current result is an error
		/// </summary>
		/// <param name="onerror">Function to run if the result in an error</param>
		/// <returns>Task returning the new Result</returns>
		public async Task<Result<TOk, TOutError>> BindErrorAsync<TOutError>(
			Func<TError, Task<Result<TOk, TOutError>>> onerror)
		{
			return this switch
			{
				Ok<TOk, TError> ok => ok.Value,
				Error<TOk, TError> err => await onerror(err.ErrorValue)
			};
		}

		/// <summary>
		/// Binds the successful result to a new result and catches all exceptions.
		/// Excepts are handled by the "catch" parameter.
		/// </summary>
		public Result<TOkOutput, TError> BindTry<TOkOutput>(Func<TOk, Result<TOkOutput, TError>> Try, 
			Func<Exception, Result<TOkOutput, TError>> Catch)
		{
			if (this is Error<TOk, TError> err) return Result.Error<TOkOutput, TError>(err.ErrorValue);
			try
			{
				var ok = (Ok<TOk, TError>) this;
				return Try(ok);
			}
			catch (Exception e)
			{
				return Catch(e);
			}
		}
		
		/// <summary>
		/// Binds the successful result to a new result and catches exceptions asynchronously 
		/// </summary>
		/// <param name="Try">Function binder</param>
		/// <param name="Catch">Function invoked if Exception is thrown</param>
		/// <returns>Task returning the new Result</returns>
		public async Task<Result<TOkOutput, TError>> BindTryAsync<TOkOutput>(
			Func<TOk, Task<Result<TOkOutput, TError>>> Try, 
			Func<Exception, Task<Result<TOkOutput, TError>>> Catch)
		{
			if (this is Error<TOk, TError> err) return Result.Error<TOkOutput, TError>(err.ErrorValue);
			try
			{
				var ok = (Ok<TOk, TError>) this;
				return await Try(ok);
			}
			catch (Exception e)
			{
				return await Catch(e);
			}
		}
		
		#endregion
		
		#region Map
		
		/// <summary>
		/// Maps the success result to a new value asynchronously.
		/// </summary>
		public async Task<Result<TOutput, TError>> MapAsync<TOutput>(Func<TOk, Task<TOutput>> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => await pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err)
			};
		}
		
		/// <summary>
		/// Maps the success result to a new value
		/// </summary>
		public Result<TOutput, TError> Map<TOutput>(Func<TOk, TOutput> pred)
		{
			return this switch
			{
				Ok<TOk, TError> ok => pred(ok),
				Error<TOk, TError> err => Result.Error<TOutput, TError>(err)
			};
		}

		/// <summary>
		/// Maps the error result to a new value
		/// </summary>
		/// <returns></returns>
		public Result<TOk, TError> MapError(Func<TError, TOk> onerror)
		{
			return this switch
			{
				Ok<TOk, TError> ok => ok.Value,
				Error<TOk, TError> err => onerror(err)
			};
		}
		
		/// <summary>
		/// Maps the successful result to a new value and catches a all exceptions.
		/// Exceptions are handled by the "catch" parameter.
		/// </summary>
		public Result<TOkOutput, TError> MapTry<TOkOutput>(Func<TOk, TOkOutput> Try, Func<Exception, TError> Catch)
		{
			if (this is Error<TOk, TError> err) return Result.Error<TOkOutput, TError>(err.ErrorValue);
			try
			{
				var ok = (Ok<TOk, TError>) this;
				return Try(ok);
			}
			catch (Exception e)
			{
				return Catch(e);
			}
		}
		
		/// <summary>
		/// Attempts to map the result to a new value asynchronously. 
		/// </summary>
		/// <param name="Try">Mapper that can throw</param>
		/// <param name="Catch">Function to be invoked if an exception is thrown</param>
		/// <returns>Task returning the new Result</returns>
		public async Task<Result<TOkOutput, TError>> MapTryAsync<TOkOutput>(
			Func<TOk, Task<TOkOutput>> Try, 
			Func<Exception, Task<TError>> Catch)
		{
			if (this is Error<TOk, TError> err) return Result.Error<TOkOutput, TError>(err.ErrorValue);
			try
			{
				var ok = (Ok<TOk, TError>) this;
				return await Try(ok);
			}
			catch (Exception e)
			{
				return await Catch(e);
			}
		}
		
		#endregion
		
		#region OnSuccess
		
		/// <summary>
		/// Performs an asynchronous action if the result is a success.
		/// Returns the input Result.
		/// </summary>
		public async Task<Result<TOk, TError>> OnSuccessAsync(Func<TOk, Task> onsuccess)
		{
			if (this is Ok<TOk, TError> ok) await onsuccess(ok);
			return this;
		}

		/// <summary>
		/// Performs an action if it is a success.
		/// Returns the input Result.
		/// </summary>
		public Result<TOk, TError> OnSuccess(Action<TOk> onsuccess)
		{
			if (this is Ok<TOk, TError> ok) onsuccess(ok);
			return this;
		}

		public Result<TOk, TError> OnSuccessTry(Action<TOk> Try, Func<Exception, TError> Catch)
		{
			if (this is Ok<TOk, TError> ok)
			{
				try
				{
					Try(ok);
					return this;
				}
				catch (Exception e)
				{
					return Catch(e);
				}
			}

			return this;
		}
		
		#endregion

		#region OnError

		/// <summary>
		/// Performs an action if the result is an error.
		/// Returns the input Result.
		/// </summary>
		public Result<TOk, TError> OnError(Action<TError> onerror)
		{
			if (this is Error<TOk, TError> err) onerror(err);
			return this;
		}

		/// <summary>
		/// Performs an asynchronous action if the result is an error.
		/// Returns the input result 
		/// </summary>
		public async Task<Result<TOk, TError>> OnErrorAsync(Func<TError, Task> onerror)
		{
			if (this is Error<TOk, TError> err) await onerror(err);
			return this;
		}
		
		#endregion

		#region Expect

		/// <summary>
		/// Force unwraps a result. 
		/// </summary>
		/// <returns>The unwrapped successful result</returns>
		/// <exception cref="UnwrappedResultException">Thrown if the unwrap is unsuccessful</exception>
		public TOk Expect()
		{
			return this switch
			{
				Ok<TOk, TError> ok => ok,
				_ => throw new UnwrappedResultException<TError>($"Unsuccessful unwrap of result of type: {typeof(TOk)}")
			};
		}

		/// <summary>
		/// Force unwraps a result. 
		/// </summary>
		/// <param name="e">Inner exception thrown if the unwrap is unsuccessful</param>
		/// <returns>The unwrapped successful result</returns>
		/// <exception cref="UnwrappedResultException">Thrown if the unwrap is unsuccessful</exception>
		public TOk Expect(Exception e)
		{
			return this switch
			{
				Ok<TOk, TError> ok => ok,
				_ => throw new UnwrappedResultException<TError>($"Unsuccessful unwrap of result of type: {typeof(TOk)}", e)
			};
		}
		
		#endregion

		#region Try

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Try"></param>
		/// <param name="Catch"></param>
		/// <typeparam name="TException"></typeparam>
		public void Try<TException>(Action<Result<TOk, TError>> Try, Action<TException> Catch) where TException: Exception
		{
			try
			{
				Try(this);
			}
			catch (TException e)
			{
				Catch(e);
			}
		}

		public TOutput Try<TException, TOutput>(Func<Result<TOk, TError>, TOutput> Try, Func<TException, TOutput> Catch) 
			where TException : Exception
		{
			try
			{
				return Try(this);
			}
			catch (TException e)
			{
				return Catch(e);
			}
		}
		
		#endregion

		#endregion
	}

	public sealed class Ok<TOk, TError> : Result<TOk, TError>
	{
		internal Ok(TOk value)
		{
			Value = value;
		}
		
		internal TOk Value { get; }

		public override string ToString() => Value.ToString();

		public static implicit operator TOk(Ok<TOk, TError> ok) => ok.Value;

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}

	public sealed class Error<TOk, TError> : Result<TOk, TError>
	{
		internal Error(TError errorValue)
		{
			ErrorValue = errorValue;
		}
		
		internal TError ErrorValue { get; }

		public override string ToString() => ErrorValue.ToString();

		public static implicit operator TError(Error<TOk, TError> err) => err.ErrorValue;
	}

	public static class Result
	{
		/// <summary>
		/// Converts a value to a successful result
		/// </summary>
		public static Result<TOk, TError> Ok<TOk, TError>(this TOk obj) => new Ok<TOk, TError>(obj);
		
		/// <summary>
		/// Converts a value to an unsuccessful result
		/// </summary>
		public static Result<TOk, TError> Error<TOk, TError>(this TError err) => new Error<TOk, TError>(err);

		#region Static Trys
		
		/// <summary>
		/// Functions as a try/catch block and converts to a result
		/// </summary>
		/// <param name="throwable">Function that may throw an exception</param>
		/// <param name="handler">Exception handler function</param>
		public static Result<TOk, TError> Try<TOk, TError>(Func<TOk> throwable, Func<Exception, TError> handler)
		{
			try
			{
				return throwable();
			}
			catch (Exception e)
			{
				return handler(e);
			}
		}

		/// <summary>
		/// Functions as a try/catch block and converts to a result
		/// </summary>
		/// <param name="throwable">Function that may throw an exception</param>
		/// <param name="handler">Exception handler function</param>
		public static Result<TOk, TError> Try<TOk, TError>(Func<Result<TOk, TError>> throwable,
			Func<Exception, Result<TOk, TError>> handler)
		{
			try
			{
				return throwable();
			}
			catch (Exception e)
			{
				return handler(e);
			}
		}

		/// <summary>
		/// Functions as a try/catch block and converts to a result
		/// </summary>
		/// <param name="throwable">Function that may throw an exception</param>
		/// <param name="handler">Exception handler function</param>
		public static async Task<Result<TOk, TError>> TryAsync<TOk, TError>(Func<Task<TOk>> throwable,
			Func<Exception, Task<TError>> handler)
		{
			try
			{
				return await throwable();
			}
			catch (Exception e)
			{
				return await handler(e);
			}
		}

		/// <summary>
		/// Functions as a try/catch block and converts to a result
		/// </summary>
		/// <param name="throwable">Function that may throw an exception</param>
		/// <param name="handler">Exception handler function</param>
		public static async Task<Result<TOk, TError>> TryAsync<TOk, TError>(
			Func<Task<Result<TOk, TError>>> throwable,
			Func<Exception, Task<Result<TOk, TError>>> handler)
		{
			try
			{
				return await throwable();
			}
			catch (Exception e)
			{
				return await handler(e);
			}
		}
		
		#endregion

		/// <summary>
		/// Asynchronous map extension method for Result Tasks
		/// </summary>
		/// <param name="pred">Asynchronous Function</param>
		public static async Task<Result<TOutput, TError>> MapAsync<TInput, TOutput, TError>(
			this Task<Result<TInput, TError>> res,
			Func<TInput, Task<TOutput>> pred)
		{
			return await res switch
			{
				Ok<TInput, TError> ok => await pred(ok),
				Error<TInput, TError> err => Error<TOutput, TError>(err)
			};
		}

		/// <summary>
		/// Asynchronous bind extension method for Result Tasks
		/// </summary>
		/// <param name="pred">Asynchronous Function</param>
		public static async Task<Result<TOutput, TError>> BindAsync<TInput, TOutput, TError>(
			this Task<Result<TInput, TError>> res,
			Func<TInput, Task<Result<TOutput, TError>>> pred)
		{
			return await res switch
			{
				Ok<TInput, TError> ok => await pred(ok),
				Error<TInput, TError> err => Error<TOutput, TError>(err)
			};
		}

		public static async Task<Result<TOk, TError>> OnSuccessAsync<TOk, TError>(
			this Task<Result<TOk, TError>> res,
			Func<TOk, Task> onsuccess)
		{
			var awaited = await res;
			if (awaited is Ok<TOk, TError> ok) await onsuccess(ok);
			return awaited;
		}

		public static async Task<Result<TOk, TError>> OnErrorAsync<TOk, TError>(
			this Task<Result<TOk, TError>> res,
			Func<TError, Task> onerror)
		{
			var awaited = await res;
			if (awaited is Error<TOk, TError> err) await onerror(err);
			return awaited;
		}

		#region Match
		
		/// <summary>
		/// Performs a switch depending on the success or failure of the result. 
		/// </summary>
		/// <param name="ok">Function to run if the result is successful</param>
		/// <param name="err">Function to run if the result is a failure</param>
		public static TOutput Match<TOk, TError, TOutput>(
			this Result<TOk, TError> result,
			Func<TOk, TOutput> ok,
			Func<TError, TOutput> err)
		{
			return result switch
			{
				Ok<TOk, TError> success => ok(success),
				Error<TOk, TError> error => err(error)
			};
		}

		/// <summary>
		/// Performs an asynchronous switch depending on the success or failure of the result. 
		/// </summary>
		/// <param name="ok">Function to run if the result is successful</param>
		/// <param name="err">Function to run if the result is a failure</param>
		public static async Task<TOutput> MatchAsync<TOk, TError, TOutput>(
			this Result<TOk, TError> result,
			Func<TOk, Task<TOutput>> ok,
			Func<TError, Task<TOutput>> err)
		{
			return result switch
			{
				Ok<TOk, TError> success => await ok(success),
				Error<TOk, TError> error => await err(error)
			};
		}

		/// <summary>
		/// Performs an asynchronous switch depending on the success or failure of the result. 
		/// </summary>
		/// <param name="ok">Function to run if the result is successful</param>
		/// <param name="err">Function to run if the result is a failure</param>
		public static async Task<TOutput> MatchAsync<TOk, TError, TOutput>(
			this Task<Result<TOk, TError>> result,
			Func<TOk, Task<TOutput>> ok,
			Func<TError, Task<TOutput>> err)
		{
			return await result switch
			{
				Ok<TOk, TError> success => await ok(success),
				Error<TOk, TError> error => await err(error)
			};
		}
		
		#endregion
	}
}