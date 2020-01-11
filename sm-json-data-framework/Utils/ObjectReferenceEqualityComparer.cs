using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace sm_json_data_framework.Utils
{
    public class ObjectReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        private static ObjectReferenceEqualityComparer<T> defaultComparer;

        public static ObjectReferenceEqualityComparer<T> Default
        {
            get
            {
                if (defaultComparer == null)
                {
                    defaultComparer = new ObjectReferenceEqualityComparer<T>();
                }

                return defaultComparer;
            }
        }

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
