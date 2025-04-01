using System;
using System.Collections.Generic;
using UnityEngine;

namespace MantisLOD
{
    internal class Class0 : IComparable
	{
		public bool bool_0;

		public int int_0;

		public Class1 class1_0;

		public int int_1;

		public Class2 class2_0;

		public Class0 class0_0;

		public float float_0;

		public Class0()
		{
			bool_0 = true;
		}

		public int CompareTo(object object_0)
		{
			return float_0.CompareTo((object_0 as Class0).float_0);
		}
	}

	internal class Class1
	{
		public bool bool_0;

		public bool bool_1;

		public bool bool_2;

		public Vector3 vector3_0 = default(Vector3);

		public List<Class0> list_0 = new List<Class0>();

		public Class1()
		{
			bool_0 = true;
			bool_1 = false;
			bool_2 = false;
		}
	}

	internal class Class2
	{
		public bool bool_0;

		public int int_0;

		public Class0 class0_0;

		public Vector3 vector3_0 = default(Vector3);

		public Class2()
		{
			bool_0 = true;
		}
	}

	internal class Class3
	{
		public Class0 class0_0;

		public int int_0;

		public int int_1;
	}

	internal class Class4
	{
		public bool bool_0;

		public Class1 class1_0;

		public Class1 class1_1;

		public List<Class2> list_0 = new List<Class2>();

		public List<Class3> list_1 = new List<Class3>();

		public Class4()
		{
			bool_0 = true;
		}
	}

	internal abstract class Class5
	{
		private const int int_0 = 1;

		private readonly List<Class0> list_0;

		public Class5()
		{
			list_0 = new List<Class0>();
			list_0.Add(new Class0());
		}

		public Class5(int int_1)
		{
			list_0 = new List<Class0>(int_1);
			list_0.Add(new Class0());
		}

		public void method_0(Class0 class0_0)
		{
			list_0.Add(class0_0);
			class0_0.int_0 = method_5();
			method_6(method_5());
		}

		public Class0 method_1()
		{
			if (method_5() == 0)
			{
				return null;
			}
			Class0 result = list_0[1];
			list_0[1].int_0 = -1;
			list_0[1] = list_0[method_5()];
			list_0[1].int_0 = 1;
			method_7(1);
			list_0.RemoveAt(method_5());
			return result;
		}

		public bool method_2(int int_1)
		{
			if (method_5() == 0)
			{
				return false;
			}
			list_0[int_1].int_0 = -1;
			list_0[int_1] = list_0[method_5()];
			list_0[int_1].int_0 = int_1;
			method_7(int_1);
			list_0.RemoveAt(method_5());
			return true;
		}

		public int method_3()
		{
			return list_0.Count - 1;
		}

		public Class0 method_4()
		{
			if (method_5() == 0)
			{
				return null;
			}
			return list_0[1];
		}

		private int method_5()
		{
			return list_0.Count - 1;
		}

		protected abstract bool vmethod_0(Class0 class0_0, Class0 class0_1);

		private void method_6(int int_1)
		{
			int num = int_1;
			int num2 = int_1 / 2;
			Class0 @class = list_0[num];
			while (num > 1 && vmethod_0(list_0[num2], @class))
			{
				list_0[num] = list_0[num2];
				list_0[num].int_0 = num;
				num = num2;
				num2 /= 2;
			}
			list_0[num] = @class;
			list_0[num].int_0 = num;
		}

		private void method_7(int int_1)
		{
			int index = int_1;
			int num = int_1 * 2;
			Class0 @class = list_0[index];
			while (num <= method_5())
			{
				if (num < method_5() && vmethod_0(list_0[num], list_0[num + 1]))
				{
					num++;
				}
				if (!vmethod_0(@class, list_0[num]))
				{
					break;
				}
				list_0[index] = list_0[num];
				list_0[index].int_0 = index;
				index = num;
				num *= 2;
			}
			list_0[index] = @class;
			list_0[index].int_0 = index;
		}
	}

	internal class Class6 : Class5
	{
		public Class6()
		{
		}

		public Class6(int int_1)
			: base(int_1)
		{
		}

		protected override bool vmethod_0(Class0 class0_0, Class0 class0_1)
		{
			return class0_1.CompareTo(class0_0) < 0;
		}
	}

