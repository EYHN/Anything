using System;
using System.Reflection;

#pragma warning disable SA1402

namespace StagingBox.Database.Orm
{
    public static class OrmLazy
    {
        public static OrmLazy<T> Resolved<T>(T obj)
            where T : class?
        {
            return new(() => obj);
        }

        public static Type? GetUnderlyingType(Type ormLazyType)
        {
            if ((object)ormLazyType == null)
            {
                throw new ArgumentNullException(nameof(ormLazyType));
            }

            if (ormLazyType.IsGenericType && !ormLazyType.IsGenericTypeDefinition)
            {
                // instantiated generic type only
                Type genericType = ormLazyType.GetGenericTypeDefinition();
                if (ReferenceEquals(genericType, typeof(OrmLazy<>)))
                {
                    return ormLazyType.GetGenericArguments()[0];
                }
            }

            return null;
        }

        public static Type MakeGenericType(Type underlyingType)
        {
            return typeof(OrmLazy<>).MakeGenericType(underlyingType);
        }

        public static object? GetValue(object lazyObj)
        {
            var valueProperty = lazyObj.GetType().GetProperty("Value");
            return valueProperty!.GetValue(lazyObj);
        }
    }

    public class OrmLazy<T>
        where T : class?
    {
        protected Lazy<T> Lazy { get; }

        public OrmLazy(Func<T> resultProvider)
        {
            Lazy = new Lazy<T>(resultProvider);
        }

        public T? Value => Lazy.Value;
    }
}
