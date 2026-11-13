using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public bool showGraph;
    public LineRenderer lineRenderer;
    public Color graphColor = Color.white; // Set your desired color
    public int fractionOfScreenWidth;
    public int fractionOfScreenHeight;
    private List<float> graphDataX = new List<float>();
    private List<int> graphDataY = new List<int>();
    public float startTime;

    private Sullite sullite;
    private GameManager gameManager;

    float cameraWidth;
    float cameraHeight;

    float originalLineWidth;


    void Start()
    {
        sullite = GetComponent<Sullite>();
        gameManager = GetComponent<GameManager>();

        originalLineWidth = lineRenderer.startWidth;
        startTime = Time.time;
    }

    public void UpdateGraph()
    {
        lineRenderer.enabled = showGraph;
        float time = Time.time - startTime;

        // Set the color of the line renderer
        lineRenderer.startColor = graphColor;
        lineRenderer.endColor = graphColor;

        Camera cameraComponent = Camera.main;

        // Calculate the width of the camera
        cameraWidth = 2f * cameraComponent.orthographicSize * cameraComponent.aspect;
        cameraHeight = Camera.main.orthographicSize*2;

        lineRenderer.startWidth = originalLineWidth/40*Camera.main.orthographicSize;
        lineRenderer.endWidth = originalLineWidth/40*Camera.main.orthographicSize;

        graphDataX.Add(time);
        graphDataY.Add(sullite.sulliteInfosDict.Count);
        lineRenderer.positionCount = graphDataY.Count;

        Vector2 cameraPos = Camera.main.transform.position;

        time = (graphDataX.Max()!=0)?graphDataX.Max():1;
        int maxCount = (graphDataY.Max()!=0)?graphDataY.Max():1;
        float fx = fractionOfScreenWidth;
        float fy = fractionOfScreenHeight;

        if (showGraph) {
            for (int i = 0; i < graphDataY.Count; i++)
            {
                float x = (graphDataX[i]/time*cameraWidth/fx) - (cameraWidth/2) + cameraPos.x;
                float y = (((float)graphDataY[i])/maxCount*cameraHeight/fy) + ((cameraHeight/2f)-(cameraHeight/fy)) + cameraPos.y;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }
        }
        

    }
    
    public void Clean()
    {
        graphDataX.Clear();
        graphDataY.Clear();
    }
}