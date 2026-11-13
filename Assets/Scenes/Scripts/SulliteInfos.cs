using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Diseases;

public class SulliteInfos : ISpecies, ISex
{
    public float scale;
    public string sulliteName;
    public Dictionary<string, (object, float, object, float, MinMaxRange)> genes = new Dictionary<string, (object, float, object, float, MinMaxRange)>();
    public Dictionary<string, object> stats = new Dictionary<string, object>();
    public Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes = new Dictionary<string, (object, float, object, float, MinMaxRange)>();

    public Dictionary<string, Color> sexColors = new Dictionary<string, Color>();
    public Dictionary<string, Color> stateColors = new Dictionary<string, Color>();
    public Dictionary<string, Color> dietColors = new Dictionary<string, Color>();
    public Dictionary<string, Color> diseaseColors = new Dictionary<string, Color>();
    public Dictionary<string, Color> speciescolors = new Dictionary<string, Color>();

    public enum DeathMessage {Starvation, OldAge};
    public enum Sex {Male, Female, Hermaphrodite, Protogynous, Protandrous};
    public enum State {Maturing, Pregnant, Breeding, WantsToBreed, Idle, Attacking};
    public enum SulliteDisease {None, WhiteSpot, FinRot, ScaleInfection};
    public SulliteDisease sulliteDisease;
    public FoodInfos.FoodDisease foodDisease;
    public FoodInfos.FoodType nextFood;
    public LayerMask foodLayer;
    public Dictionary<string, List<(string, float)>> geneticDiseases;
    public Dictionary<string, List<(string, float)>> infectiousDiseases;
    public State state;
    public Sex sex;

    public string foodName;
    public float lastEaten;
    public string mateName;
    public Sex mateSex;
    public float startBreading;
    public float lastBred;
    public float lastSick;
    public float spawnTime;

    public int maxHP;
    public float health;
    public Vector2 randomDirection;

    public Transform transformSullite;
    public GameObject sulliteGameObject;
    public GameManager gameManager;
    public Sullite sullite;

    public SulliteInfos (GameManager gameManager, Sullite sullite, Vector2 sullitePos, Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes = null)
    {
        this.gameManager = gameManager;
        this.sullite = sullite;

        InitialiseSulliteName(sullite);
        SetSulliteStats(newGenes);
        GiveSulliteDiseases();
        CreateSulliteObject(sullitePos);
    }

    private void InitialiseSulliteName(Sullite sullite)
    {
        sulliteName = $"Sullite{sullite.sulliteInfosDict.Count}";
        for (int i = 0; i <= sullite.sulliteInfosDict.Count; i++)
        {
            if (!sullite.sulliteInfosDict.ContainsKey($"Sullite{i}"))
            {
                sulliteName = $"Sullite{i}";
                break;
            }
        }
    }

    private void SetSulliteStats(Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes)
    {
        scale = gameManager.sulliteScale;

        SetGeneticDiseases();
        SetGenesStats(newGenes);

        maxHP = (int)genes["maxHP"].Item1;
        health = maxHP/2;
        foodName = null;
        nextFood = ChooseNextFood();

        sexColors = SetColorDict(gameManager.sexColors);
        stateColors = SetColorDict(gameManager.stateColors);
        dietColors = SetColorDict(gameManager.dietColors);
        diseaseColors = SetColorDict(gameManager.diseaseColors);
        speciescolors = SetColorDict(gameManager.speciesColors);


        state = State.Maturing;
        
        lastEaten = Time.time;
        lastBred = Time.time;
        spawnTime = Time.time;

    }

    private void SetGeneticDiseases()
    {
        geneticDiseases = Diseases.Diseases.GeneticDiseases;
        infectiousDiseases =  Diseases.Diseases.InfectiousDiseases;
    }

