namespace Perfectial.Infrastructure.Identity.Base
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public struct CultureAwaiter : ICriticalNotifyCompletion
    {
        private readonly Task task;

        public bool IsCompleted => this.task.IsCompleted;

        public CultureAwaiter(Task task)
        {
            this.task = task;
        }

        public CultureAwaiter GetAwaiter() => this;

        public void GetResult()
        {
            this.task.GetAwaiter().GetResult();
        }

        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;

            this.task.ConfigureAwait(false).GetAwaiter().UnsafeOnCompleted((() =>
            {
                CultureInfo temporaryCulture = Thread.CurrentThread.CurrentCulture;
                CultureInfo temporaryUICulture = Thread.CurrentThread.CurrentUICulture;

                Thread.CurrentThread.CurrentCulture = currentCulture;
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
                try
                {
                    continuation();
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = temporaryCulture;
                    Thread.CurrentThread.CurrentUICulture = temporaryUICulture;
                }
            }));
        }
    }
}
