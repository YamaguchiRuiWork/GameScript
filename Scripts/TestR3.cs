using System;
using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    //private IObservable<Unit> Test = new Subject<Unit>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private Observable<Unit> TestObservable()
    {
        //var a = new Subject<Unit>();
        //a.OnNext();
        return Observable.Return(Unit.Default);
    }
}