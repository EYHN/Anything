using System;
using System.Reflection;

namespace OwnHub.Sqlite.Orm
{
    public class OrmConstructorInfo
    {
        public ConstructorInfo TargetConstructor { get; }

        public OrmConstructrorParameterInfo[] Parameters { get; }

        public OrmConstructorInfo(ConstructorInfo targetConstructor, OrmConstructrorParameterInfo[] parameters)
        {
            TargetConstructor = targetConstructor;
            Parameters = parameters;
        }

        public object Invoke(object?[]? parameters)
        {
            if ((parameters == null && Parameters.Length != 0) || (parameters != null && parameters.Length != Parameters.Length))
            {
                throw new ArgumentException("The number of parameters is wrong.");
            }

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (!Parameters[i].IsAssignableFrom(parameters[i]))
                    {
                        throw new ArgumentException("The type of parameters is wrong.");
                    }
                }
            }

            return TargetConstructor.Invoke(parameters);
        }
    }
}