    private void SetGenesStats(Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes)
    {
        if (newGenes == null) {
            genes["speed"] = CreateMyGeneTuple(typeof(float), gameManager.sulliteSpeed, 2, scale);
            genes["rotationSpeed"] = CreateMyGeneTuple(typeof(float), gameManager.sulliteRotationSpeed, 2);

            genes["raycastDistance"] = CreateMyGeneTuple(typeof(float), gameManager.raycastDistance, 1, scale);
            genes["angleRange"] = CreateMyGeneTuple(typeof(int), gameManager.angleRange);
            genes["raycastPrecision"] = CreateMyGeneTuple(typeof(int), gameManager.raycastPrecision);

            genes["digestingTime"] = CreateMyGeneTuple(typeof(float), gameManager.digestionTime, 2);
            genes["maxHP"] = CreateMyGeneTuple(typeof(int), gameManager.sulliteMaxHP);
            genes["attackPower"] = CreateMyGeneTuple(typeof(float), gameManager.sulliteAttackPower, 2);
            genes["diet"] = CreateMyGeneTuple(typeof(float), new MinMaxRange(0,1), 2);

            genes["matingDuration"] = CreateMyGeneTuple(typeof(float), gameManager.matingDuration, 2);
            genes["pregnancyTime"] = CreateMyGeneTuple(typeof(float), gameManager.pregnancyTime, 2);
            genes["maturingTime"] = CreateMyGeneTuple(typeof(float), gameManager.maturingTime, 2);

            genes["lifetime"] = CreateMyGeneTuple(typeof(float), gameManager.lifetime, 2, (float)genes["maturingTime"].Item1);
            genes["color"] = CreateMyGeneTuple(typeof(Color), new MinMaxRange(0, 1), 2);
            genes["size"] = CreateMyGeneTuple(typeof(float), gameManager.sulliteSize, 2, 0.3f*scale);

            genes["immunity"] = CreateMyGeneTuple(typeof(float), gameManager.immunity, 2);
            genes["recoveryTime"] = CreateMyGeneTuple(typeof(float), gameManager.recoveryTime, 2);

            genes["XYchromosomes"] = (GetRandomStat(0,1)==1)?(0,1,0,1,null):(1,0,0,1,null);

            foreach (var disease in geneticDiseases) {
                genes[disease.Key] = (GetRandomStat(0,3)==1)?(0, 1, 1, 0, null):(0, 1, 0, 1, null);
            }
        } else {
            genes = newGenes;
        }

        foreach (var kvp in genes) {
            stats[kvp.Key] = kvp.Value.Item1;
        }
        stats["size"] = (float)genes["size"].Item1 / (float)genes["maturingTime"].Item1;
        stats["endSize"] = (float)genes["size"].Item1;
    }

    private void GiveSulliteDiseases()
    {
        GiveGeneticDisease();
        GiveRandomDisease();

    }

    private void GiveGeneticDisease()
    {
        foreach (var kvp in genes) {
            string gene = kvp.Key;
            if (genes[gene].Item1 is int geneValue && geneticDiseases.ContainsKey(gene) && geneValue == 1) {
                List<(string, float)> geneChanges = geneticDiseases[gene];
                Debug.Log($"{sulliteName} has the genetic disease: {gene}");
                SetDiseaseStats(geneChanges, true);
            }
        }
    }

    private void SetDiseaseStats(List<(string, float)> geneChanges, bool isGenetic = false)
    {
        for (int i = 0; i < geneChanges.Count; i++) {
            if (stats[geneChanges[i].Item1] is float) {
                stats[geneChanges[i].Item1] = (float)((float)stats[geneChanges[i].Item1]*geneChanges[i].Item2);
            } else if (stats[geneChanges[i].Item1] is int) {
                stats[geneChanges[i].Item1] = (int)Math.Round((int)stats[geneChanges[i].Item1]*geneChanges[i].Item2);
            } else if (stats[geneChanges[i].Item1] is Color && geneChanges[i].Item2 == 0f) {
                stats[geneChanges[i].Item1] = new Color(1, 1, 1);
            } else if (stats[geneChanges[i].Item1] is Color sulliteColor && geneChanges[i].Item2 is float geneChange) {
                stats[geneChanges[i].Item1] = new Color(sulliteColor.r*geneChange, sulliteColor.g*geneChange, sulliteColor.b*geneChange);
            }
        }
    }

