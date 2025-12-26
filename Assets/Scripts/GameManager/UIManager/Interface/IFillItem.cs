using UnityEngine;

namespace GameUI
{
	public interface IFillItem
	{
		public Vector2 MinMax { get; set; }
		public Vector2 Value { get; set; }
		/// <summary>
		/// {0} == ValueMax // Value.y
		/// {1} == Max // MinMax.y
		/// {2} == Min // MinMax.x
		/// {4} == ValueMin // Value.x
		/// Ex) "{0}/{1}" == $"{Value.y}/{MinMax.y}"
		/// </summary>
		public string TextFormat { get; set; }
	}

	public interface IFillGroup : IFillItem
	{
		int Count { get; }
		float this[int index] { get;  set; }

		void SetValue(params float[] values);
		void SetValue(params int[] values);
	}
}
