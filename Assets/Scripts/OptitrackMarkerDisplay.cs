using System.Collections.Generic;
using UnityEngine;

public class OptitrackMarkerDisplay : MonoBehaviour
{
    // Reference to the OptitrackStreamingClient in the scene
    private OptitrackStreamingClient streamingClient;

    // A list to store the latest marker states
    private List<OptitrackMarkerState> markerStates;

    // Dictionary to store marker ID and their corresponding spheres
    private Dictionary<int, GameObject> markerSpheres = new Dictionary<int, GameObject>();

    void Start()
    {
        // Find the OptitrackStreamingClient in the scene
        streamingClient = OptitrackStreamingClient.FindDefaultClient();

        if (streamingClient == null)
        {
            Debug.LogError("OptitrackStreamingClient not found. Make sure it exists in the scene.");
            return;
        }

        // Initialize the list for storing marker states
        markerStates = new List<OptitrackMarkerState>();
    }

    void Update()
    {
        if (streamingClient == null)
        {
            return;
        }

        // Get the latest marker states from the streaming client
        markerStates = streamingClient.GetLatestMarkerStates();

        // Check if any marker data is available
        if (markerStates == null || markerStates.Count == 0)
        {
            Debug.LogWarning("No markers tracked or no data received.");
            return;
        }

        // Update marker positions and create spheres if necessary
        UpdateMarkerSpheres();
    }

    void UpdateMarkerSpheres()
    {
        // Loop through each marker state
        foreach (OptitrackMarkerState marker in markerStates)
        {
            // If a sphere for this marker ID doesn't exist, create one
            if (!markerSpheres.ContainsKey(marker.Id))
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f); // Set size of the sphere
                markerSpheres[marker.Id] = sphere;
            }

            // Update the sphere's position to the marker's position
            markerSpheres[marker.Id].transform.position = marker.Position;
        }
    }
}