    public void GiveRandomDisease()
    {
        bool giveDisease = (GetRandomStat(0,(int)Math.Round(10000*(float)stats["immunity"]*Time.deltaTime)) == 1);
        if (giveDisease && sulliteDisease == SulliteDisease.None) {
            sulliteDisease = (SulliteDisease)UnityEngine.Random.Range(1, Enum.GetValues(typeof(SulliteDisease)).Length);
            SetDiseaseStats(infectiousDiseases[sulliteDisease.ToString()]);
            lastSick = Time.time;
            Debug.Log($"{sulliteName} contracted the disease {sulliteDisease.ToString()!}");
        }
    }

    public void CheckForDisease(Food food, Sullite sullite)
    {
        CheckFoodDisease(food);
        CheckSulliteDisease(sullite);

        if ((foodDisease != FoodInfos.FoodDisease.None || sulliteDisease != SulliteDisease.None) && Time.time - lastSick > (float)stats["recoveryTime"]) {
            Debug.Log($"{sulliteName} healed from {sulliteDisease} and/or {foodDisease}!");
            foodDisease = FoodInfos.FoodDisease.None;
            sulliteDisease = SulliteDisease.None;
            stats["immunity"] = (float)stats["immunity"] + (float)genes["immunity"].Item1;
        }
    }

    private void CheckFoodDisease(Food food)
    {
        if (foodName != null && food.foodInfosDict.ContainsKey(foodName)
        && food.foodInfosDict[foodName].foodDisease != FoodInfos.FoodDisease.None && foodDisease==FoodInfos.FoodDisease.None 
        && GetRandomStat(0,(int)Math.Round(1+2*(float)stats["immunity"])) == 1) {
            foodDisease = food.foodInfosDict[foodName].foodDisease;
            SetDiseaseStats(infectiousDiseases[foodDisease.ToString()]);
            lastSick = Time.time;
            Debug.Log($"{sulliteName} got the disease {foodDisease}!");
        }
    }

    private void CheckSulliteDisease(Sullite sullite)
    {
        for (int i = 0; i < 360; i++)
        {
            float raycastAngleRad = Mathf.Deg2Rad * (i);
            Vector2 raycastDirection = new Vector2(Mathf.Cos(raycastAngleRad), Mathf.Sin(raycastAngleRad));
            RaycastHit2D hit = Physics2D.Raycast(transformSullite.position, raycastDirection, gameManager.touchingRaycastDistance * 5 * (float)stats["size"], gameManager.sulliteLayer);
            //Debug.DrawRay(transformSullite.position, raycastDirection * gameManager.touchingRaycastDistance * 5 * (float)stats["size"], Color.blue);

            if (hit.collider != null && hit.collider.gameObject != sulliteGameObject)
            {
                if (sullite.sulliteInfosDict[hit.collider.gameObject.name].sulliteDisease != SulliteDisease.None
                && sulliteDisease == SulliteDisease.None
                && GetRandomStat(0,(int)Math.Round(1+2*(float)stats["immunity"])) == 1) {
                    sulliteDisease = sullite.sulliteInfosDict[hit.collider.gameObject.name].sulliteDisease;
                    SetDiseaseStats(infectiousDiseases[sulliteDisease.ToString()]);
                    lastSick = Time.time;
                    Debug.Log($"{sulliteName} has the contagious disease {sulliteDisease} from {hit.collider.gameObject.name}!");
                }
            }
        }
    }

    private Dictionary<string, Color> SetColorDict(ColorMapping[] colorMappings)
    {
        Dictionary<string, Color> colorDict = new Dictionary<string, Color>();
        foreach (var colorMapping in colorMappings) {
            colorDict[colorMapping.label] = colorMapping.color;
        }
        return colorDict;
    }

    private float GetRandomStat(float minValue, float maxValue, int decimalPlace = 0)
    {
        if (decimalPlace > 0) {
            decimalPlace = (int)Math.Pow(10,decimalPlace);
            return UnityEngine.Random.Range((int)Math.Round(minValue*decimalPlace), (int)Math.Round(maxValue*decimalPlace))/(float)decimalPlace;
        } else {
            return (float)Math.Round(UnityEngine.Random.Range(minValue, maxValue+1));
        }
    }

