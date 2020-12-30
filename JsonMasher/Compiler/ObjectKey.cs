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

        public override bool Equals(object obj)
            => obj switch {
                ObjectKey key => Object.ReferenceEquals(_object, key._object),
                _ => false
            };

        public override int GetHashCode() => RuntimeHelpers.GetHashCode(_object);
    }
}
