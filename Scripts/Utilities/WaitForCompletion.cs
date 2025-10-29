using System;
using UnityEngine;

namespace Polygonia
{
    public class WaitForCompletion : CustomYieldInstruction
    {
        public WaitForCompletion(Func<bool> condition)
        {
            this._condition = condition;
        }

        private readonly Func<bool> _condition;
        
        
        public override bool keepWaiting
        {
            get { return _condition != null && _condition.Invoke() == false; }
        }
    }
}