    private (object, float, object, float, MinMaxRange) CreateMyGeneTuple(Type type, MinMaxRange gene, int decimalPlace = 0, float geneScale = 1)
    {
        MinMaxRange scaledGene = new MinMaxRange(gene.min, (int)Math.Round(gene.max*geneScale));
        float influence = GetRandomStat(0,1,3);
        float geneValue = (gameManager.useRandomStats)?GetRandomStat(scaledGene.min, scaledGene.max, decimalPlace):scaledGene.middle;
        
        if(type == typeof(int)) {
            return ((int)geneValue, influence, (int)geneValue, influence, scaledGene);
        } else if (type == typeof(float)){
            return (geneValue, influence, geneValue, influence, scaledGene);
        } else if (type == typeof(Color)) {
            float r = (gameManager.useRandomStats)?GetRandomStat(scaledGene.min, scaledGene.max, decimalPlace):scaledGene.middle;
            float g = (gameManager.useRandomStats)?GetRandomStat(scaledGene.min, scaledGene.max, decimalPlace):scaledGene.middle;
            float b = (gameManager.useRandomStats)?GetRandomStat(scaledGene.min, scaledGene.max, decimalPlace):scaledGene.middle;
            Color randomColor = new Color(r,g,b,1f);
            return (randomColor, influence, randomColor, influence, scaledGene);
        } else {
            return (geneValue, influence, geneValue, influence, scaledGene);
        }
    }

    private void CreateSulliteObject(Vector2 sullitePos)
    {
        sulliteGameObject = new GameObject(sulliteName, typeof(SpriteRenderer));
        transformSullite = sulliteGameObject.GetComponent<Transform>();
        sulliteGameObject.GetComponent<SpriteRenderer>().sprite = gameManager.sulliteSprites[0];
        sulliteGameObject.AddComponent<BoxCollider2D>();
        sulliteGameObject.transform.position = new Vector3(sullitePos.x, sullitePos.y);

        transformSullite.rotation = Quaternion.Euler(0,0,GetRandomStat(0, 360));

        stats["startSize"] = sulliteGameObject.transform.localScale;
        sulliteGameObject.transform.localScale = sulliteGameObject.transform.localScale * (float)stats["size"];
        sulliteGameObject.transform.parent = gameManager.sulliteHolder;
        for (int i = 0; i < 32; i++) {
            if ((gameManager.sulliteLayer.value & (1 << i)) != 0) {
                sulliteGameObject.layer = i;
            }
        }
        randomDirection = SetRandomDirection();
    }

    public void UpdateSulliteObject()
    {
        sulliteGameObject.transform.localScale = (Vector3)stats["startSize"] * (float)stats["size"];
        ChangeColor();
    }

    public bool CanMate(LayerMask targetLayer)
    {
        bool isTouchingMate = IsTouchingObjectOfLayer(targetLayer);

        if (isTouchingMate && sullite.sulliteInfosDict[mateName].GetType() != sullite.sulliteInfosDict[sulliteName].GetType()) {
            return false;
        }

        if (isTouchingMate && sullite.sulliteInfosDict[mateName].sex != sex) {
            return true;
        } else if (isTouchingMate && sullite.sulliteInfosDict[mateName].sex == Sex.Hermaphrodite && sex == Sex.Hermaphrodite) {
            return true;
        } else if (isTouchingMate && sullite.sulliteInfosDict[mateName].sex == Sex.Protandrous && sex == Sex.Protandrous) {
            return true;
        } else if (isTouchingMate && sullite.sulliteInfosDict[mateName].sex == Sex.Protogynous && sex == Sex.Protogynous) {
            return true;
        } else {
            return false;
        }
    }

    public bool IsNotChildCarrier(Sex sulliteSex, Sex mateSex)
    {
        return (sulliteSex == Sex.Male || sulliteSex == Sex.Protandrous || 
        (sulliteSex == Sex.Hermaphrodite && mateSex != Sex.Male && mateSex != Sex.Protandrous));
    }

    public void ChangeSex()
    {
        if ((sullite.sulliteInfosDict[mateName].sex == Sex.Male || sullite.sulliteInfosDict[mateName].sex == Sex.Protandrous) && sex == Sex.Protandrous) {
            sex = Sex.Female;
            Debug.Log($"{sulliteName} changed sex from Protandrous to Female");
        } else if ((sullite.sulliteInfosDict[mateName].sex == Sex.Female || sullite.sulliteInfosDict[mateName].sex == Sex.Protogynous) && sex == Sex.Protogynous) {
            sex = Sex.Male;
            Debug.Log($"{sulliteName} changed sex from Protogynous to Male");
        }
    }

