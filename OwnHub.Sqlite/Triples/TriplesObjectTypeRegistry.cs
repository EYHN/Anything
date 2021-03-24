using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OwnHub.Sqlite.Triples
{
    internal class TriplesObjectTypeRegistry
    {
        private readonly Dictionary<Type, TriplesTypeDesc> _typeDescMap = new();

        private readonly Dictionary<TriplesTypeDesc, Type> _reverseTypeDescMap = new();

        public void Registry(Type type, TriplesTypeDesc typeDesc)
        {
            _typeDescMap.Add(type, typeDesc);
            _reverseTypeDescMap.Add(typeDesc, type);
        }

        public bool TryGetTypeDesc(Type type, [MaybeNullWhen(false)] out TriplesTypeDesc typeDesc)
        {
            return _typeDescMap.TryGetValue(type, out typeDesc);
        }

        public bool TryGetType(TriplesTypeDesc typeDesc, [MaybeNullWhen(false)] out Type type)
        {
            return _reverseTypeDescMap.TryGetValue(typeDesc, out type);
        }
    }
}