	internal class Class7 : IEqualityComparer<Vector3>
	{
		public bool Equals(Vector3 vec1, Vector3 vec2)
		{
			if (vec1.x + 1E-07f >= vec2.x && vec1.x <= vec2.x + 1E-07f && vec1.y + 1E-07f >= vec2.y && vec1.y <= vec2.y + 1E-07f && vec1.z + 1E-07f >= vec2.z)
			{
				return vec1.z <= vec2.z + 1E-07f;
			}
			return false;
		}

		public int GetHashCode(Vector3 vector3_0)
		{
			return vector3_0.x.GetHashCode() ^ vector3_0.y.GetHashCode() ^ vector3_0.z.GetHashCode();
		}
	}

	internal class Class8
	{
		private readonly List<Class1> list_0 = new List<Class1>();

		private readonly List<Class2> list_1 = new List<Class2>();

		private readonly List<Class0> list_2 = new List<Class0>();

		private readonly List<Class4> list_3 = new List<Class4>();

		private readonly List<Vector3> list_4 = new List<Vector3>();

		private readonly List<Vector4> list_5 = new List<Vector4>();

		private readonly List<Vector2> list_6 = new List<Vector2>();

		private int int_0;

		private readonly Class6 class6_0 = new Class6();

		private Vector3 vector3_0 = default(Vector3);

		private Vector3 vector3_1 = default(Vector3);

		private float float_0;

		private int int_1;

		private int int_2;

		private bool bool_0;

		private bool bool_1;

		private bool bool_2;

		private bool bool_3;

		private bool bool_4;

		public Class8()
		{
			bool_0 = true;
			bool_1 = false;
			bool_2 = false;
			bool_3 = false;
			bool_4 = false;
			int_1 = 0;
		}

		public int method_0()
		{
			return list_3.Count;
		}

		public void method_1(Vector3[] vector3_2, int int_3, int[] int_4, int int_5, Vector3[] vector3_3, int int_6, Color[] color_0, int int_7, Vector2[] vector2_0, int int_8, int int_9, int int_10, int int_11, int int_12, int int_13)
		{
			if (list_3.Count == 0)
			{
				bool_0 = (int_9 == 1);
				bool_1 = (int_10 == 1);
				bool_2 = (int_11 == 1);
				bool_3 = (int_12 == 1);
				bool_4 = (int_13 == 1);
				method_3(vector3_2, int_3, int_4, int_5, vector3_3, int_6, color_0, int_7, vector2_0, int_8);
				method_11();
				method_13();
				method_14(0);
			}
		}

		public void method_2(int int_3, int[] int_4, ref int int_5)
		{
			if (list_3.Count == 0)
			{
				int_5 = 0;
				return;
			}
			int_3 = Math.Max(Math.Min(int_3, list_3.Count), 0);
			int_3 = method_14(int_3);
			int num = list_3.Count;
			List<List<int>> list = new List<List<int>>(int_2);
			for (int i = 0; i < int_2; i++)
			{
				list.Add(new List<int>());
			}
			int num2 = list_3.Count - 1;
			while (num2 >= 0 && num != int_3)
			{
				foreach (Class2 item in list_3[num2].list_0)
				{
					list[item.class0_0.class2_0.int_0].Add(item.class0_0.int_1);
					list[item.class0_0.class0_0.class2_0.int_0].Add(item.class0_0.class0_0.int_1);
					list[item.class0_0.class0_0.class0_0.class2_0.int_0].Add(item.class0_0.class0_0.class0_0.int_1);
				}
				num--;
				num2--;
			}
			num = 0;
			for (int j = 0; j < int_2; j++)
			{
				int num3 = int_4[num] = list[j].Count;
				num++;
				if (num3 > 0)
				{
					list[j].CopyTo(int_4, num);
					num += num3;
				}
			}
			int_5 = num;
		}

