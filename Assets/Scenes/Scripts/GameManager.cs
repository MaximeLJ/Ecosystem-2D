using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapSize;
    [Range(0,20)]
    public float timeScale = 1f;

    [Header("Food Settings")]
    public float foodScale;
    public MinMaxRange foodGrowthTime;
    public int minEndLifeGrowth;
    public float timeBetweenSpawn;
    public int foodAmount;
    public float foodPosRandomness;
    public int foodSpreadPrecision;
    public bool randomiseAlpha;
    [Range(0,1)]
    public float minColorRandomness;
    [Range(0,1)]
    public float maxColorRandomness;
    public Color foodColor;

    [Header("Sullite Settings")]
    public SulliteTypeData sulliteTypes;
    public int sulliteAmount;
    public float sulliteScale;
    public int sullitePosRandomness;
    public bool useRandomStats;
    public MinMaxRange sulliteMaxHP;
    public MinMaxRange sulliteAttackPower;
    public MinMaxRange digestionTime;
    public MinMaxRange pregnancyTime;
    public MinMaxRange maturingTime;
    public MinMaxRange matingDuration;
    public MinMaxRange lifetime;
    public MinMaxRange sulliteSize;
    public MinMaxRange immunity;
    public MinMaxRange recoveryTime;
    public enum ColorChoice {SulliteColor, Sex, Species, Diet, State, Diseases};
    public ColorChoice colorChoice;
    public ColorMapping[] sexColors;
    public ColorMapping[] stateColors;
    public ColorMapping[] dietColors;
    public ColorMapping[] diseaseColors;
    public ColorMapping[] speciesColors;
    public MinMaxRange raycastDistance;
    public MinMaxRange angleRange;
    public MinMaxRange raycastPrecision;
    public float touchingRaycastDistance = 0.5f;
    public MinMaxRange sulliteSpeed;
    public MinMaxRange sulliteRotationSpeed;

    [Header("References")]
    public Sprite[] foodSprites;
    public Sprite meatSprite;
    public Sprite[] sulliteSprites;
    public Transform foodHolder;
    public Transform sulliteHolder;
    public Transform background;
    public Transform cameraMovementCover;
    public LayerMask plantLayer; // The layer(s) to interact with
    public LayerMask meatLayer;
    public LayerMask sulliteLayer; 
    public Camera cameraComponent;
    public Text numberOfSullites;

    Food food;
    Sullite sullite;
    CameraController cameraController;
    BackgroundManager backgroundManager;
    Graph graph;

    void Start()
    {
        cameraController = cameraComponent.GetComponent<CameraController>();
        cameraController.InitialiseCamera(this);

        backgroundManager = GetComponent<BackgroundManager>();
        graph = GetComponent<Graph>();

        food = GetComponent<Food>();
        food.LevelGrid(mapSize);

        sullite = GetComponent<Sullite>();
        sullite.InitialiseSullite();

    }

    void Update()
    {
        Time.timeScale = timeScale;

        food.UpdateFood();
        sullite.UpdateSullites();
        graph.UpdateGraph();

        DisplayText();
    }

    private void DisplayText()
    {
        if (numberOfSullites != null && sullite.sulliteInfosDict != null)
        {
            // Update the text to display the length of the dictionary
            numberOfSullites.text = "Number of Sullites: " + sullite.sulliteInfosDict.Count.ToString();
        }
        else
        {
            Debug.LogError("Please assign the Text component and provide a valid dictionary reference in the inspector.");
        }
    }

    public void Reset()
    {
        sullite.Clean();
        food.Clean();
        graph.Clean();
    }

    public void Spawn()
    {
        food.canSpawn = true;
        if (food.foodInfosDict.Count == 0 || sullite.sulliteInfosDict.Count == 0) {
            graph.startTime = Time.time;
            graph.Clean();
        }
        if (food.foodInfosDict.Count == 0) {
            for (int i = 0; i < foodAmount/2; i++) {
                food.SpawnFood(true);
            }
        }

        sullite.SpawnAllSullites(sulliteAmount);
    }
}


[System.Serializable]
public class MinMaxRange
{
    public int min = 0;
    public int max = 1;
    public float middle = 0.5f;

    public MinMaxRange(int min, int max)
    {
        this.min = min;
        this.max = max;
        this.middle = (min+max)/2f;
    }
}

[System.Serializable]
public struct ColorMapping
{
    public string label;
    public Color color;
}