    public Dictionary<string, (object, float, object, float, MinMaxRange)> Breed(SulliteInfos parent1, SulliteInfos parent2)
    {
        Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes = new Dictionary<string, (object, float, object, float, MinMaxRange)>();
        foreach (var kvp in parent1.genes) {
            string gene = kvp.Key;
            
            (object allele1, float geneInfluence1) = GetRandomAlleleAndInfluence(parent1.genes[gene]);
            (object allele2, float geneInfluence2) = GetRandomAlleleAndInfluence(parent2.genes[gene]);

            if (parent1.genes[gene].Item5 != null) {
                allele1 = Mutate(allele1, parent1, gene);
                allele2 = Mutate(allele2, parent2, gene);
            }

            if (geneInfluence2 > geneInfluence1) {
                newGenes[gene] = (allele2, geneInfluence2, allele1, geneInfluence1, parent1.genes[gene].Item5);
            } else {
                newGenes[gene] = (allele1, geneInfluence1, allele2, geneInfluence2, parent1.genes[gene].Item5);
            }
        }
        return newGenes;
    }

    private (object allele, float influence) GetRandomAlleleAndInfluence((object, float, object, float, MinMaxRange) geneInfo)
    {
        int randomAllele = UnityEngine.Random.Range(0, 2);
        return randomAllele == 0
            ? (geneInfo.Item1, geneInfo.Item2)
            : (geneInfo.Item3, geneInfo.Item4); 
    }

    private float RandomMutation()
    {
        float mutation = GetRandomStat(2/3f,3/2f,4);
        int randomNumber = (int)GetRandomStat(0,15);
        if (randomNumber==1) {
            Debug.Log($"A random mutation occured! The factor was: {mutation}");
            return mutation;
        } else {
            return 1f;
        }
    }

    private object Mutate(object allele, SulliteInfos parent, string gene)
    {
        object mutatedAllele = allele;
        if (allele is int intAllele) {
            mutatedAllele = Mathf.Clamp((int)Math.Round(intAllele*RandomMutation()), parent.genes[gene].Item5.min, parent.genes[gene].Item5.max);
        } else if (allele is float floatAllele) {
            mutatedAllele = Mathf.Clamp((float)floatAllele*RandomMutation(), parent.genes[gene].Item5.min, parent.genes[gene].Item5.max);
        } else if (allele is Color colorAllele) {
            float r = Mathf.Clamp(colorAllele.r * RandomMutation(), 0f, 1f);
            float g = Mathf.Clamp(colorAllele.g * RandomMutation(), 0f, 1f);
            float b = Mathf.Clamp(colorAllele.b * RandomMutation(), 0f, 1f);
            mutatedAllele = new Color(r, g, b);
        }
        return mutatedAllele;
    }

    public bool IsTouchingObjectOfLayer(LayerMask targetLayer)
    {
        RaycastHit2D hit = Physics2D.Raycast(transformSullite.position, transformSullite.right, gameManager.touchingRaycastDistance * (float)stats["size"], targetLayer);
        //Debug.DrawRay(transformSullite.position, transformSullite.right * 1f * (float)stats["size], Color.blue);
        if (hit.collider != null  && sulliteName != hit.collider.gameObject.name) {
            if (targetLayer == gameManager.sulliteLayer && state == State.WantsToBreed) {
                mateName = hit.collider.gameObject.name;
            } else {
                foodName = hit.collider.gameObject.name;
            }
            return true;
        } else {
            if (targetLayer == gameManager.sulliteLayer && state == State.WantsToBreed) {
                mateName = null;
            } else {
                foodName = null;
            } 
            return false;
        }
    }

