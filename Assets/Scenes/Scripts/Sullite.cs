using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Sullite : MonoBehaviour
{    
    GameManager gameManager;
    Food food;

    public Dictionary<string, SulliteInfos> sulliteInfosDict = new Dictionary<string, SulliteInfos>();
    SulliteInfos previousStats;

    public void InitialiseSullite()
    {
        this.gameManager = GetComponent<GameManager>();
        food = GetComponent<Food>();
    }

    public void UpdateSullites()
    {
        List<(string, string)> toDestroy = new List<(string, string)>();
        List<(Vector2, SulliteInfos)> toSpawn = new List<(Vector2, SulliteInfos)>();
        foreach (var kvp in sulliteInfosDict)
        {
            SulliteInfos sullite = sulliteInfosDict[kvp.Key];

            UpdateSulliteState(sullite);
            UpdateSulliteBreeding(sullite, toSpawn);             
            UpdateSulliteBehaviour(sullite);
            UpdateSulliteStats(sullite, toDestroy);

            sullite.UpdateSulliteObject();
            sullite.CheckForDisease(food, this);

            ManageStatsView();
        }
        SpawnAndDestroysullites(toSpawn, toDestroy);
    }

    private void UpdateSulliteState(SulliteInfos sullite)
    {
        if (Time.time - sullite.spawnTime > (float)sullite.stats["maturingTime"] && sullite.state == SulliteInfos.State.Maturing) {
            sullite.state = SulliteInfos.State.Idle;
        } else if (sullite.state == SulliteInfos.State.Maturing || Time.time - sullite.spawnTime < (float)sullite.stats["maturingTime"]) {
            sullite.state = SulliteInfos.State.Maturing;
            sullite.stats["size"] = (Time.time - sullite.spawnTime) / (float)sullite.stats["maturingTime"] * ((float)sullite.stats["endSize"]/3*2) + (float)sullite.stats["endSize"]/3;
        } else if (sullite.health > sullite.maxHP/3*2 && sullite.state == SulliteInfos.State.Idle) {
            sullite.state = SulliteInfos.State.WantsToBreed;
        } else if (sullite.health <= sullite.maxHP/3*2 && sullite.state == SulliteInfos.State.WantsToBreed) {
            sullite.state = SulliteInfos.State.Idle;
        }
    }

    private void UpdateSulliteBehaviour(SulliteInfos sullite)
    {
        Vector3 closestSullite = sullite.ClosestObjectOfLayer(gameManager.sulliteLayer);
        Vector3 closestFood = sullite.ClosestObjectOfLayer(sullite.foodLayer);
        
        bool canMate = IsMatingPossible(sullite);
        bool isTouchingFood = sullite.IsTouchingObjectOfLayer(sullite.foodLayer);
        bool isAttacking = sullite.IsAttacking(closestFood, closestSullite);

        if (sullite.state == SulliteInfos.State.WantsToBreed && !canMate && closestSullite!=sullite.transformSullite.position) {
            UpdateMovement(closestSullite, sullite);
        } else if (sullite.state == SulliteInfos.State.WantsToBreed && canMate 
        && sulliteInfosDict[sullite.mateName].state == SulliteInfos.State.WantsToBreed) {
            UpdateBreedingStats(sullite);
        } else if (sullite.state == SulliteInfos.State.Breeding){
            sullite.UpdatePosition(sullite.randomDirection, true);
        } else if (isTouchingFood && sullite.health < sullite.maxHP) {
            UpdateEatingSullite(sullite);
        } else if (sullite.health >= sullite.maxHP) {
            UpdateMovement(sullite.transformSullite.position, sullite);
        } else if (isAttacking) {
            UpdateMovement(closestSullite, sullite);
        } else {
            UpdateMovement(closestFood, sullite);
        }
    }

    private void UpdateSulliteBreeding(SulliteInfos sullite, List<(Vector2, SulliteInfos)> toSpawn)
    {
        if (Time.time - sullite.startBreading > (float)sullite.stats["matingDuration"] && sullite.state == SulliteInfos.State.Breeding && sulliteInfosDict.ContainsKey(sullite.mateName) 
        && sullite.IsNotChildCarrier(sullite.sex, sulliteInfosDict[sullite.mateName].sex)){
            sullite.lastBred = Time.time;
            sullite.health -= sullite.maxHP/3;
            sulliteInfosDict[sullite.mateName].lastBred = Time.time;
            sulliteInfosDict[sullite.mateName].health -= sullite.maxHP/3;

            sullite.state = SulliteInfos.State.Idle;
            sulliteInfosDict[sullite.mateName].state = SulliteInfos.State.Pregnant;
        } else if (sullite.state == SulliteInfos.State.Breeding && !sulliteInfosDict.ContainsKey(sullite.mateName)) {
            sullite.lastBred = Time.time;
            sullite.health -= sullite.maxHP/3;
            sullite.state = SulliteInfos.State.Idle;
        } else if (Time.time - sullite.lastBred > (float)sullite.stats["pregnancyTime"] && sullite.state == SulliteInfos.State.Pregnant) {
            sullite.state = SulliteInfos.State.Idle;
            sullite.health -= sullite.maxHP/3;
            toSpawn.Add((sullite.transformSullite.position, sullite));
        }
    }

    private void UpdateBreedingStats(SulliteInfos sullite)
    {
        sullite.startBreading = sulliteInfosDict[sullite.mateName].startBreading = Time.time;

        sullite.state = sulliteInfosDict[sullite.mateName].state = SulliteInfos.State.Breeding;
        sulliteInfosDict[sullite.mateName].mateName = sullite.sulliteName;
        sullite.mateSex = sulliteInfosDict[sullite.mateName].sex;
        sulliteInfosDict[sullite.mateName].mateSex = sullite.mateSex;
        sullite.randomDirection = sulliteInfosDict[sullite.mateName].randomDirection = sullite.SetRandomDirection();

        sullite.newGenes = sulliteInfosDict[sullite.mateName].newGenes = sullite.Breed(sullite, sulliteInfosDict[sullite.mateName]);

        sullite.ChangeSex();
        sulliteInfosDict[sullite.mateName].ChangeSex();
    }

    private bool IsMatingPossible(SulliteInfos sullite)
    {
        if (sullite.state == SulliteInfos.State.WantsToBreed) {
            return sullite.CanMate(gameManager.sulliteLayer);
        } else if (sullite.state == SulliteInfos.State.Pregnant) {
            return false;
        } else if (sullite.state == SulliteInfos.State.Breeding) {
            return true;
        } else {
            return false;
        }
    }

    private void UpdateEatingSullite(SulliteInfos sullite)
    {
        sullite.CheckForDisease(food, this);
        food.UpdateFoodSprite(sullite.foodName, FoodInfos.FoodState.Eaten);
        sullite.health++;
        sullite.lastEaten = Time.time;
    }

    private void UpdateSulliteStats(SulliteInfos sullite, List<(string, string)> toDestroy)
    {
        if (Time.time - sullite.lastEaten > (float)sullite.stats["digestingTime"] && sullite.health >= 0) {
            sullite.health--;
            sullite.nextFood = sullite.ChooseNextFood();
            sullite.lastEaten = Time.time;
            if (sullite.health <= 0) {
                toDestroy.Add((sullite.sulliteName, SulliteInfos.DeathMessage.Starvation.ToString()));
            }
        }


        if (sullite.foodName!=null) {
            SulliteInfos foodInfos;
            bool keyExists = sulliteInfosDict.TryGetValue(sullite.foodName, out foodInfos);
            if (keyExists && foodInfos.health <= 0) {
                toDestroy.Add((sullite.foodName, $"an attack from {sullite.sulliteName}"));
            }
        }
        if (Time.time - sullite.spawnTime > (float)sullite.stats["lifetime"]){
            toDestroy.Add((sullite.sulliteName, SulliteInfos.DeathMessage.OldAge.ToString()));
        }
        sullite.GiveRandomDisease();
    }

    private void SpawnAndDestroysullites(List<(Vector2, SulliteInfos)> toSpawn, List<(string, string)> toDestroy)
    {
        for (int i = 0; i < toSpawn.Count; i++) {
            print($"parent1: {toSpawn[i].Item2.sulliteName} ({toSpawn[i].Item2.sex}), parent2: {toSpawn[i].Item2.mateName} ({toSpawn[i].Item2.mateSex})");
            Type parentType = sulliteInfosDict[toSpawn[i].Item2.sulliteName].GetType();
            SpawnSullite(parentType, toSpawn[i].Item1, toSpawn[i].Item2.newGenes);
        }

        for (int i = 0; i < toDestroy.Count; i++) {
            DestroySullite(toDestroy[i].Item1, toDestroy[i].Item2);
        }
    }

    public void DestroySullite(string name, string deathMessage = null)
    {
        food.SpawnMeat(sulliteInfosDict[name].transformSullite.position, sulliteInfosDict[name].transformSullite.localScale.x);
        Destroy(sulliteInfosDict[name].sulliteGameObject);

        if (deathMessage!=null) {
            string completeDeathMessage = $"{name} ({sulliteInfosDict[name].sex}) died by {deathMessage}. Its expected lifetime was {sulliteInfosDict[name].stats["lifetime"]} and it lived {Time.time - sulliteInfosDict[name].spawnTime}. Origininal lifetime: {sulliteInfosDict[name].genes["lifetime"].Item1}.";
            Debug.Log(completeDeathMessage);
        }

        sulliteInfosDict.Remove(name);
    }

    public void SpawnAllSullites(int sulliteAmount)
    {
        int remainingSullites = sulliteAmount;

        while (remainingSullites > 0) {
            if (gameManager.sulliteTypes.createMammal) {
                SpawnSullite(typeof(MammalSullite));
                remainingSullites--;
            }

            if (remainingSullites > 0 && gameManager.sulliteTypes.createHermaphrodite) {
                SpawnSullite(typeof(HermaphroditeSullite));
                remainingSullites--;
            }

            if (remainingSullites > 0 && gameManager.sulliteTypes.createProtandrous) {
                SpawnSullite(typeof(ProtandrousSullite));
                remainingSullites--;
            }

            if (remainingSullites > 0 && gameManager.sulliteTypes.createProtogynous) {
                SpawnSullite(typeof(ProtogynousSullite));
                remainingSullites--;
            }
        }
    }

    public void SpawnSullite(Type sulliteType, Vector2 pos = default, Dictionary<string, (object, float, object, float, MinMaxRange)> genes = null)
    {
        int mapSize = gameManager.mapSize;
        pos = (pos == Vector2.zero)?GetRandomPosition(mapSize):pos;
        Vector2 sullitePos = new Vector2(pos.x, pos.y);

        SulliteInfos newSullite;
        newSullite = Activator.CreateInstance(sulliteType, gameManager, this, sullitePos, genes) as SulliteInfos;

        sulliteInfosDict.Add(newSullite.sulliteName, newSullite);

        print($"{newSullite.sulliteName} ({newSullite.sex})");
    }

    private Vector2 GetRandomPosition(int mapSize)
    {
        Vector2 pos;

        pos.x = UnityEngine.Random.Range((-mapSize / 2) * gameManager.sullitePosRandomness, (mapSize / 2) * gameManager.sullitePosRandomness) / gameManager.sullitePosRandomness;
        pos.y = UnityEngine.Random.Range((-mapSize / 2) * gameManager.sullitePosRandomness, (mapSize / 2) * gameManager.sullitePosRandomness) / gameManager.sullitePosRandomness;

        return pos;
    }

    public void UpdateMovement(Vector3 foodPos, SulliteInfos sulliteInfos)
    {
        Vector3 newPos;
        Vector3 sullitePos = sulliteInfos.transformSullite.position;
        Vector3 targetPos = (foodPos==sullitePos)?sulliteInfos.randomDirection:foodPos;
        float distanceFromTarget = Vector2.Distance((Vector2)targetPos, sullitePos);
        float distanceThreshold = .5f;

        if (foodPos != sullitePos) {
            newPos = foodPos;
        } else if (distanceFromTarget < distanceThreshold || distanceFromTarget > (float)sulliteInfos.stats["raycastDistance"]/3){
            newPos = sulliteInfos.randomDirection = sulliteInfos.SetRandomDirection();
        } else {
            newPos = sulliteInfos.randomDirection;
        }
        sulliteInfos.UpdatePosition(newPos);
    }

    private void ManageStatsView()
    {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePosition = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 1, gameManager.sulliteLayer);

            if (hit.collider != null && sulliteInfosDict[hit.collider.gameObject.name] != previousStats) {
                print(hit.collider.gameObject.name);
                previousStats = sulliteInfosDict[hit.collider.gameObject.name];
                foreach (var item in sulliteInfosDict[hit.collider.gameObject.name].genes) {
                    print($"{item.Key} {item.Value}");
                }
                foreach (var item in sulliteInfosDict[hit.collider.gameObject.name].stats) {
                    print($"{item.Key} {item.Value}");
                }
            }
        }
    }

    public void Clean()
    {
        List<string> toDestroy = new List<string>();
        foreach (var kvp in sulliteInfosDict) {
            toDestroy.Add(kvp.Key);
        }
        for (int i = 0; i < toDestroy.Count; i++) {
            DestroySullite(toDestroy[i]);
        }
        Debug.Log("Cleaned Up");
    }

    void OnApplicationQuit()
    {
        Clean();
    }
}