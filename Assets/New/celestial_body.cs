using UnityEngine;
using System.Collections.Generic;

public class celestial_body : MonoBehaviour
{
    /// <summary>
    /// Every gameObject that you want to have gravity should have this script, planets too
    /// use only for gameobjects with Rigidbody
    /// </summary>
    // Actual mass of Earth 5,972E24 while in game ~ 10,000      // This is only for the planet // Different for every gameobject
    public Rigidbody rb;

    // Actual G of Earth 6.67408 × 10-11 m3 kg-1 s-2 while in game ~ 667
    [SerializeField] float G = 667.4f;

    public static List<celestial_body> attractors;      /// List of all objects with this script

    private void FixedUpdate()
    {
        foreach (celestial_body body in attractors)
        {
            if (body != this)
                Attract(body);
        }
    }

    private void OnEnable()                         /// Add itself to the list
    {
        if(attractors == null)
        {
            attractors = new List<celestial_body>();
        }
        attractors.Add(this);
    }
    private void OnDisable()                            /// Remove itself from list
    {
        attractors.Remove(this);
    }

    void Attract(celestial_body objToAttract) /// Attracts all the objects with this script on them
    {
        Rigidbody rbToAttract = objToAttract.rb;

        Vector3 direction = rb.position - rbToAttract.position;
        float distance = direction.magnitude;

        if(distance == 0f)
        {
            return;
        }

        float forceMagnitude = G* (rb.mass * rbToAttract.mass) / Mathf.Pow(distance, 2);
        Vector3 force = direction.normalized * forceMagnitude;

        rbToAttract.AddForce(force);
    }
}
