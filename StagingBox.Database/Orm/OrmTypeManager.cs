using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace StagingBox.Database.Orm
{
    public class OrmTypeManager
    {
        private readonly List<OrmTypeInfo> _registeredTypes = new();

        public OrmTypeInfo GetOrmTypeInfo(Type targetType)
        {
            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            targetType = OrmLazy.GetUnderlyingType(targetType) ?? targetType;

            if (TryGetOrmTypeInfo(targetType, out var typeInfo))
            {
                return typeInfo;
            }

            throw new InvalidOperationException("No match type.");
        }

        public OrmTypeInfo GetOrmTypeInfo(string name)
        {
            if (TryGetOrmTypeInfo(name, out var typeInfo))
            {
                return typeInfo;
            }

            throw new InvalidOperationException("No match type.");
        }

        public bool TryGetOrmTypeInfo(Type targetType, [MaybeNullWhen(false)] out OrmTypeInfo typeInfo)
        {
            typeInfo = _registeredTypes.FirstOrDefault(ormType => ormType.TargetType == targetType);
            return typeInfo != null;
        }

        public bool TryGetOrmTypeInfo(string name, [MaybeNullWhen(false)] out OrmTypeInfo typeInfo)
        {
            typeInfo = _registeredTypes.FirstOrDefault(ormType => ormType.Name == name);
            return typeInfo != null;
        }

        public virtual OrmTypeInfo RegisterType(Type type, string? name = null)
        {
            var attr = type.GetCustomAttribute<OrmTypeAttribute>();
            name ??= attr?.Name ?? type.Name;
            var typeInfo = new OrmTypeInfo(this, name, type, false);
            _registeredTypes.Add(typeInfo);
            return typeInfo;
        }

        public virtual OrmTypeInfo RegisterScalar(Type type, string? name = null)
        {
            name ??= type.Name;

            var typeInfo = new OrmTypeInfo(this, name, type, true);
            _registeredTypes.Add(typeInfo);
            return typeInfo;
        }
    }
}
