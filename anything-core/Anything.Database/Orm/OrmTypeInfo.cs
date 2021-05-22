using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Anything.Database.Orm
{
    public class OrmTypeInfo
    {
        public OrmTypeManager TypeManager { get; }

        public string Name { get; }

        public Type TargetType { get; }

        public OrmPropertyInfo[] Properties { get; }

        public OrmConstructorInfo Constructor { get; }

        public bool IsScalar { get; }

        public OrmTypeInfo(OrmTypeManager typeManager, string name, Type targetType, bool isScalar)
        {
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            TypeManager = typeManager;
            Name = name;
            TargetType = targetType;
            IsScalar = isScalar;
            Properties = isScalar ? new OrmPropertyInfo[] { } : AnalyzeProperties(targetType);
            Constructor = isScalar ? null! : AnalyzeConstructor(targetType);
        }

        public OrmPropertyInfo? GetProperty(string name)
        {
            return Properties.FirstOrDefault(info => info.Name == name);
        }

        public OrmPropertyInfo[] AnalyzeProperties(Type type)
        {
            var result = new List<OrmPropertyInfo>();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<OrmPropertyAttribute>();
                if (attribute == null || !property.CanRead)
                {
                    continue;
                }

                var propertyName = attribute.Name ?? property.Name;

                var valueType = TypeManager.GetOrmTypeInfo(property.PropertyType);
                result.Add(new OrmPropertyInfo(propertyName, property, valueType, !property.CanWrite));
            }

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<OrmPropertyAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                var propertyName = attribute.Name ?? field.Name;
                var valueTypeInfo = TypeManager.GetOrmTypeInfo(field.FieldType);
                result.Add(new OrmPropertyInfo(propertyName, field, valueTypeInfo, field.IsInitOnly));
            }

            return result.ToArray();
        }

        public OrmConstructorInfo AnalyzeConstructor(Type type)
        {
            var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            ConstructorInfo? ctorInfo = null;
            foreach (var ctor in ctors)
            {
                if (ctor.GetCustomAttribute<OrmConstructorAttribute>() != null)
                {
                    ctorInfo = ctor;
                }
            }

            if (ctorInfo == null)
            {
                ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);
            }

            if (ctorInfo == null)
            {
                throw new InvalidOperationException("No constructor available.");
            }

            var parameters = ctorInfo.GetParameters();
            var parameterInfos = new List<OrmConstructrorParameterInfo>();

            foreach (var parameter in parameters)
            {
                var attr = parameter.GetCustomAttribute<OrmPropertyAttribute>();
                var propertyName = attr?.Name ?? parameter.Name;
                if (propertyName == null)
                {
                    throw new InvalidOperationException("Missing parameter name");
                }

                var parameterTypeInfo = TypeManager.GetOrmTypeInfo(parameter.ParameterType);

                var bindProperty = Properties.FirstOrDefault(info =>
                    info.Name == propertyName && info.IsAssignableFrom(parameter.ParameterType));

                if (bindProperty == null)
                {
                    throw new InvalidOperationException(
                        $"The property associated with the parameter named '{parameter.Name}' could not be found.");
                }

                parameterInfos.Add(new OrmConstructrorParameterInfo(bindProperty, parameter, parameterTypeInfo));
            }

            return new OrmConstructorInfo(ctorInfo, parameterInfos.ToArray());
        }

        public override string ToString()
        {
            return $"OrmTypeInfo [{TargetType}]";
        }
    }
}
