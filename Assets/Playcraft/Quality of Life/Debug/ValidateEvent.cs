using System;
using UnityEngine;
using UnityEngine.Events;

namespace Playcraft
{
    public class ValidateEvent : MonoBehaviour
    {
        [SerializeField] Binding[] bindings;
    
        void OnValidate() { CheckTrigger(); }
        void Update() { CheckTrigger(); }
        
        void CheckTrigger()
        {
            foreach (var binding in bindings)
                binding.CheckTrigger();
        }

        [Serializable]
        public class Binding
        {
            [SerializeField] bool trigger;
            [SerializeField] UnityEvent response;   
            
            public void CheckTrigger()
            {
                if (!trigger) return;
                response.Invoke();
                trigger = false;
            }
        }
    }
}
