using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Food : MonoBehaviour
{
    private int mapSize;
    private float lastSpawningTime;
    public bool canSpawn = false;

    GameManager gameManager;

    public Dictionary<string, FoodInfos> foodInfosDict = new Dictionary<string, FoodInfos>();

    public void LevelGrid(int size) 
    {
        this.mapSize = size;

        gameManager = GetComponent<GameManager>();
    }

    public void SpawnFood(bool isStartSpawn = false)
    {
        if (canSpawn) {
            FoodInfos new_food = new FoodInfos(gameManager, this, mapSize, isStartSpawn);
            foodInfosDict.Add(new_food.foodName, new_food);
            lastSpawningTime = Time.time;
        }
    }

    public void SpawnMeat(Vector2 foodPos, float size)
    {
        FoodInfos new_food = new FoodInfos(gameManager, this, foodPos, size);
        foodInfosDict.Add(new_food.foodName, new_food);
    }


    public void DestroyFood(string name)
    {
        Destroy(foodInfosDict[name].foodObject);

        foodInfosDict.Remove(name);
    }

    public void UpdateFoodSprite(string gameObjectName, FoodInfos.FoodState foodState = FoodInfos.FoodState.Growing)
    {
        if (foodInfosDict[gameObjectName].foodType == FoodInfos.FoodType.Plant) {
            UpdatePlantSprite(gameObjectName, foodState);
        } else if (foodInfosDict[gameObjectName].foodType == FoodInfos.FoodType.Meat) {
            UpdateMeatSprite(gameObjectName);
        }
    }

    private void UpdatePlantSprite(string gameObjectName, FoodInfos.FoodState foodState = FoodInfos.FoodState.Growing)
    {
        Sprite[] sprites = gameManager.foodSprites;  
        FoodInfos food = foodInfosDict[gameObjectName];      
        float timeSinceLastGrowth = Time.time-food.lastChangeTime;

        int oldIndex = food.spritesIndex;
        if (timeSinceLastGrowth > food.randomGrowthTime && foodState == FoodInfos.FoodState.Growing && oldIndex<food.maxGrowth) {
            food.spritesIndex++;
            food.lastChangeTime = Time.time;
        } else if (foodState == FoodInfos.FoodState.Eaten) {
            food.spritesIndex--;
            food.lastChangeTime = Time.time;
        }
        int newIndex = food.spritesIndex;

        if (newIndex <= food.maxGrowth && newIndex >= 0) {
            food.foodObject.GetComponent<SpriteRenderer>().sprite = gameManager.foodSprites[newIndex];
            food.foodObject.GetComponent<BoxCollider2D>().size = food.foodObject.GetComponent<SpriteRenderer>().bounds.size/gameManager.foodScale;
        } else if (foodState == FoodInfos.FoodState.Eaten && newIndex < 0) {
                DestroyFood(gameObjectName);
        }
    }

    private void UpdateMeatSprite(string gameObjectName)
    {
        FoodInfos meat = foodInfosDict[gameObjectName];
        meat.foodObject.transform.localScale -= Vector3.one*gameManager.sulliteScale/2f;
        meat.timesEaten++;
        if (meat.timesEaten>=Math.Round(meat.size)/gameManager.sulliteScale) {
            DestroyFood(gameObjectName);
        }
    }

    public void UpdateFood()
    {
        if (foodInfosDict.Count < gameManager.foodAmount && Time.time - lastSpawningTime > gameManager.timeBetweenSpawn) {
            SpawnFood();
        }

        foreach (var kvp in foodInfosDict) {
            if (kvp.Value.foodType == FoodInfos.FoodType.Plant) {
                UpdateFoodSprite(kvp.Key);
            }
        }
    }

    public void Clean()
    {
        List<string> toDestroy = new List<string>();
        foreach (var kvp in foodInfosDict) {
            toDestroy.Add(kvp.Key);
        }
        for (int i = 0; i < toDestroy.Count; i++) {
            DestroyFood(toDestroy[i]);
        }

        canSpawn = false;
        Debug.Log("Cleaned Up");
    }

    void OnApplicationQuit()
    {
        Clean();
    }
}