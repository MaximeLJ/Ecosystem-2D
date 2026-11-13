using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FoodInfos
{
    public string foodName;
    public enum FoodType {Plant, Meat, None};
    public enum FoodState {Growing, Eaten};
    public enum FoodDisease {None, Salmonella, Ichthyobesity, ToxicFood, ParasiticInfestation};
    public FoodDisease foodDisease;
    public FoodType foodType;
    public int timesEaten;
    public int spritesIndex;

    public Vector2 foodPos;
    public GameObject foodObject;
    public float size;
    public float lastChangeTime;
    public float randomGrowthTime;
    public int maxGrowth;

    public float colorRandom;
    public Color newFoodColor;
    public Vector2 randomFoodPos;

    public FoodInfos(GameManager gameManager, Food food, int mapSize, bool isStartSpawn = false)
    {
        foodType = FoodType.Plant;
        timesEaten = 0;
        lastChangeTime = Time.time;

        InitializeFoodName(food);
        CalculateRandomFoodPos(gameManager, mapSize);
        CreateFoodObject(gameManager);
        SetFoodColor(gameManager);
        GiveRandomDisease();

        randomGrowthTime = UnityEngine.Random.Range(gameManager.foodGrowthTime.min * 100, gameManager.foodGrowthTime.max * 100) / 100f;
        maxGrowth = UnityEngine.Random.Range(gameManager.minEndLifeGrowth,gameManager.foodSprites.Length);
        spritesIndex = (isStartSpawn)?UnityEngine.Random.Range(0,maxGrowth):0;
    }
    public FoodInfos(GameManager gameManager, Food food, Vector2 foodPos, float size)
    {
        foodType = FoodType.Meat;
        this.foodPos = foodPos;
        this.size = size;
        timesEaten = 0;

        InitializeFoodName(food);
        CreateFoodObject(gameManager);
    }

    private void InitializeFoodName(Food food)
    {
        foodName = $"Food{food.foodInfosDict.Count}";
        for (int i = 0; i <= food.foodInfosDict.Count; i++)
        {
            if (!food.foodInfosDict.ContainsKey($"Food{i}"))
            {
                foodName = $"Food{i}";
                break;
            }
        }
    }

    private void CalculateRandomFoodPos(GameManager gameManager, int mapSize)
    {
        for (int i = 1; i < gameManager.foodSpreadPrecision + 1; i++)
        {
            foodPos += UnityEngine.Random.insideUnitCircle * mapSize / i;
        }
        foodPos /= gameManager.foodSpreadPrecision;
    }

    private void CreateFoodObject(GameManager gameManager)
    {
        foodObject = new GameObject(foodName, typeof(SpriteRenderer));
        var spriteRenderer = foodObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = (foodType==FoodType.Plant)?gameManager.foodSprites[0]:gameManager.meatSprite;
        spriteRenderer.flipX = UnityEngine.Random.Range(0, 2) == 1;
        foodObject.transform.position = new Vector3(foodPos.x, foodPos.y);
        foodObject.transform.localScale *= (foodType==FoodType.Plant)?gameManager.foodScale:size;
        foodObject.transform.parent = gameManager.foodHolder;
        foodObject.AddComponent<BoxCollider2D>();

        SetFoodObjectLayer(gameManager);
    }

    private void SetFoodObjectLayer(GameManager gameManager)
    {
        for (int i = 0; i < 32; i++)
        {
            if (foodType == FoodType.Plant && (gameManager.plantLayer.value & (1 << i)) != 0)
            {
                foodObject.layer = i;
            }
            else if (foodType == FoodType.Meat && (gameManager.meatLayer.value & (1 << i)) != 0)
            {
                foodObject.layer = i;
            }
        }
    }

    private void SetFoodColor(GameManager gameManager)
    {
        colorRandom = UnityEngine.Random.Range(gameManager.minColorRandomness * 1000, gameManager.maxColorRandomness * 1000) / 1000f;
        newFoodColor = gameManager.foodColor / colorRandom;
        if (gameManager.randomiseAlpha == false)
        {
            newFoodColor = new Color(newFoodColor.r, newFoodColor.g, newFoodColor.b, 1f);
        }
        foodObject.GetComponent<SpriteRenderer>().color = newFoodColor;
    }

    private void GiveRandomDisease()
    {
        bool giveDisease = (UnityEngine.Random.Range(0,100) == 1);
        if (giveDisease) {
            foodDisease = (FoodDisease)UnityEngine.Random.Range(1, Enum.GetValues(typeof(FoodDisease)).Length);
            Debug.Log($"{foodName} has the disease {foodDisease.ToString()!}");
        } else {
            foodDisease = FoodDisease.None;
        }
    }
}