		private void method_3(Vector3[] vector3_2, int int_3, int[] int_4, int int_5, Vector3[] vector3_3, int int_6, Color[] color_0, int int_7, Vector2[] vector2_0, int int_8)
		{
			Dictionary<Vector3, int> dictionary = new Dictionary<Vector3, int>(new Class7());
			List<int> list = new List<int>();
			for (int i = 0; i < int_3; i++)
			{
				if (!dictionary.ContainsKey(vector3_2[i]))
				{
					Class1 @class = new Class1();
					@class.vector3_0 = vector3_2[i];
					Class1 class2 = @class;
					if (class2.vector3_0.x > vector3_0.x)
					{
						vector3_0.x = class2.vector3_0.x;
					}
					if (class2.vector3_0.y > vector3_0.y)
					{
						vector3_0.y = class2.vector3_0.y;
					}
					if (class2.vector3_0.z > vector3_0.z)
					{
						vector3_0.z = class2.vector3_0.z;
					}
					if (class2.vector3_0.x < vector3_1.x)
					{
						vector3_1.x = class2.vector3_0.x;
					}
					if (class2.vector3_0.y < vector3_1.y)
					{
						vector3_1.y = class2.vector3_0.y;
					}
					if (class2.vector3_0.z < vector3_1.z)
					{
						vector3_1.z = class2.vector3_0.z;
					}
					int count = list_0.Count;
					dictionary.Add(vector3_2[i], count);
					list.Add(count);
					list_0.Add(class2);
				}
				else
				{
					list.Add(dictionary[vector3_2[i]]);
				}
			}
			int num = 0;
			int num2 = 0;
			while (num2 < int_5)
			{
				int num3 = int_4[num2];
				num2++;
				for (int j = 0; j < num3; j += 3)
				{
					int num4 = list[int_4[num2 + j]];
					int num5 = list[int_4[num2 + j + 1]];
					int num6 = list[int_4[num2 + j + 2]];
					if (num4 != num5 && num5 != num6 && num6 != num4)
					{
						Class2 class3 = new Class2();
						Class0[] array = new Class0[3]
						{
						new Class0(),
						new Class0(),
						new Class0()
						};
						for (int k = 0; k < 3; k++)
						{
							array[k].class0_0 = array[(k + 1) % 3];
							array[k].class2_0 = class3;
							int index = list[int_4[num2 + j + k]];
							array[k].class1_0 = list_0[index];
							array[k].int_1 = int_4[num2 + j + k];
							list_0[index].list_0.Add(array[k]);
							list_2.Add(array[k]);
						}
						class3.class0_0 = array[0];
						class3.int_0 = num;
						list_1.Add(class3);
					}
				}
				num2 += num3;
				num++;
			}
			int_2 = num;
			for (int l = 0; l < int_6; l++)
			{
				Vector3 item = vector3_3[l];
				list_4.Add(item);
			}
			for (int m = 0; m < int_7; m++)
			{
				Vector4 item2 = color_0[m];
				list_5.Add(item2);
			}
			for (int n = 0; n < int_8; n++)
			{
				Vector2 item3 = vector2_0[n];
				list_6.Add(item3);
			}
			float_0 = (vector3_0 - vector3_1).sqrMagnitude;
			int_1 = list_1.Count;
		}

		private void method_4(Class2 class2_0)
		{
			class2_0.vector3_0 = Vector3.Cross(class2_0.class0_0.class0_0.class1_0.vector3_0 - class2_0.class0_0.class1_0.vector3_0, class2_0.class0_0.class0_0.class0_0.class1_0.vector3_0 - class2_0.class0_0.class1_0.vector3_0);
			class2_0.vector3_0.Normalize();
		}

		private void method_5()
		{
			int num = 0;
			foreach (Class2 item in list_1)
			{
				method_4(item);
				num++;
			}
		}

		private bool method_6(Class0 class0_0)
		{
			Class1 class1_ = class0_0.class1_0;
			Class1 class1_2 = class0_0.class0_0.class1_0;
			int num = 0;
			foreach (Class0 item in class1_.list_0)
			{
				foreach (Class0 item2 in class1_2.list_0)
				{
					if (item.class2_0 == item2.class2_0)
					{
						num++;
						break;
					}
				}
			}
			return num == 1;
		}

		private void method_7()
		{
			int num = 0;
			foreach (Class0 item in list_2)
			{
				if (method_6(item))
				{
					item.class1_0.bool_1 = true;
					item.class0_0.class1_0.bool_1 = true;
					num++;
				}
			}
		}

