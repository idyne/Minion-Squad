using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class CarryingPolice : State
    {
        public CarryingPolice(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.BEING_RESCUED ||
                minion.State == Minion.MinionState.GOING_TO_RESCUE;
        }
        public override void OnEnter()
        {
            minion.Agent.enabled = false;
            minion.Transform.parent = minion.CurrentPoliceSlot.SlotTransform;
            minion.Transform.SetPositionAndRotation(minion.CurrentPoliceSlot.SlotTransform.position, minion.CurrentPoliceSlot.SlotTransform.rotation);
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