# Actually useful monads for .NET
This is a collection of monads that I'd actually use in production code.

You can express many monads in C#, but not all of them are useful, as idiomatic C# would have you do things another way. 
An example could be that you *could* implement and use the Reader monad, but it would be much more idiomatic to use
dependency injection. That is likely already present in the framework you use anyway.
