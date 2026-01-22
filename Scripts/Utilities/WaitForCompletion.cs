using System;
using UnityEngine;

namespace LayeredInputSystem.Runtime
{
	internal class WaitForCompletion : CustomYieldInstruction
	{
		public WaitForCompletion(Func<bool> condition)
		{
			this._condition = condition;
		}

		private Func<bool> _condition;
		private bool _completed;


		public override bool keepWaiting
		{
			get { return this.IsNotCompleted(); }
		}
		
		public bool isCompleted
		{
			get { return _completed; }
		}


		public void ResetCondition(Func<bool> condition)
		{
			_condition = condition;
			_completed = false;
		}

		
		private bool IsNotCompleted()
		{
			if(_condition != null && _condition.Invoke() == false)
			{
				return true;
			}

			_completed = true;
			return false;
		}
	}
}