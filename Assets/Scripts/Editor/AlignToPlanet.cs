using UnityEditor;
using UnityEngine;

public class AlignToPlanet : MonoBehaviour
{
    [MenuItem("Custom/Align to planet %g")]
    public static void Align()
    {
        var planets = Resources.FindObjectsOfTypeAll<Planet>();

        foreach(var transform in Selection.transforms)
        {
            Transform bestPlanet = null;
            float closest = Mathf.Infinity;
            
            foreach(Planet planet in planets)
            {
                Vector3 directionToPlanet = planet.transform.position - transform.position;
                float dSqr = directionToPlanet.sqrMagnitude;
                if(dSqr < closest)
                {
                    closest = dSqr;
                    bestPlanet = planet.transform;
                }
            }      
            
            Vector3 gravityUp = (transform.position - bestPlanet.position).normalized;
            Vector3 localUp = transform.up;
            // Align up axis with the center of planet
            transform.rotation = Quaternion.FromToRotation(localUp, gravityUp) * transform.rotation;
        }
    }

}