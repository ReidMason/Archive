using System.Collections.Generic;
using System;
using System.Linq;

namespace NoxCore.Utilities
{
	public static class DotNETExtensionMethods
	{
		/* string extensions */
		
		public static string Truncate( this string value, int maxLength )
		{
			if (string.IsNullOrEmpty(value)) { return value; }
			
			return value.Substring(0, Math.Min(value.Length, maxLength));
		}

		public static string CapitaliseFirstLetterOnly( this string value )
		{
			if (string.IsNullOrEmpty(value)) { return value; }

			return value[0].ToString().ToUpper() + value.ToLower().Substring(1);
		}

        public static void Resize<T>(this List<T> list, int size, T element = default(T))
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)   // Optimization
                    list.Capacity = size;

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }

        public static void Resize<T>(this List<T> list, int size) where T : new()
        {
            Resize(list, size, new T());
        }

        public static object GetDefaultValue(this Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
                return Activator.CreateInstance(t);
         
            return null;
        }
    }
}