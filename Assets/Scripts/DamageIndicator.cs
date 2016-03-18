using UnityEngine;
using System.Collections;

public class DamageIndicator : MonoBehaviour
{
    public float FlingForce;
    public float LifeTime;
    public AnimationCurve SizeOverLife;

    private float _life;

	void Start ()
	{
        GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-20.0f, 20.0f), FlingForce));
        Destroy(gameObject, LifeTime);
	}

    void Update()
    {
        _life += Time.deltaTime;
        var t = _life/LifeTime;

        transform.localScale = Vector3.one * SizeOverLife.Evaluate(t);
    }
}
