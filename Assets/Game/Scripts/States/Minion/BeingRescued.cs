using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class BeingRescued : State
    {
        public BeingRescued(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.ARRESTED;
        }
        public override void OnEnter()
        {
            if (!minion.InPoliceCar)
            {
                minion.Agent.enabled = true;
                minion.Agent.speed = minion.CarryingSpeed;
                minion.Agent.SetDestination(Police.PoliceCar.Transform.position);
            }
            else
            {
                ReachPoliceCar();
            }
        }

        public override void OnExit()
        {
            minion.Agent.speed = minion.NormalSpeed;
        }
        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Police Car"))
            {
                ReachPoliceCar();
            }

        }
        private void ReleasePolice()
        {
            Police police = minion.Slot.OccupiedBy;
            police.AbandonCurrentSlot();
            police.ChangeState(Police.PoliceState.RETURNING);
            minion.Slot.Clear();

        }

        private void ReachPoliceCar()
        {
            minion.ChangeState(Minion.MinionState.ARREST_ANIMATING);
            ReleasePolice();
        }
        public override void OnTriggerExit(Collider other)
        {
        }
        public override void Update()
        {
            minion.Agent.SetDestination(Police.PoliceCar.Transform.position);
        }
    }
}