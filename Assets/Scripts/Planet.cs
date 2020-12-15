using System;
using UnityEngine;

public class Planet : MonoBehaviour {
	
    public event Action<Planet> OnPlanetChange = delegate { };
    public float Gravity = 10f;
    
    public float rotationSpeed = 10f;
    public Transform orbitTarget;
    public Vector2 orbitRadius = new Vector2(10f, 10f);
    [Range(0.0f, 360.0f)]
    public float orbitProgress = 0f;
    public float orbitSpeed = 10f;
    public Vector3 orbitPlane = Vector3.zero;
    public Vector3 orbitOffset = Vector3.zero;
    private int gizmoResolution = 500;
    private Quaternion _orbitPlane = Quaternion.identity;
    private SphereCollider gravityField;
    
    private void Awake()
    {
        SphereCollider surface = gameObject.GetComponent<SphereCollider>();
        gravityField = gameObject.AddComponent<SphereCollider>();
        gravityField.center = surface.center;
        gravityField.radius = surface.radius * 1.1f;
        gravityField.isTrigger = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var inhabitant = other.GetComponent<PlanetInhabitant>();
        if (inhabitant != null)
        {
            OnPlanetChange(this);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 255, 255, 0.5f);
        Vector3 from = _GetEllipsisPoint(0f) + orbitOffset;
        for (int i = 1; i < gizmoResolution; i++) 
        {
            Vector3 to = _GetEllipsisPoint((float)i / gizmoResolution * 360) + orbitOffset;
            Gizmos.DrawLine(from, to);
            from = to;
        }
        Gizmos.DrawLine(from, _GetEllipsisPoint(0f) + orbitOffset);
        Gizmos.matrix = Matrix4x4.TRS(
            orbitTarget.position + orbitOffset, 
            Quaternion.Euler(orbitPlane), 
            new Vector3(1, 1, 1));
        Gizmos.DrawCube(Vector3.zero, 5 * Vector3.one);
    }
    
    private Vector3 _GetEllipsisPoint(float angle)
    {
        float rad = Mathf.Deg2Rad * angle;
        Vector3 pointPosition = new Vector3(
            orbitRadius.x * Mathf.Cos (rad), 
            orbitRadius.y * Mathf.Sin (rad), 
            0.0f);
        _orbitPlane.eulerAngles = orbitPlane;
        pointPosition = _orbitPlane * pointPosition;
        return  orbitTarget.position + pointPosition;
    }

    public Vector3 GetGravityUpForPosition(Vector3 position)
    {
        return (position - transform.position).normalized;
    }
    
    public void Tick()
    {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * rotationSpeed);
        orbitProgress = (orbitProgress + orbitSpeed * Time.deltaTime) % 360;
        transform.position = _GetEllipsisPoint(orbitProgress) + orbitOffset;
    }
}