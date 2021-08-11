using System;
using System.Collections.Generic;

namespace EF.Tools
{
	public static class ExtensionMethods
	{
		public static bool IsNull(this object obj) => obj == null;

		public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);	
		// Чтобы не писать String.IsNullOrEmpty(localized), а писать  localized.IsNullOrEmpty()
		
		public static T LastValue<T>(this List<T> list)
		{
			return list.Count > 0 ? list[list.Count - 1] : default(T);
		}


		public const float PRECISION  = 0.01f;
		public static bool EqualTo(this float a, float b, float precision = PRECISION)
		{
			return (a - b).Abs() < precision;
		}
		
		public static float Abs(this float a)
		{
			return a < 0 ? -a : a;
		}
	}
}




















