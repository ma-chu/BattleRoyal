using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EF.Tools
{
	public static class ExtensionMethods
	{
		public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);	
		// Чтобы не писать String.IsNullOrEmpty(localized), а писать  localized.IsNullOrEmpty()

		/*public static bool IsNullOrEmpty<T>(this T[] array)
		{
			return array == null || array.Length <= 0;
		}

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
		{
			return enumerable == null || !enumerable.Any();
		}*/
		
		public static T LastValue<T>(this List<T> list)
		{
			return list.Count > 0 ? list[list.Count - 1] : default(T);
		}
		
		/*public static string UppercaseFirst(this string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return string.Empty;
			}

			var a = str.ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}

		public static string UppercaseFirst2(this string str)
		{
			return str.First().ToString().ToUpper() + str.Substring(1);
		}
		
		public static T[,] ResizeArray<T>(this T[,] original, int rows, int cols)
		{
			var newArray = new T[rows, cols];
			int minRows = Math.Min(rows, original.GetLength(0));
			int minCols = Math.Min(cols, original.GetLength(1));
			for (int i = 0; i < minRows; i++)
			for (int j = 0; j < minCols; j++)
				newArray[i, j] = original[i, j];
			return newArray;
		}

		public static T[,] ResetArray<T>(this T[,] original)
		{
			return new T[original.GetLength(0), original.GetLength(1)];
		}

		public static (int width, int height) GetLength<T>(this T[,] array)
		{
			return (array.GetLength(0), array.GetLength(1));
		}
		
		public static T RandomElement<T>(this T[] items)
		{
			return items[Random.Range(0, items.Length)];
		}

		public static T RandomElement<T>(this List<T> items)
		{
			return items[Random.Range(0, items.Count)];
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (T element in source) action(element);
		}

		public static bool IsNull(this object @object)
		{
			return @object == null;
		}

		public static Vector3 ClampRect(this Vector3 vec, Rect rect)
		{
			if (vec.x < rect.xMin) vec.x = rect.xMin;
			else if (vec.x > rect.xMax) vec.x = rect.xMax;
			if (vec.y < rect.yMin) vec.y = rect.yMin;
			else if (vec.y > rect.yMax) vec.y = rect.yMax;

			return vec;
		}

		public static byte Clamp(this byte value, byte min, byte max)
		{
			if (value < min)
				value = min;
			else if (value > max)
				value = max;
			return value;
		}

		public static int Clamp(this int value, int min, int max)
		{
			if (value < min)
				value = min;
			else if (value > max)
				value = max;
			return value;
		}

		public static Vector2 Round(this Vector2 vec)
		{
			return new Vector2(Mathf.Round(vec.x), Mathf.Round(vec.y));
		}

		public static Vector2 Ceil(this Vector2 vec)
		{
			return new Vector2(Mathf.Ceil(vec.x), Mathf.Ceil(vec.y));
		}

		public static Vector2 Divide(this Vector2 vec, float value)
		{
			return vec / value;
		}

		public static Vector2 Multiply(this Vector2 vec, float value)
		{
			return vec * value;
		}

		public static Color HtmlToColor(this Color color, string htmlColor)
		{
			ColorUtility.TryParseHtmlString(htmlColor, out color);
			return color;
		}

		public static T1 MaxObject<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
			where T2 : IComparable<T2>
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			var first = true;
			var maxObj = default(T1);
			var maxKey = default(T2);
			foreach (var item in source)
			{
				if (first)
				{
					maxObj = item;
					maxKey = selector(maxObj);
					first = false;
				}
				else
				{
					var currentKey = selector(item);
					if (currentKey.CompareTo(maxKey) <= 0) continue;
					maxKey = currentKey;
					maxObj = item;
				}
			}

			if (first) throw new InvalidOperationException("Sequence is empty.");
			return maxObj;
		}

		public static T[] Combine<T>(this T[] array0, T[] array1)
		{
			var result = new T[array0.Length + array1.Length];
			Array.Copy(array0, result, array0.Length);
			Array.Copy(array1, 0, result, array0.Length, array1.Length);
			return result;
		}

		public static int IndexOf<T>(this T[] array, T element)
		{
			return Array.IndexOf(array, element);
		}

		public static Vector2 With(this Vector2 origin, float? x = null, float? y = null)
		{
			if (x.HasValue) origin.x = x.Value;
			if (y.HasValue) origin.y = y.Value;
			return origin;
		}

		public static bool IsNaN(this Vector2 origin)
		{
			return float.IsNaN(origin.x) || float.IsNaN(origin.y);
		}

		public static string ToUnderscoreCase(this string str)
		{
			return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()))
				.ToLower();
		}

		public static T GetRandom<T>(this List<T> list)
		{
			return list[Random.Range(0, list.Count)];
		}
		
		public static List<T> GetRandom<T>(this List<T> list, int count)
		{
			if (count == list.Count) return list;
			if (count > list.Count) throw new ArgumentOutOfRangeException("count must be <= list.count");
			
			var result = new List<T>();
			var max = list.Count;
			while (result.Count < count)
			{
				var item = list[Random.Range(0, max)];
				if(!result.Contains(item))
					result.Add(item);
			}
			
			return result;
		}
		
		public static void Swap<T>(this List<T> list, T item0, T item1)
		{
			var i0 = list.IndexOf(item0);
			var i1 = list.IndexOf(item1);

			list[i0] = item1;
			list[i1] = item0;
		}

		public static List<string> GetBoolConstants(this System.Type type, bool value = true)
		{
			return type.GetFields(BindingFlags.Public |
			                      BindingFlags.Static |
			                      BindingFlags.FlattenHierarchy)
			           .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
			           .Where(fi => (bool)type.GetField(fi.Name).GetValue(null) == value)
			           .Select(fi => fi.Name)
			           .ToList();
		}
		
		public static RectTransform PlaceInRect(this RectTransform rect,
			RectTransform targetRect,
			bool moveBack = true)
		{
			var prevParent = rect.parent;

			rect.SetParent(targetRect);

			// rect.anchorMin = Vector2.zero;
			// rect.anchorMax = Vector2.one;
			// rect.offsetMin = Vector2.zero;
			// rect.offsetMax = Vector2.zero;

			if (moveBack) rect.SetParent(prevParent);

			return rect;
		}*/
	}
}




