    public Vector3 ClosestObjectOfLayer(LayerMask targetLayer)
    {
        float raycastStartAngle = transformSullite.eulerAngles.z - (int)stats["angleRange"] / 2;

        float closestDistance = float.MaxValue;  // Initialize to a large value
        Transform closestObject = null;  // Variable to store the closest object

        for (int i = 0; i < (int)stats["angleRange"] * (int)stats["raycastPrecision"]; i++)
        {
            float raycastAngleRad = Mathf.Deg2Rad * (raycastStartAngle + i / (int)stats["raycastPrecision"]);
            Vector2 startDirection = new Vector2(Mathf.Cos(raycastAngleRad), Mathf.Sin(raycastAngleRad));

            RaycastHit2D hit = Physics2D.Raycast(transformSullite.position, startDirection, (float)stats["raycastDistance"], targetLayer);
            //Debug.DrawRay(transformSullite.position, startDirection * (float)stats["raycastDistance"], Color.red);

            if (hit.collider != null && hit.collider.gameObject != sulliteGameObject)
            {
                float distance = Vector2.Distance(transformSullite.position, hit.collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = hit.collider.transform;
                }
            }
        }

        if (closestObject != null)
        {
            return closestObject.position;
            
        } else {
            return transformSullite.position;
        }
    }

    public FoodInfos.FoodType ChooseNextFood()
    {
        float meatChance = (float)stats["diet"];
        float randomChance = GetRandomStat(0, 1, 2);
        if (randomChance>meatChance) {
            foodLayer = gameManager.plantLayer;
            return FoodInfos.FoodType.Plant;
        } else {
            foodLayer = gameManager.meatLayer;
            return FoodInfos.FoodType.Meat;
        }
    }

    public bool IsAttacking(Vector3 closestMeat, Vector3 closestSullite)
    {
        if (nextFood == FoodInfos.FoodType.Meat && closestMeat == transformSullite.position 
        && closestSullite != transformSullite.position && state != State.Attacking && state != State.WantsToBreed && state != State.Breeding) {
            state = State.Attacking;
            stats["speed"] = (float)stats["speed"]*1.5f;
            gameManager.timeScale = 1;
            Debug.Log($"{sulliteName} changed to attack");
            return true;
        } else if (state == State.Attacking && IsTouchingObjectOfLayer(gameManager.sulliteLayer)) {
            sullite.sulliteInfosDict[foodName].health -= (float)stats["attackPower"];
            Debug.Log($"{sulliteName} attacked {foodName}");
            return false;
        } else if (state == State.Attacking && (nextFood == FoodInfos.FoodType.Plant || closestMeat != transformSullite.position || closestSullite == transformSullite.position)) {
            state = State.Idle;
            stats["speed"] = (float)stats["speed"]/1.5f;
            Debug.Log($"{sulliteName} changed to Idle");
            return false;
        }
        return false;
    }

    public void UpdatePosition(Vector2 newPos, bool drift = false) 
    {
        float newSpeed = (drift)? gameManager.sulliteSpeed.max/20f : (float)stats["speed"];
        Vector2 sullitePos = transformSullite.position;
        Vector3 direction = (newPos - sullitePos).normalized;
        float distanceFromTarget = Vector2.Distance((Vector2)sullitePos, newPos);

        if (distanceFromTarget < (float)stats["raycastDistance"]/10f && !drift) {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transformSullite.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            transformSullite.Translate(Vector2.right * newSpeed * Time.deltaTime);
        } else if (!drift) {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transformSullite.rotation = Quaternion.RotateTowards(transformSullite.rotation, targetRotation, (float)stats["rotationSpeed"] * Time.deltaTime);
            transformSullite.Translate(Vector2.right * newSpeed * Time.deltaTime);
        } else {
            transformSullite.position += (direction * newSpeed * Time.deltaTime);
        }

        Debug.DrawRay(sullitePos, direction * Vector2.Distance((Vector2)sullitePos, newPos), Color.blue);
    }

    public Vector2 SetRandomDirection()
    {
        int angleRange = (int)stats["angleRange"];
        float randomAngle = GetRandomStat(-angleRange/ 2, angleRange / 2, 2);
        Vector2 localRight = transformSullite.TransformDirection(Vector2.right);
        Vector2 newRandomDirection = Quaternion.Euler(0, 0, randomAngle) * localRight;
        float randomDistance = UnityEngine.Random.Range(0, (float)stats["raycastDistance"]/3+1);
        newRandomDirection *= randomDistance;
        newRandomDirection += (Vector2)transformSullite.position;
        newRandomDirection += IsOutsideOfBounds(transformSullite.position)*randomDistance;
        return newRandomDirection;
    }