		private bool method_8(Class0 class0_0)
		{
			Class1 class1_ = class0_0.class1_0;
			Class1 class1_2 = class0_0.class0_0.class1_0;
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			foreach (Class0 item in class1_.list_0)
			{
				if (item.class0_0.class1_0 == class1_2)
				{
					list.Add(item.class0_0.class0_0.int_1);
				}
			}
			foreach (Class0 item2 in class1_2.list_0)
			{
				if (item2.class0_0.class1_0 == class1_)
				{
					list2.Add(item2.class0_0.class0_0.int_1);
				}
			}
			if (list.Count != list2.Count)
			{
				return false;
			}
			bool flag = false;
			foreach (int item3 in list)
			{
				if (!flag)
				{
					bool flag2 = false;
					foreach (int item4 in list2)
					{
						if (item3 != item4 && list_6.Count > 0 && list_6[item3] == list_6[item4])
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						flag = true;
					}
				}
			}
			return !flag;
		}

		private void method_9()
		{
			int num = 0;
			foreach (Class0 item in list_2)
			{
				if (method_8(item))
				{
					item.class1_0.bool_2 = true;
					item.class0_0.class1_0.bool_2 = true;
					num++;
				}
			}
		}

		private float method_10(Class0 class0_0)
		{
			Class1 class1_ = class0_0.class1_0;
			Class1 class1_2 = class0_0.class0_0.class1_0;
			float sqrMagnitude = (class1_2.vector3_0 - class1_.vector3_0).sqrMagnitude;
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			List<Class2> list3 = new List<Class2>();
			foreach (Class0 item in class1_.list_0)
			{
				foreach (Class0 item2 in class1_2.list_0)
				{
					if (item.class2_0 == item2.class2_0)
					{
						list.Add(item.int_1);
						list2.Add(item.class2_0.int_0);
						list3.Add(item.class2_0);
						break;
					}
				}
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			float num = float.MinValue;
			float num2 = 0f;
			foreach (Class0 item3 in class1_.list_0)
			{
				bool flag5 = false;
				float num3 = float.MaxValue;
				foreach (Class2 item4 in list3)
				{
					float num4 = (1f - Vector3.Dot(item3.class2_0.vector3_0, item4.vector3_0)) * 0.5f;
					if (num4 < num3)
					{
						num3 = num4;
					}
					if (item3.class2_0 == item4)
					{
						flag5 = true;
					}
				}
				if (num3 > num)
				{
					num = num3;
				}
				if (!flag5)
				{
					if (bool_4)
					{
						Class1 @class = class1_2;
						Class1 class1_3 = item3.class0_0.class1_0;
						Class1 class1_4 = item3.class0_0.class0_0.class1_0;
						Vector3 normalized = (class1_3.vector3_0 - @class.vector3_0).normalized;
						Vector3 normalized2 = (class1_4.vector3_0 - class1_3.vector3_0).normalized;
						Vector3 normalized3 = (@class.vector3_0 - class1_4.vector3_0).normalized;
						float val = Vector3.Dot(normalized3, normalized);
						float val2 = Vector3.Dot(normalized, normalized2);
						float val3 = Vector3.Dot(normalized2, normalized3);
						float num5 = Math.Min(val, Math.Min(val2, val3));
						float num6 = Math.Max(val, Math.Max(val2, val3));
						float num7 = (num6 - num5) * 0.5f;
						if (num7 > num2)
						{
							num2 = num7;
						}
					}
					if (bool_3 && !flag)
					{
						bool flag6 = false;
						foreach (int item5 in list)
						{
							if (list_4.Count == 0 || list_4[item3.int_1] == list_4[item5])
							{
								flag6 = true;
								break;
							}
						}
						if (!flag6)
						{
							flag = true;
						}
					}
					if (!flag2)
					{
						bool flag7 = false;
						foreach (int item6 in list)
						{
							if (list_5.Count == 0 || list_5[item3.int_1] == list_5[item6])
							{
								flag7 = true;
								break;
							}
						}
						if (!flag7)
						{
							flag2 = true;
						}
					}
					if (!flag3)
					{
						bool flag8 = false;
						foreach (int item7 in list)
						{
							if (list_6.Count == 0 || list_6[item3.int_1] == list_6[item7])
							{
								flag8 = true;
								break;
							}
						}
						if (!flag8)
						{
							flag3 = true;
						}
					}
					if (!flag4)
					{
						bool flag9 = false;
						foreach (int item8 in list2)
						{
							if (item3.class2_0.int_0 == item8)
							{
								flag9 = true;
								break;
							}
						}
						if (!flag9)
						{
							flag4 = true;
						}
					}
				}
			}
			float num8 = flag ? float_0 : 0f;
			float num9 = flag2 ? float_0 : 0f;
			float num10 = flag3 ? float_0 : 0f;
			float num11 = flag4 ? float_0 : 0f;
			float num12 = 0f;
			if (bool_2)
			{
				num12 = ((!class1_.bool_2 || class1_2.bool_2) ? 0f : float_0);
			}
			float num13 = 0f;
			float num14 = 0f;
			if (bool_0)
			{
				if (class1_.bool_1 || class1_2.bool_1)
				{
					num13 = float_0;
				}
			}
			else if (class1_.bool_1)
			{
				if (class1_2.bool_1)
				{
					foreach (Class0 item9 in class1_.list_0)
					{
						if (method_6(item9.class0_0.class0_0))
						{
							Vector3 b = item9.class0_0.class0_0.class1_0.vector3_0;
							Vector3 vector = class1_.vector3_0;
							Vector3 a = class1_2.vector3_0;
							float num15 = (1f - Vector3.Dot((vector - b).normalized, (a - vector).normalized)) * 0.5f;
							if (num15 > num14)
							{
								num14 = num15;
							}
						}
					}
				}
				else
				{
					num13 = float_0;
				}
			}
			if (bool_1)
			{
				num *= num;
			}
			if (num < 1E-06f)
			{
				num = 1E-06f;
			}
			return (float)((double)sqrMagnitude * (((double)num * 20.0 + (double)num14 * 20.0 + (double)num2 * 1.0) / 41.0) + (double)num13 + (double)num8 + (double)num9 + (double)num10 + (double)num12 + (double)num11);
		}

		private void method_11()
		{
			method_5();
			method_7();
			method_9();
			foreach (Class0 item in list_2)
			{
				item.float_0 = method_10(item);
				class6_0.method_0(item);
			}
		}

		private bool method_12(Class0 class0_0)
		{
			Class1 class1_ = class0_0.class1_0;
			Class1 class1_2 = class0_0.class0_0.class1_0;
			List<KeyValuePair<int, int>> list = new List<KeyValuePair<int, int>>();
			List<Class2> list2 = new List<Class2>();
			foreach (Class0 item2 in class1_.list_0)
			{
				foreach (Class0 item3 in class1_2.list_0)
				{
					if (item2.class2_0 == item3.class2_0)
					{
						if (item2.class0_0.class1_0 == class1_2)
						{
							list.Add(new KeyValuePair<int, int>(item2.int_1, item2.class0_0.int_1));
						}
						else if (item2.class0_0.class0_0.class1_0 == class1_2)
						{
							list.Add(new KeyValuePair<int, int>(item2.int_1, item2.class0_0.class0_0.int_1));
						}
						list2.Add(item2.class2_0);
						break;
					}
				}
			}
			Class4 @class = new Class4();
			List<Class0> list3 = new List<Class0>();
			foreach (Class0 item4 in class1_.list_0)
			{
				bool flag = false;
				foreach (Class2 item5 in list2)
				{
					if (item4.class2_0 == item5)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					item4.class1_0 = class1_2;
					int value = list[list.Count - 1].Value;
					foreach (KeyValuePair<int, int> item6 in list)
					{
						if (list_6.Count == 0 || list_6[item4.int_1] == list_6[item6.Key])
						{
							value = item6.Value;
							break;
						}
					}
					Class3 class2 = new Class3();
					class2.class0_0 = item4;
					class2.int_0 = item4.int_1;
					class2.int_1 = value;
					Class3 item = class2;
					@class.list_1.Add(item);
					item4.int_1 = value;
					list3.Add(item4);
				}
			}
			foreach (Class0 item7 in class1_2.list_0)
			{
				bool flag2 = false;
				foreach (Class2 item8 in list2)
				{
					if (item7.class2_0 == item8)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					list3.Add(item7);
				}
			}
			class1_2.list_0 = list3;
			foreach (Class0 item9 in class1_2.list_0)
			{
				item9.float_0 = method_10(item9);
				item9.class0_0.float_0 = method_10(item9.class0_0);
				item9.class0_0.class0_0.float_0 = method_10(item9.class0_0.class0_0);
				class6_0.method_2(item9.int_0);
				class6_0.method_0(item9);
				class6_0.method_2(item9.class0_0.int_0);
				class6_0.method_0(item9.class0_0);
				class6_0.method_2(item9.class0_0.class0_0.int_0);
				class6_0.method_0(item9.class0_0.class0_0);
				method_4(item9.class2_0);
			}
			foreach (Class2 item10 in list2)
			{
				item10.bool_0 = false;
				int_1--;
				@class.list_0.Add(item10);
				item10.class0_0.bool_0 = false;
				item10.class0_0.class0_0.bool_0 = false;
				item10.class0_0.class0_0.class0_0.bool_0 = false;
				class6_0.method_2(item10.class0_0.int_0);
				class6_0.method_2(item10.class0_0.class0_0.int_0);
				class6_0.method_2(item10.class0_0.class0_0.class0_0.int_0);
				Class0 class0_ = item10.class0_0;
				Class0 class3 = class0_;
				do
				{
					if (class3.class1_0 != class1_ && class3.class1_0 != class1_2)
					{
						class3.class1_0.list_0.Remove(class3);
						break;
					}
					class3 = class3.class0_0;
				}
				while (class3 != class0_);
			}
			class1_.bool_0 = false;
			@class.class1_0 = class1_;
			@class.class1_1 = class1_2;
			if (class0_0.float_0 >= float_0)
			{
				@class.bool_0 = false;
			}
			list_3.Add(@class);
			return true;
		}

		private void method_13()
		{
			int num = int_1;
			while (int_1 > 0 && class6_0.method_4() != null)
			{
				method_12(class6_0.method_4());
				if (num > int_1 + 2500)
				{
					num = int_1;
				}
			}
			int_0 = list_3.Count;
		}

		private int method_14(int int_3)
		{
			while (int_0 != int_3)
			{
				if (int_0 > int_3)
				{
					int_0--;
					foreach (Class3 item in list_3[int_0].list_1)
					{
						item.class0_0.class1_0 = list_3[int_0].class1_0;
						item.class0_0.int_1 = item.int_0;
					}
					continue;
				}
				if (!list_3[int_0].bool_0)
				{
					break;
				}
				foreach (Class3 item2 in list_3[int_0].list_1)
				{
					item2.class0_0.class1_0 = list_3[int_0].class1_1;
					item2.class0_0.int_1 = item2.int_1;
				}
				int_0++;
			}
			return int_0;
		}
	}

