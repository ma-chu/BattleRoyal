using System;
using System.Collections.Generic;

namespace EF.Tools
{
	public static class ExtensionMethods
	{
		public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);	
		// Чтобы не писать String.IsNullOrEmpty(localized), а писать  localized.IsNullOrEmpty()
		
		
		public static T LastValue<T>(this List<T> list)
		{
			return list.Count > 0 ? list[list.Count - 1] : default(T);
		}

	}
}




















