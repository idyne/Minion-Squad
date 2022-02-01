using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class HoldingStuff : State
    {
        public HoldingStuff(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.GOING_TO_STUFF || minion.State == Minion.MinionState.CARRYING_STUFF;
        }

        //Target Slot Occupy edildi
        //Current Slot Going to Stuff state'inin çýkýþýnda atandý ve Target Slot null olarak atandý
        public override void OnEnter()
        {
            minion.CurrentSlot.Owner.TakePosition(minion);
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
            if (Time.time > minion.StateTime + 10 && Vector3.Distance(minion.Transform.position, Minion.Truck.Transform.position) > 30)
            {
                minion.AbandonCurrentSlot();
                minion.ChangeState(Minion.MinionState.RETURNING);
            }
        }
    }
}