using UnityEngine;
namespace States.StuffState
{
    public abstract class State
    {
        protected string name;
        protected Stuff stuff;

        public string Name { get => name; }

        public State(Stuff stuff, string name)
        {
            this.stuff = stuff;
            this.name = name;
        }
        public abstract void OnEnter();
        public abstract void Update();
        public abstract void OnExit();
        public abstract void OnTriggerEnter(Collider other);
        public abstract bool CanEnter();

        public abstract void OnTriggerExit(Collider other);
    }
}