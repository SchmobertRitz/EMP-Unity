using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectInjectionTest : MonoBehaviour {

	void Start () {
        Wire wire = new Wire();
        wire.RegisterModule(this);
        for(int i=0; i<50; i++)
        {
            wire.Get<GameObject>("Electron");
        }
    }

    [Provides(typeof(GameObject), "Proton")]
    [Singleton]
    [GameObject("The Proton")]
    private UnityEngine.Object ProvideProton()
    {
        return Resources.Load("Proton");
    }


    [Provides(typeof(GameObject), "Electron")]
    [GameObject("An Electron")]
    private UnityEngine.Object ProvideElectron()
    {
        return Resources.Load("Electron");
    }

}
