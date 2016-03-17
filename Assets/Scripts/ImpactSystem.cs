using UnityEngine;
using System.Collections;

public class ImpactSystem : MonoBehaviour
{

    public static ImpactSystem Current;

    public GameObject Explode;
    public GameObject Hit;
    public GameObject Disconnect;

	void Awake ()
	{
	    Current = this;
	}

    public GameObject MakeImpact(GameObject impact, Vector2 position, float force, Color color)
    {
        var system = (GameObject) Instantiate(impact, position, Quaternion.identity);

        var particles = system.GetComponentInChildren<ParticleSystem>();

        //set color
        particles.startColor = color;

        Destroy(system, 3f);

        return system;
    }
}
