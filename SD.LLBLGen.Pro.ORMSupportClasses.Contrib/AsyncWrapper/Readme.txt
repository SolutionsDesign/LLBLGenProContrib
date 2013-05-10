About the AsyncWrapper code
===============================

The code in this folder is not a full async implementation of the LLBLGen Pro runtime framework. The AsyncWrapper code
wraps the LLBLGen Pro runtime code so it can be called in an asynchronous way using await however all the work is still
done synchronously as the LLBLGen Pro runtime framework is .NET 3.5 code and not async aware. 

The two advantages of Async code in .NET 4.5
---------------------------------------------
Async code in .NET 4.5 has two advantages:

1) Calling async aware code doesn't block the caller
2) Tasks are scheduled by the Task Parallel Library (TPL) scheduler which could lead to a performance gain because clever
   scheduling and the avoidance of waiting could lead to the overall code become faster. 

As this AsyncWrapper is not a full async implementation, it only has the first advantage: it avoids the caller to block. 
To have the second advantage, it requires a full async implementation in the LLBLGen Pro Runtime Framework which requires
.NET 4.0 specific code. This requires two codebases as the runtime is currently also to be supported on.NET 3.5. As we have
just one codebase (the one for .NET 3.5 and up), the second advantage will not be available with this AsyncWrapper. 


Why/when you should (not) use Async Wrapper
---------------------------------------------
Async Wrapper makes it possible to do non-blocking calls to LLBLGen Pro Runtime Framework methods. However as stated above,
it doesn't get rid of e.g. the time wasted when waiting for the RDBMS to complete a query. The first advantage, not having
blocking calls, is done by the async/await feature of C#5/VB.NET 11, but this comes at a small price: under the hood the
compiler will create code to use a thread to make the non-blocking call possible. This is done by the TPL and it's pulled
from the thread pool. In situations where the thread pool is used in other cases, like ASP.NET, it's not efficient to 
use AsyncWrapper. For application where the threadpool isn't used, e.g. desktop applications, it's OK.

For applications which are not allowed to block when calls are made to persistence logic, the AsyncWrapper can help fulfilling
this requirement without any extra code. Be aware that webpages don't regularly fall into this category.