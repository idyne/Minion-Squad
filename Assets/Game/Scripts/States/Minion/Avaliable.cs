using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class Avaliable : State
    {
        public Avaliable(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.ON_AIR;
        }

        public override void OnEnter()
        {
            if (minion.SearchTask(out Minion.TaskType taskType))
            {
                // targetPoliceSlot Search Task sýrasýnda atandý
                if(taskType == Minion.TaskType.RESCUE)
                    minion.ChangeState(Minion.MinionState.GOING_TO_RESCUE);
                // targetSlot Search Task sýrasýnda atandý
                else if (taskType == Minion.TaskType.STUFF)
                    minion.ChangeState(Minion.MinionState.GOING_TO_STUFF);
            }
            else
                minion.ChangeState(Minion.MinionState.RETURNING);
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