using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    private int max, current;

    void Start()
    {
        current = max;
    }

    public int Max
    {
        get { return max; }
        set { max = value; }
    }

    public int Current
    {
        get { return current; }
        set
        {
            current = value;

            if (current <= 0)
                OnDeath();
        }
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }
}
