using UnityEngine;
using System.Collections;

public class ImpactSystem : MonoBehaviour
{

    public static ImpactSystem current;

    public GameObject explode;
    public GameObject hit;

	void Awake ()
	{
	    current = this;
	}

    public GameObject MakeImpact(GameObject impact, Vector2 position, float force)
    {
        var system = Instantiate(impact, position, Quaternion.identity) as GameObject;

        //var particles = system.GetComponent<ParticleSystem>();

        Destroy(system, 5.0f);

        return system;
    }
}
