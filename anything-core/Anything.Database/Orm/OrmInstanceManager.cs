using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Anything.Database.Orm
{
    public class OrmInstanceManager
    {
        private ConditionalWeakTable<object, OrmInstanceInfo> _instanceInfoTable = new();

        /// <summary>
        /// Gets the transaction associated with the instance manager.
        /// </summary>
        public OrmTransaction Transaction { get; }

        public OrmInstanceManager(OrmTransaction transaction)
        {
            Transaction = transaction;
        }

        public bool TryGetInstanceInfo(object instance, [MaybeNullWhen(false)] out OrmInstanceInfo instanceInfo)
        {
            return _instanceInfoTable.TryGetValue(instance, out instanceInfo);
        }

        public object CreateInstance(OrmTypeInfo typeInfo, long objectId, OrmSnapshot snapshot, out OrmInstanceInfo instanceInfo)
        {
            if (typeInfo.IsScalar)
            {
                throw new InvalidOperationException("Can't create instance for scalar type.");
            }

            var ctorParamterBindProperties = typeInfo.Constructor.Parameters.Select(parameter => parameter.BindProperty).ToArray();
            var paramters = ctorParamterBindProperties.Select(property => snapshot.GetValueOrDefault(property)).ToArray();
            var instance = typeInfo.Constructor.Invoke(paramters);
            if (instance == null)
            {
                throw new InvalidOperationException("Create instance failed");
            }

            foreach (var pair in snapshot)
            {
                if (pair.Key.IsReadOnly || ctorParamterBindProperties.Contains(pair.Key))
                {
                    continue;
                }

                pair.Key.SetValue(instance, pair.Value);
            }

            instanceInfo = new OrmInstanceInfo(Transaction, objectId, instance, typeInfo);
            instanceInfo.SavedState = snapshot;

            _instanceInfoTable.Add(instance, instanceInfo);
            return instance;
        }

        public OrmInstanceInfo RegisterInstance(object instance, OrmTypeInfo typeInfo, long objectId, OrmSnapshot snapshot)
        {
            if (typeInfo.IsScalar)
            {
                throw new InvalidOperationException("Can't manage instance of scalar type.");
            }

            var instanceInfo = new OrmInstanceInfo(Transaction, objectId, instance, typeInfo);
            instanceInfo.SavedState = snapshot;

            _instanceInfoTable.Add(instance, instanceInfo);
            return instanceInfo;
        }

        public void UnregisterInstance(object instance)
        {
            _instanceInfoTable.Remove(instance);
        }
    }
}
