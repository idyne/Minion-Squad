using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.StuffState
{
    public class Animating : State
    {
        public Animating(Stuff stuff, string name) : base(stuff, name)
        {
        }
        public override bool CanEnter()
        {
            return stuff.State == Stuff.StuffState.MOVING;
        }

        public override void OnEnter()
        {
            AnimateLoading();
        }

        public override void OnExit()
        {

        }


        public override void OnTriggerEnter(Collider other)
        {

        }

        private void ReleaseMinionsOnSlots()
        {

        }

        private void AnimateLoading()
        {
            //TODO animasyon yapýlacak
            stuff.Transform.LeanScale(Vector3.zero, 0.2f).setOnComplete(() => { stuff.gameObject.SetActive(false); });
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
        }
    }
}