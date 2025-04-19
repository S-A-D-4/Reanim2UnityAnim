using System.Collections.Generic;
using UnityEngine;

namespace Reanim2UnityAnim
{
	public class UniqueMaterialController : MonoBehaviour
	{
		private static int sortingOrderIncrement;
		public SpriteRenderer[] spriteRenderers;

		private void Awake()
		{
			foreach (SpriteRenderer t in spriteRenderers)
			{
				t.material = new Material(t.material);
				t.sortingOrder += sortingOrderIncrement;
			}
			sortingOrderIncrement += 30;
		}
	}
}