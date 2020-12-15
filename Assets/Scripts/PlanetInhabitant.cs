using System;
using UnityEngine;

public class PlanetInhabitant : MonoBehaviour
{
    public bool keepAligned = true;
    public bool keepAttracted = true;
    public Planet CurrentPlanet;
    private Rigidbody _rigidBody;
    
    private void Awake()
    {
        _rigidBody = FindObjectOfType<Rigidbody>();

        // Automatically assign the nearest Planet
        var planets = (Planet[]) FindObjectsOfType(typeof(Planet));
        float closest = Mathf.Infinity;
        
        foreach(Planet planet in planets)
        {
            Vector3 directionToPlanet = planet.transform.position - transform.position;
            float dSqr = directionToPlanet.sqrMagnitude;
            if(dSqr < closest)
            {
                closest = dSqr;
                CurrentPlanet = planet;
            }
        }

        // We cannot parent the player to a moving transform due to KCC 
        if (gameObject.name == "Player") return;
        transform.parent = CurrentPlanet.transform;
    }

    public void Align()
    {
        if (keepAligned)
        {
            Vector3 gravityUp = CurrentPlanet.GetGravityUpForPosition(_rigidBody.position);
            Vector3 localUp = _rigidBody.transform.up;
            // Align body's up axis with the center of planet
            _rigidBody.rotation = Quaternion.FromToRotation(localUp, gravityUp) * _rigidBody.rotation;
        }
    }

    public void Attract()
    {
        if (keepAttracted)
        {
            Vector3 gravityUp = CurrentPlanet.GetGravityUpForPosition(_rigidBody.position);
            _rigidBody.AddForce(gravityUp * (-CurrentPlanet.Gravity * Time.deltaTime));
        }
    }
}
