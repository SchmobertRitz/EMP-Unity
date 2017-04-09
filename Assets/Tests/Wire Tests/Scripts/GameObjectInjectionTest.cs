using EMP.Wire;
using UnityEngine;

public class GameObjectInjectionTest : MonoBehaviour {

	void Start () {
        float time = Time.realtimeSinceStartup;
        Wire wire = new Wire();
        wire.RegisterModule(this);
        for(int i=0; i<100; i++)
        {
            wire.Get<GameObject>("Electron");
        }
        Debug.Log((Time.realtimeSinceStartup - time) + " seconds took");
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
        return Resources.Load("Electron Pivot");
    }

}
