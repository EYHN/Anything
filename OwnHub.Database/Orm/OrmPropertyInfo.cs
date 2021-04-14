using System;
using System.Reflection;

namespace OwnHub.Database.Orm
{
    public class OrmPropertyInfo
    {
        public MemberInfo TargetMember { get; }

        private Type ValueType
        {
            get
            {
                if (TargetMember is PropertyInfo propertyInfo)
                {
                    return propertyInfo.PropertyType;
                }

                if (TargetMember is FieldInfo fieldInfo)
                {
                    return fieldInfo.FieldType;
                }

                throw new InvalidOperationException("Invalid state.");
            }
        }

        public string Name { get; }

        public OrmTypeInfo ValueTypeInfo { get; }

        public bool IsReadOnly { get; }

        public bool IsLazy => ValueType.IsGenericType && ValueType.GetGenericTypeDefinition() == typeof(OrmLazy<>);

        public bool IsNullable => !ValueType.IsValueType ||
                                  (ValueType.IsGenericType && ValueType.GetGenericTypeDefinition() == typeof(Nullable<>));

        public OrmPropertyInfo(string name, PropertyInfo property, OrmTypeInfo valueTypeInfo, bool isReadOnly)
        {
            TargetMember = property;
            Name = name;
            ValueTypeInfo = valueTypeInfo;
            IsReadOnly = isReadOnly;
        }

        public OrmPropertyInfo(string name, FieldInfo field, OrmTypeInfo valueTypeInfo, bool isReadOnly)
        {
            TargetMember = field;
            Name = name;
            ValueTypeInfo = valueTypeInfo;
            IsReadOnly = isReadOnly;
        }

        public object? ReadValue(object instance)
        {
            if (TargetMember is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetValue(instance);
            }

            if (TargetMember is FieldInfo fieldInfo)
            {
                return fieldInfo.GetValue(instance);
            }

            throw new InvalidOperationException("Invalid state.");
        }

        public void SetValue(object? instance, object? value)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Property is read only.");
            }

            if (!IsAssignableFrom(value))
            {
                throw new ArgumentException("Value type error.", nameof(value));
            }

            if (TargetMember is PropertyInfo propertyInfo)
            {
                propertyInfo.SetValue(instance, value);
                return;
            }

            if (TargetMember is FieldInfo fieldInfo)
            {
                fieldInfo.SetValue(instance, value);
                return;
            }

            throw new InvalidOperationException("Invalid state.");
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
