using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace LayeredInputSystem.Runtime
{
	internal static class YieldCache
	{
		private readonly static Dictionary<float, WaitForSecondsRealtime> _TimeIntervalReal = new Dictionary<float, WaitForSecondsRealtime>(new FloatComparer());
		private readonly static Dictionary<float, WaitForSeconds> _TimeInterval = new Dictionary<float, WaitForSeconds>(new FloatComparer());
		private readonly static List<WaitForCompletion> _Conditional = new List<WaitForCompletion>();

		public readonly static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
		public readonly static WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();


		public static WaitForSeconds WaitForSeconds(float seconds)
		{
			if (_TimeInterval.TryGetValue(seconds, out WaitForSeconds wfs) == false)
			{
				_TimeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
			}

			return wfs;
		}


		public static WaitForSecondsRealtime WaitForSecondsRealTime(float seconds)
		{
			if (_TimeIntervalReal.TryGetValue(seconds, out WaitForSecondsRealtime wfsReal) == false)
			{
				_TimeIntervalReal.Add(seconds, wfsReal = new WaitForSecondsRealtime(seconds));
			}

			return wfsReal;
		}


		public static WaitForCompletion WaitForCompletion(Func<bool> condition)
		{
			WaitForCompletion coroutine = _Conditional.FirstOrDefault(i => i.isCompleted);

			if (coroutine == null)
			{
				coroutine = new WaitForCompletion(condition);
				_Conditional.Add(coroutine);
			}
			else
			{
				coroutine.ResetCondition(condition);
			}

			return coroutine;
		}
	}
}