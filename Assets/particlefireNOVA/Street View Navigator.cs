using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StreetViewAutoNavigator : MonoBehaviour
{
    [Header("Street View Settings")]
    [SerializeField] private string apiKey = "YOUR_GOOGLE_API_KEY";
    [SerializeField] private Material sphereMaterial;
    [SerializeField] private Transform cameraRig;
    
    [Header("Navigation Settings")]
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private float pauseAtLocation = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool autoRotate = true;
    
    [Header("Cache Settings")]
    [SerializeField] private int maxCacheSize = 10;

    private GameObject currentSphere;
    private GameObject nextSphere;
    private Vector3 currentLocation;
    private List<Vector3> navigationPoints = new List<Vector3>();
    private int currentPointIndex = 0;
    private bool isTransitioning = false;
    private Dictionary<Vector2, Texture2D> imageCache = new Dictionary<Vector2, Texture2D>();
    private Queue<Vector2> cacheQueue = new Queue<Vector2>();

    private void Start()
    {
        CreateViewSphere();
        StartCoroutine(AutoNavigateCoroutine());
    }

    private void CreateViewSphere()
    {
        currentSphere = CreateSphereObject();
        currentSphere.transform.SetParent(transform);
        
        // Create second sphere for transitions
        nextSphere = CreateSphereObject();
        nextSphere.transform.SetParent(transform);
        nextSphere.SetActive(false);
    }

    private GameObject CreateSphereObject()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(-10f, 10f, 10f);
        
        Material newMaterial = new Material(sphereMaterial);
        sphere.GetComponent<MeshRenderer>().material = newMaterial;
        
        return sphere;
    }

    private IEnumerator AutoNavigateCoroutine()
    {
        while (true)
        {
            // Wait for any ongoing transition
            while (isTransitioning)
            {
                yield return null;
            }

            // Pause at current location
            yield return new WaitForSeconds(pauseAtLocation);

            // Move to next point
            if (currentPointIndex < navigationPoints.Count - 1)
            {
                yield return StartCoroutine(TransitionToNextPoint());
                currentPointIndex++;
            }
            else
            {
                // Reset to beginning or stop based on your needs
                currentPointIndex = 0;
                yield return StartCoroutine(TransitionToNextPoint());
            }
        }
    }

    private IEnumerator TransitionToNextPoint()
    {
        isTransitioning = true;
        Vector3 nextLocation = navigationPoints[currentPointIndex + 1];

        // Pre-load next location's texture
        yield return StartCoroutine(PreloadNextLocation(nextLocation));

        // Prepare next sphere
        nextSphere.SetActive(true);
        nextSphere.transform.position = nextLocation;
        
        float elapsedTime = 0f;
        Vector3 startPos = currentSphere.transform.position;
        
        // Fade in/out and position transition
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            float fadeValue = Mathf.SmoothStep(0, 1, t);

            // Update materials
            currentSphere.GetComponent<MeshRenderer>().material.SetFloat("_Alpha", 1 - fadeValue);
            nextSphere.GetComponent<MeshRenderer>().material.SetFloat("_Alpha", fadeValue);

            // Move camera
            transform.position = Vector3.Lerp(startPos, nextLocation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Swap spheres
        GameObject temp = currentSphere;
        currentSphere = nextSphere;
        nextSphere = temp;
        
        nextSphere.SetActive(false);
        currentLocation = nextLocation;
        
        isTransitioning = false;
    }

    private IEnumerator PreloadNextLocation(Vector3 location)
    {
        Vector2 latLong = ConvertToLatLong(location);
        
        // Check cache first
        if (!imageCache.ContainsKey(latLong))
        {
            yield return StartCoroutine(LoadAndCacheStreetViewImage(latLong));
        }

        // Apply cached texture to next sphere
        nextSphere.GetComponent<MeshRenderer>().material.mainTexture = imageCache[latLong];
    }

    private IEnumerator LoadAndCacheStreetViewImage(Vector2 latLong)
    {
        string imageUrl = $"https://maps.googleapis.com/maps/api/streetview" +
                         $"?location={latLong.x},{latLong.y}" +
                         $"&size=640x640&key={apiKey}";

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D streetViewTexture = DownloadHandlerTexture.GetContent(request);
                CacheTexture(latLong, streetViewTexture);
            }
        }
    }

    private void CacheTexture(Vector2 latLong, Texture2D texture)
    {
        // Remove oldest cache entry if we're at capacity
        if (cacheQueue.Count >= maxCacheSize)
        {
            Vector2 oldestKey = cacheQueue.Dequeue();
            if (imageCache.ContainsKey(oldestKey))
            {
                Destroy(imageCache[oldestKey]);
                imageCache.Remove(oldestKey);
            }
        }

        // Add new texture to cache
        imageCache[latLong] = texture;
        cacheQueue.Enqueue(latLong);
    }

    private Vector2 ConvertToLatLong(Vector3 unityPosition)
    {
        // Implement conversion from Unity world position to latitude/longitude
        float latitude = unityPosition.x * 0.0001f;
        float longitude = unityPosition.z * 0.0001f;
        return new Vector2(latitude, longitude);
    }

    private void Update()
    {
        if (autoRotate && !isTransitioning)
        {
            cameraRig.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    // Call this method to set up your navigation path
    public void SetNavigationPath(List<Vector3> points)
    {
        navigationPoints = new List<Vector3>(points);
        currentPointIndex = 0;
        currentLocation = navigationPoints[0];
        
        // Start loading first location
        StartCoroutine(LoadAndCacheStreetViewImage(ConvertToLatLong(currentLocation)));
    }

    private void OnDestroy()
    {
        // Clean up cached textures
        foreach (var texture in imageCache.Values)
        {
            Destroy(texture);
        }
    }
}

public class TransitionShader : MonoBehaviour
{
    public Shader streetViewShader;
}