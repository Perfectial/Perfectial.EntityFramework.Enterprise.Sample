namespace Perfectial.Infrastructure.Identity
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Owin;
    using Microsoft.Owin.Security.DataProtection;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.OAuth;

    using Owin;

    using Perfectial.Domain.Model;

    internal static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            CultureInfo cultureUi = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.CurrentCulture;
            return System.Threading.Tasks.TaskExtensions.Unwrap<TResult>(AsyncHelper._myTaskFactory.StartNew<Task<TResult>>((Func<Task<TResult>>)(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }))).GetAwaiter().GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            CultureInfo cultureUi = CultureInfo.CurrentUICulture;
            CultureInfo culture = CultureInfo.CurrentCulture;
            System.Threading.Tasks.TaskExtensions.Unwrap(AsyncHelper._myTaskFactory.StartNew<Task>((Func<Task>)(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }))).GetAwaiter().GetResult();
        }
    }
}
