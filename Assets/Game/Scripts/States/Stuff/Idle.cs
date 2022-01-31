using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.StuffState
{
    public class Idle : State
    {
        public Idle(Stuff stuff, string name) : base(stuff, name)
        {
        }
        public override bool CanEnter()
        {
            return stuff.State == Stuff.StuffState.MOVING;
        }

        public override void OnEnter()
        {
            for (int i = 0; i < stuff.Slots.Length; i++)
            {
                Slot<Stuff, Minion> slot = stuff.Slots[i];
                Minion minion = slot.OccupiedBy;
                if (minion && minion.State == Minion.MinionState.CARRYING_STUFF)
                    minion.ChangeState(Minion.MinionState.HOLDING_STUFF);
            }
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