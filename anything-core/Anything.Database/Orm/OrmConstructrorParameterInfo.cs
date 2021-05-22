using System;
using System.Reflection;

namespace Anything.Database.Orm
{
    public class OrmConstructrorParameterInfo
    {
        public OrmPropertyInfo BindProperty { get; }

        public ParameterInfo TargetParameter { get; }

        public OrmTypeInfo ParameterType { get; }

        private Type ValueType => TargetParameter.ParameterType;

        public bool IsNullable => !ValueType.IsValueType ||
                                  (ValueType.IsGenericType && ValueType.GetGenericTypeDefinition() == typeof(Nullable<>));

        public OrmConstructrorParameterInfo(OrmPropertyInfo bindProperty, ParameterInfo targetParameter, OrmTypeInfo parameterType)
        {
            BindProperty = bindProperty;
            TargetParameter = targetParameter;
            ParameterType = parameterType;
        }

        public bool IsAssignableFrom(object? obj)
        {
            if (obj == null)
            {
                return IsNullable;
            }

            return ValueType.IsAssignableFrom(obj.GetType());
        }

        public bool IsAssignableFrom(Type type)
        {
            return ValueType.IsAssignableFrom(type);
        }
    }
}
