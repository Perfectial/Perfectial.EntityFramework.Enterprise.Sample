using System;
using System.Threading.Tasks;

namespace Perfectial.Infrastructure.Utils
{
    public static class Mapper
    {
        public static TResult Map<TResult>(object source)
            where TResult : class
        {
            throw new NotImplementedException();
        }

        public static TResult MapContents<TSource, TResult>(TSource source, TResult destination)
            where TResult : class
            where TSource : class
        {
            throw new NotImplementedException();
        }
    }
}
