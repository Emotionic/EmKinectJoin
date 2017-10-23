using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWorksTest : MonoBehaviour
{
    private RainbowColor rc = new RainbowColor(0, 0.01f);

    void InstantiateFireWorks(string path)
    {
        var fireWorks = Instantiate(Resources.Load(path) as GameObject, this.transform);
        fireWorks.GetComponent<FireWorksManager>().StartColor = rc.Rainbow;

        fireWorks.GetComponent<ParticleSystem>().Play(true);

        Destroy(fireWorks, 10);
    }
    
	// Update is called once per frame
	void Update ()
    {
        rc.Update();

        if (Input.GetKeyDown(KeyCode.G))
        {
            InstantiateFireWorks("Prefabs/Fire Works Senrin");
        }

        if(Input.GetKey(KeyCode.F))
        {
            InstantiateFireWorks("Prefabs/Fire Works Botan");
        }
	}
}
