using System;
using UnityEngine;
using System.Collections;

public class HexLoader : MonoBehaviour
{

    public GameObject outerRing;
    public GameObject loadingRing;
    public ParticleSystem FinishParticleSystem;

    public float loadingTime;
    public float rotateSpeed;

    private float _time = 0;
	
	void Update ()
	{
	    _time += Time.deltaTime;

	    float scale = _time/loadingTime;

        loadingRing.transform.localScale = new Vector3(scale, scale, 1);

        outerRing.transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        loadingRing.transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        if (_time >= loadingTime)
        {
        	    
            FinishParticleSystem.Play();
            outerRing.SetActive(false);
            loadingRing.SetActive(false);

            Destroy(gameObject, 1.5f);
            Destroy(this);
        }
    }
}
