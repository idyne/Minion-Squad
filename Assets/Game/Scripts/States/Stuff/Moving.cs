using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.StuffState
{
    public class Moving : State
    {
        public Moving(Stuff stuff, string name) : base(stuff, name)
        {
        }
        public override bool CanEnter()
        {
            return stuff.State == Stuff.StuffState.IDLE;
        }

        public override void OnEnter()
        {
            for (int i = 0; i < stuff.Slots.Length; i++)
            {
                Minion minionInSlot = stuff.Slots[i].OccupiedBy;
                minionInSlot.ChangeState(Minion.MinionState.CARRYING_STUFF);
            }
            if (stuff.InTruck)
                LoadToTruck();
            else
            {
                stuff.Agent.enabled = true;
                stuff.Animator.enabled = true;
                stuff.Agent.SetDestination(Stuff.Truck.Transform.position);
            }
        }

        public override void OnExit()
        {
            stuff.Agent.enabled = false;
            stuff.Animator.enabled = false;
            
        }


        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Truck"))
            {
                LoadToTruck();
            }
        }

        private void ReleaseMinionsOnSlots()
        {
            for (int i = 0; i < stuff.Slots.Length; i++)
            {
                Minion minion = stuff.Slots[i].OccupiedBy;
                minion.AbandonCurrentSlot();
                minion.ChangeState(Minion.MinionState.RETURNING);
                stuff.Slots[i].Clear();
            }
        }

        private void LoadToTruck()
        {
            stuff.ChangeState(Stuff.StuffState.ANIMATING);
            ReleaseMinionsOnSlots();
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
            stuff.Agent.SetDestination(Stuff.Truck.Transform.position);
        }
    }
}