    private Vector2 IsOutsideOfBounds(Vector2 position)
    {
        Vector2 newPos = Vector2.zero;
        if (position.x > gameManager.mapSize/2) {
            newPos.x -=1 ;
        } else if (position.x < -gameManager.mapSize/2) {
            newPos.x += 1;
        } else if (position.y > gameManager.mapSize/2) {
            newPos.y -= 1;
        } else if (position.y < -gameManager.mapSize/2) {
            newPos.y += 1;
        } else {
            return Vector2.zero;
        }
        return newPos;
    }

    public void ChangeColor()
    {
        if (gameManager.colorChoice == GameManager.ColorChoice.SulliteColor) {
            sulliteGameObject.GetComponent<SpriteRenderer>().color = (Color)stats["color"];
        } else if (gameManager.colorChoice == GameManager.ColorChoice.Sex) {
            sulliteGameObject.GetComponent<SpriteRenderer>().color = sexColors[sex.ToString()];
        } else if (gameManager.colorChoice == GameManager.ColorChoice.State) {
            sulliteGameObject.GetComponent<SpriteRenderer>().color = stateColors[state.ToString()];
        } else if (gameManager.colorChoice == GameManager.ColorChoice.Species) {
            sulliteGameObject.GetComponent<SpriteRenderer>().color = speciescolors[this.GetType().Name];
        } else if (gameManager.colorChoice == GameManager.ColorChoice.Diseases) {
            sulliteGameObject.GetComponent<SpriteRenderer>().color = (foodDisease!=FoodInfos.FoodDisease.None||sulliteDisease!=SulliteDisease.None)?diseaseColors["Disease"]:diseaseColors["None"];
        } else if (gameManager.colorChoice == GameManager.ColorChoice.Diet) {
            sulliteGameObject.GetComponent<SpriteRenderer>().color = Color.Lerp(dietColors["herbivore"], dietColors["carnivore"], (float)stats["diet"]);
        }
    }
}

public interface ISpecies
{
    //
}

public interface ISex
{
    //
}

public class MammalSullite : SulliteInfos
{
    // Mammal-specific properties and methods
    public MammalSullite(GameManager gameManager, Sullite sullite, Vector2 sullitePos, Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes)
        : base(gameManager, sullite, sullitePos, newGenes)
    {
        sex = ((int)genes["XYchromosomes"].Item1==1)?Sex.Male:Sex.Female;
    }
}

public class HermaphroditeSullite : SulliteInfos
{
    // Non-mammal-specific properties and methods
    public HermaphroditeSullite(GameManager gameManager, Sullite sullite, Vector2 sullitePos, Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes)
        : base(gameManager, sullite, sullitePos, newGenes)
    {
        sex = Sex.Hermaphrodite;
    }
}

public class ProtandrousSullite : SulliteInfos
{
    // Non-mammal-specific properties and methods
    public ProtandrousSullite(GameManager gameManager, Sullite sullite, Vector2 sullitePos, Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes)
        : base(gameManager, sullite, sullitePos, newGenes)
    {
        sex = Sex.Protandrous;
    }
}

public class ProtogynousSullite : SulliteInfos
{
    // Non-mammal-specific properties and methods
    public ProtogynousSullite(GameManager gameManager, Sullite sullite, Vector2 sullitePos, Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes)
        : base(gameManager, sullite, sullitePos, newGenes)
    {
        sex = Sex.Protogynous;
    }
}

public class MaleSullite : MammalSullite
{
    public MaleSullite(GameManager gameManager, Sullite sullite, Vector2 sullitePos, Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes)
        : base(gameManager, sullite, sullitePos, newGenes)
    {
        // Male-specific initialization
    }
}

public class FemaleSullite : MammalSullite
{
    // Female-specific properties and methods
    public FemaleSullite(GameManager gameManager, Sullite sullite, Vector2 sullitePos, Dictionary<string, (object, float, object, float, MinMaxRange)> newGenes)
        : base(gameManager, sullite, sullitePos, newGenes)
    {
        // Female-specific initialization
    }
}