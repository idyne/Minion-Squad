using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class Inactive : State
    {
        public Inactive(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.RETURN_ANIMATING;
        }

        public override void OnEnter()
        {

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