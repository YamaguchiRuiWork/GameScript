using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R3;

namespace GameScript.Scripts
{
    public class TriggerEventHandler : MonoBehaviour
    {
        private readonly Subject<int> _isCombat = new();

        public Observable<int> IsCombatAsObservable() => _isCombat;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }


        private void InCombat(int isCombat)
        {
            _isCombat.OnNext(isCombat);
        }
    }
}