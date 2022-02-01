using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class GoingToRescue : State
    {
        public GoingToRescue(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.AVALIABLE;
        }
        public override void OnEnter()
        {
            minion.TargetPoliceSlot.Occupy(minion);
            minion.TargetPoliceSlot.Owner.CurrentSlot.Owner.ChangeState(Minion.MinionState.BEING_RESCUED);
            if (minion.OverlapPolices.Contains(minion.TargetPoliceSlot.Owner))
                ReachTargetPolice();
            else
            {
                minion.Agent.enabled = true;
                minion.Agent.SetDestination(minion.TargetPoliceSlot.Owner.Transform.position);
            }
        }
        public override void OnExit()
        {
        }
        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Police"))
            {
                Police police = other.GetComponent<Police>();
                if (police == minion.TargetPoliceSlot.Owner)
                    ReachTargetPolice();
            }
        }

        private void ReachTargetPolice()
        {
            if (minion.TargetPoliceSlot.Owner.State == Police.PoliceState.CARRYING_MINION)
            {
                //TODO state giriþ þartlarý ayarlanacak
                Minion arrestedMinion = minion.TargetPoliceSlot.Owner.CurrentSlot.Owner;
                Police police = minion.TargetPoliceSlot.Owner;
                minion.CurrentPoliceSlot = minion.TargetPoliceSlot;
                arrestedMinion.CurrentPoliceSlot = police.GetAvaliableSlot();
                arrestedMinion.CurrentPoliceSlot.Occupy(arrestedMinion);
                minion.CurrentPoliceSlot.Reach();
                arrestedMinion.CurrentPoliceSlot.Reach();
                minion.TargetPoliceSlot = null;
                arrestedMinion.TargetPoliceSlot = null;
                police.AbandonCurrentSlot();
                arrestedMinion.ChangeState(Minion.MinionState.CARRYING_POLICE);
                minion.ChangeState(Minion.MinionState.CARRYING_POLICE);
                police.ChangeState(Police.PoliceState.BEING_CARRIED);
            }
            else
                TargetPoliceNotAvaliable();
        }

        private void TargetPoliceNotAvaliable()
        {
            minion.TargetPoliceSlot.Abandon();
            minion.ChangeState(Minion.MinionState.RETURNING);
        }


        public override void OnTriggerExit(Collider other)
        {
        }
        public override void Update()
        {
            if (minion.TargetPoliceSlot.Owner.State == Police.PoliceState.CARRYING_MINION)
                minion.Agent.SetDestination(minion.TargetPoliceSlot.Owner.Transform.position);
            else
            {
                TargetPoliceNotAvaliable();
            }
        }
    }
}