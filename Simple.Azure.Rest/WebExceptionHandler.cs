namespace Simple.Azure.Rest
{
    using System;
    using System.Linq;
    using System.Net;

    public class WebExceptionHandler<T>
    {
        private readonly int[] _statusCodes;
        private readonly Func<T> _handler;

        public WebExceptionHandler(int[] statusCodes)
            : this(statusCodes, () => default(T))
        {
        }

        public WebExceptionHandler(int[] statusCodes, Func<T> handler)
        {
            _statusCodes = statusCodes;
            _handler = handler;
        }

        public WebExceptionHandler<TReturn> With<TReturn>(Func<TReturn> handler)
        {
            return new WebExceptionHandler<TReturn>(_statusCodes, handler);
        }

        public WebExceptionHandler<TReturn> WithDefault<TReturn>()
        {
            return new WebExceptionHandler<TReturn>(_statusCodes, () => default(TReturn));
        }

        public T Handle(AggregateException exception)
        {
            var webException = exception.Flatten().InnerExceptions.OfType<WebException>().FirstOrDefault();
            if (webException == null) throw exception;

            var response = webException.Response as HttpWebResponse;
            if (response != null && _statusCodes.Contains((int)response.StatusCode))
            {
                return _handler();
            }

            throw exception;
        }
    }
}