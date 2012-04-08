namespace Simple.Azure.Rest
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        const int RetryCount = 3;
        const int RetryIntervalMS = 200;

        public static Task<HttpWebResponse> ContinueWithResponse(this Task<HttpWebRequest> requestTask)
        {
            return requestTask
                .ContinueWith(t => Task.Factory.FromAsync(t.Result.BeginGetResponse, ar => (HttpWebResponse)t.Result.EndGetResponse(ar), null))
                .Unwrap();
        }

        public static Task<TResult> ContinueWith<T, TResult>(this Task<T> task, Func<T, TResult> func, WebExceptionHandler<TResult> exceptionHandler)
        {
            return task.ContinueWith(t =>
                                         {
                                             if (t.IsFaulted)
                                             {
                                                 if (t.Exception == null) throw new ApplicationException();
                                                 if (exceptionHandler == null)
                                                 {
                                                     throw t.Exception.InnerException;
                                                 }
                                                 return exceptionHandler.Handle(t.Exception);
                                             }

                                             return func(t.Result);
                                         });
        }


        public static T Catch<TException, T>(this AggregateException aggregate, Func<TException, T> handler)
            where TException : Exception
        {
            var exception = aggregate.Flatten().InnerExceptions.OfType<TException>().FirstOrDefault();

            if (exception == null)
            {
                throw aggregate;
            }

            return handler(exception);
        }

        public static Task<T> Retry<T>(Func<Task<T>> taskFunc)
        {
            return Retry(RetryCount, taskFunc);
        }

        public static Task<T> Retry<T>(int retryCount, Func<Task<T>> taskFunc)
        {
            return taskFunc().ContinueWith(t =>
                                               {
                                                   if (t.IsFaulted)
                                                   {
                                                       if (--retryCount < 1)
                                                       {
                                                           return t;
                                                       }
                                                       return Retry(retryCount - 1, taskFunc);
                                                   }
                                                   return t;
                                               }).Unwrap();
        }
    }
}