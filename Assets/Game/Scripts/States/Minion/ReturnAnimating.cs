using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class ReturnAnimating : State
    {
        public ReturnAnimating(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.RETURNING;
        }

        public override void OnEnter()
        {
            minion.GetOnTruck();
        }

        public override void OnExit()
        {
        }


        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
        }
    }
}