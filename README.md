### NetResult

A simple and straightforward result type for C#. Inspired by Result type in language like F#, Rust, and Swift. 

---

#### Creation

`Result` types are created using static methods `Result.Ok()` and `Result.Error()` with provided success and error types. 

```C#
// A Successful Result
var success = Result.Ok<MyObject, string>(new MyObject());

// An Error Result
var error = Result.Error<MyObject, string>("An error occurred. ");
```

When returning results, the types are inferred and can the static methods can omitted.

```c#
public Result<int, string> ErrorIfOverTen(int a)
{
  if (a > 10)
    return "An error occurred. 'a' was greater than 10. ";
  else
    return a;
}
```

---

#### Accessing Results

Results are built to be accessed in various ways. Safe ways include:

Pattern matching:

```c#
var res = ErrorIfOverTen(4);

switch (res) 
{
  case Ok<int, string> ok:
    Console.WriteLine("Successful Result:" + ok);
    // Note that 'ok' and 'err' values are implicitly read as their underlying unboxed values.
    // Therefore the following is valid:
    return ok + 4;
  case Error<int, string> err:
    Console.WriteLine("Error occured: " + err);
    return 0;
}
```

the `OnSuccess()` method performs some function on the value if it is a successful result.

```c#
var res = ErrorIfOverTen(20);

res.OnSuccess(
    ok => Console.WriteLine("Success: " + ok)
);
```

the `OnError()` method performs some function if the value is unsuccessful:

```c#
var res = ErrorIfOverTen(6);

res.OnError(
    err => Console.WriteLine("An error occurred: " + err)
);
```

Both `OnSuccess` and `OnError` return the original result, so they can chained together:

```c#
var res = ErrorIfOverTen(9);

res.OnSuccess(ok => Console.WriteLine("Success: " + ok))
  .OnError(err => Console.WriteLine("An error occurred: " + err));
```

If a valid value needs returned regardless of the result, `Match()` can be used

```c#
var res = ErrorIfOverTen(20);

// Note that both methods must return the same type
var newNumber = res.Match(
    ok => ok + 4 * 3, // on success
    err => 0 // on error
)
```



The following ways of accessing the results are unsafe, so should be used with caution. 

Guard methods:

```c#
var res = ErrorIfOverTen(13);

// if the result is a success, 'err' will be 'default(string)' in this scenario
if (res.IsError(out string err))
{
  Console.WriteLine(err);
  return;
}

// if the reuslt is an error, 'ok' will be 'default(int)' in this scenario
if (res.IsOk(out int ok))
{
  DoSomethingWithNumber(ok);
  return;
}

// These two methods can be combined into
if (res.IsError(out var ok, out var err))
{
  Console.WriteLine(err);
  return;
}

// Use the result since we know it'll have a value. 
DoSomethingWithNumber(ok);

// Note: the inverse of `res.IsError(out var ok, out var err)` exists as `res.IsOk(out var ok, out var err)`
```

the `Except` method can be used to force unwrap a result at the risk of throwing an `UnwrappedResultException` if the result is an error. This method can be given an optional `Exception` object to be the inner exception of the `UnwrappedResultException`

```c#
var res = ErrorIfOverTen(-3);

// Throws UnwrappedResultException
var forcedInteger = res.Except();

// Throw UnwrappedResultException with custom inner exception
var myException = new MyCustomException("My Exception's message");
forcedInteger = res.Except(myException);
```

---

#### Binding and Mapping

Results have methods `Bind()`, `Map()`, and `BindError()` for safely performing operations on results. 

Map does not affect the error value if it exits, and it will be propogated up:

```c#
var res = ErrorIfOverTen(3);

var newRes = res.Map(x => x * 2); 

newRes.OnSuccess(ConsoleWriteLine); // Prints "6"

res = ErrorIfOverTen(13);

newRes = res.Map(x => x + 3);

newRes.OnError(Console.WriteLine); // Prints "An error occurred. 'a' was greater than 10. "
```

Bind also does not affect the error, but is used to to bind the result to a new result

```c#
var res = ErrorIfOverTen(6);

var newRes = res.Map(x => x * 2) // The result is now 12
  .Bind(ErrorIfOverTen) // The result is now an error
```

---

#### Combining with Exceptions

The `Result` class has a static `Try()` method for mapping some operation than can throw into a result type

```c#
public void FunctionThatCanThrow()
{
    // Perform operation that can throw exception
}

public void HandleMyException(Exception e)
{
    // Handle the exception  
}

Result.Try(
	() => FunctionThatCanThrow(), 
    e => HandleMyException(e)
);

// Or shorthand
Result.Try(FunctionThatCanThrow, HandleMyException);
```

`Try()` can also return values if needed, however both the throwable function and the exception handler must return the same type

Result types themselves also have a `Try()` method that can be called on them to attempt to perform a throwable operation on the result itself. Like the static function, these methods can also return a value. 

```c#
var res = ErrorIfOverTen(14);

res.Try(
    ok => FunctionThatCanThrow(ok),
    e => Console.WriteLine(e)
)
```

---

#### Asynchronous Results

All mentioned methods have `Async` version to deal with asynchronous operations

```c#
public async Task<Result<MyObject, string>> LongOperationAysnc(int input)
{
  // perform async operation
  
  if (thereWasAnError)
    return "Oops, an error occurred. ";
  else
    return new MyObject();
}

Result<MyObject, string> res = await ErrorIfOverTen(3)
  .BindAsync(LongOperationAsync)
  .OnSuccessAsync(myObj => DoSomethingWith(myObj))
  .OnErrorAsync(err => Console.WriteLine(err));
```

