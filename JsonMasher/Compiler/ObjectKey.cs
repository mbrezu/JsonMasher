using System;
using System.Runtime.CompilerServices;

namespace JsonMasher.Compiler
{
    public class ObjectKey
    {
        private object _object;

        public ObjectKey(object obj)
        {
            _object = obj;
        }

        public override bool Equals(object obj) => Object.ReferenceEquals(_object, obj);

        public override int GetHashCode() => RuntimeHelpers.GetHashCode(_object);
    }
}
