using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.PoliceState
{
    public class BeingCarried : State
    {
        public BeingCarried(Police police, string name) : base(police, name)
        {
        }
        public override bool CanEnter()
        {
            return police.State == Police.PoliceState.CARRYING_MINION;
        }

        public override void OnEnter()
        {
            if (!police.InTruck)
            {
                police.Agent.speed = police.CarryingSpeed;
                police.Agent.enabled = true;
                police.Agent.SetDestination(Minion.Truck.Transform.position);
            }
            else
            {
                ReachToTruck();
            }
        }

        private void ReachToTruck()
        {
            for (int i = 0; i < police.Slots.Length; i++)
            {
                Slot<Police, Minion> slot = police.Slots[i];
                Minion minion = slot.OccupiedBy;
                minion.CurrentPoliceSlot.Abandon();
                minion.Transform.parent = null;
                minion.CurrentPoliceSlot = null;
                minion.ChangeState(Minion.MinionState.RETURN_ANIMATING);
            }
            police.ChangeState(Police.PoliceState.GETTING_IN_TRUCK);
        }

        public override void OnExit()
        {
            police.Agent.speed = police.NormalSpeed;
        }


        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Truck"))
                ReachToTruck();
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
            police.Agent.SetDestination(Minion.Truck.Transform.position);
        }
    }
}