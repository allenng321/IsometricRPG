using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlanetManager : MonoBehaviour
{
    private Planet[] planets;
    private PlanetInhabitant[] inhabitants;

    private void Awake()
    {
        planets = (Planet[]) FindObjectsOfType(typeof(Planet));
        inhabitants = (PlanetInhabitant[]) FindObjectsOfType(typeof(PlanetInhabitant));
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            foreach (PlanetInhabitant inhabitant in inhabitants)
            {
                inhabitant.Align();
            }
        }

        foreach (Planet planet in planets)
        {
            planet.Tick();
        }
    }

    private void FixedUpdate()
    {
        if (Application.isPlaying)
        {
            foreach (PlanetInhabitant inhabitant in inhabitants)
            {
                inhabitant.Attract();
            }
        }
    }
}