	public static class MantisLODSimpler
	{
		private static readonly List<Class8> list_0 = new List<Class8>();

		public static int get_triangle_list(int index, float goal, int[] triangle_array, ref int triangle_count)
		{
			if (index >= 0 && index < list_0.Count && list_0[index] != null)
			{
				Class8 @class = list_0[index];
				int int_ = (int)((float)@class.method_0() * (1f - goal * 0.01f) + 0.5f);
				@class.method_2(int_, triangle_array, ref triangle_count);
				return 1;
			}
			return 0;
		}

		public static int create_progressive_mesh(Vector3[] vertex_array, int vertex_count, int[] triangle_array, int triangle_count, Vector3[] normal_array, int normal_count, Color[] color_array, int color_count, Vector2[] uv_array, int uv_count, int protect_boundary, int protect_detail, int protect_symmetry, int protect_normal, int protect_shape)
		{
			bool flag = false;
			int num = -1;
			for (int i = 0; i < list_0.Count; i++)
			{
				if (list_0[i] == null)
				{
					flag = true;
					list_0[i] = new Class8();
					num = i;
					break;
				}
			}
			if (!flag)
			{
				list_0.Add(new Class8());
				num = list_0.Count - 1;
			}
			Class8 @class = list_0[num];
			@class.method_1(vertex_array, vertex_count, triangle_array, triangle_count, normal_array, normal_count, color_array, color_count, uv_array, uv_count, protect_boundary, protect_detail, protect_symmetry, protect_normal, protect_shape);
			return num;
		}

		public static int delete_progressive_mesh(int index)
		{
			if (index >= 0 && index < list_0.Count && list_0[index] != null)
			{
				list_0[index] = null;
				return 1;
			}
			return 0;
		}
	}
}
