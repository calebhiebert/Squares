using UnityEngine;
using System.Collections;

public class ImpactSystem : MonoBehaviour
{

    public static ImpactSystem Current;

    public GameObject Explode;
    public GameObject Hit;

	void Awake ()
	{
	    Current = this;
	}

    public GameObject MakeImpact(GameObject impact, Vector2 position, float force)
    {
        var system = Instantiate(impact, position, Quaternion.identity) as GameObject;

        //var particles = system.GetComponent<ParticleSystem>();

        Destroy(system, 2f);

        return system;
    }
}
