//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all backend data with properties of loading/saving/seed construction 
/// </summary>
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;

    [Header("Default Data")]
    public List<Scenario> allScenarios;
    [Tooltip("0## - reserved\n===\n1## - Forest\n2## - Sea\n3## - Farmlands\n4## - Town\n===\n5## - Kitchen\n6## - Library\n7## - Dungeon\n8## - Dormitory\n9## - Default Indoors")]
    public int[] totalScenariosPerIndex;
    public GameObject[] partyMembersPrefabs;
    char[] requiredLetters = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' }; //add as needed

    string[] nameGeneration = {"Reginald", "Donavan", "Ester", "Xander", "Siegfried", "Cassie", "Leah", "Sydney", "Alyssa", "Dresh", "Wesley", "Daphne", "Rebecca", "Margaret", "Sophia", "Linda", "Ruth", "Jerome", "Carl" };

    Dictionary<int, Scenario> scenarioMasterList;
    Dictionary<char, GameObject> partyMemberList;

    void Start()
    {
        scenarioMasterList = new Dictionary<int, Scenario>();
        foreach (Scenario a in allScenarios) scenarioMasterList.Add(a.id, a);
        Debug.Log(scenarioMasterList.Count + " scenarios loaded successfully.");

        partyMemberList = new Dictionary<char, GameObject>();
        for (int i = 0; i < partyMembersPrefabs.Length; i++) partyMemberList.Add(requiredLetters[i], partyMembersPrefabs[i]);
        Debug.Log(partyMemberList.Count + " party members loaded successfully.");

            // Seed Testing:
        //Debug.Log("generating seed...");
        //activeSeed = BuildSeed();
        //Debug.Log("Seed Generation Completed. Seed: '" + activeSeed + "'");
    }



    #region Seed Generation

    public string BuildSeed()       // current seed: _ _ _ _ = party members [char] + _ _ _ = room [int], three room example: abcd111222333
    {
        Scenario[] activeGameSequence = new Scenario[GameManager.instance.outdoorCount + GameManager.instance.indoorCount];
        GameManager.instance.currentScenarioIndex = -1;

        // Generate Party:

        List<char> partyList = new List<char>();

        int firstMember = Random.Range(0, partyMembersPrefabs.Length - 1);
        partyList.Add(requiredLetters[firstMember]);
        for (int i = 0; i < 3; i++)
        {
            while (partyList.Contains(requiredLetters[firstMember]))
            {
                firstMember = Random.Range(0, partyMembersPrefabs.Length - 1);
            }
            partyList.Add(requiredLetters[firstMember]);
        }

        string[] partyNames = GenerateNamesList();
        string party = "";
        PartyMemberStat[] partyMembers = new PartyMemberStat[4];
        for (int i = 0; i < 4; i++)
        {
            //get another random party member *unique
            partyMembers[i] = new PartyMemberStat();
            partyMembers[i].name = partyNames[i];
            partyMembers[i].partyID = partyList[i];
            partyMembers[i].health = partyMemberList[partyList[i]].GetComponent<PartyMember>().health;
            party += partyList[i];
        }

        GameManager.instance.partyMembers = partyMembers;

        // Generate Scenarios:

        List<int> roomList = new List<int>();

        int prevLocation = Random.Range(1, 5);
        int room = Random.Range(0, totalScenariosPerIndex[prevLocation]);

        // start on boat, not beach
        bool startOnBoat = false;
        if (prevLocation == 2)
        {
            startOnBoat = Random.Range(0, 10) < 5;

            if (startOnBoat)
            {
                Debug.Log("I'm starting on the boat!");
                room = Random.Range(50, totalScenariosPerIndex[10] + 50);
                startOnBoat = true;
            }
        }

        roomList.Add((prevLocation * 100) + room);

        //OUTSIDE
        for (int i = 0; i < GameManager.instance.outdoorCount - 1; i++)
        {
            bool stayOnBoat = false;
            if(prevLocation == 2 && startOnBoat)
            {
                // keep going on the boat, unless there are no more scenarios
                stayOnBoat = Random.Range(0, 10) < 5;

                if (stayOnBoat)
                {
                    room = Random.Range(50, totalScenariosPerIndex[10] + 50);
                    int checkForLim = 0;
                    while (roomList.Contains((prevLocation * 100) + room))
                    {
                        checkForLim++;
                        if (checkForLim > 10)
                        {
                            //stop trying to stay on the boat
                            stayOnBoat = false;
                            break;
                        }

                        room = Random.Range(50, totalScenariosPerIndex[10] + 50);
                        roomList.Add((prevLocation * 100) + room);
                        break;
                    }
                    if (stayOnBoat) continue;
                }
            }

            if (startOnBoat) startOnBoat = false;

            int nextLocation = Random.Range(1, 10);
            if (nextLocation > 4) nextLocation = prevLocation;
            room = Random.Range(0, totalScenariosPerIndex[nextLocation]);

            // if location chosen has no more scenarios to load
            int checkForLimit = 0;
            while (roomList.Contains((nextLocation * 100) + room))
            {
                checkForLimit++;
                if (checkForLimit > 10)
                {
                    //pick a new location
                    nextLocation = Random.Range(1, 5);
                    checkForLimit = 0;
                }

                room = Random.Range(0, totalScenariosPerIndex[nextLocation]);
            }
            roomList.Add((nextLocation * 100) + room);
            prevLocation = nextLocation;
        }

        //INSIDE
        prevLocation = Random.Range(5, 10);       //<- UPDATE UNTIL ALL INDOOR LOCATIONS HAVE A SCENARIO

        for (int i = 0; i < GameManager.instance.indoorCount; i++)
        {
            int nextLocation = Random.Range(1, 10);
            if (nextLocation < 5) nextLocation = prevLocation;
            room = Random.Range(0, totalScenariosPerIndex[nextLocation]);

            // if location chosen has no more scenarios to load
            int checkForLimit = 0;
            while (roomList.Contains((nextLocation * 100) + room))
            {
                checkForLimit++;
                if (checkForLimit > 3)          // UPDATE THIS TOO WITH LARGER POOL
                {
                    //pick a new location
                    nextLocation = Random.Range(5, 10);
                    checkForLimit = 0;
                }

                room = Random.Range(0, totalScenariosPerIndex[nextLocation]);
            }
            roomList.Add((nextLocation * 100) + room);
            prevLocation = nextLocation;
        }

        string newSeed = "";
        for (int a = 0; a < roomList.Count; a++)
        {
            if (scenarioMasterList.ContainsKey(roomList[a]))
            {
                activeGameSequence[a] = scenarioMasterList[roomList[a]];
                newSeed = newSeed + roomList[a].ToString();
            }
            else
            {
                Debug.LogError("Error building seed, scenario " + roomList[a] + " not found.");
            }
        }

        GameManager.instance.activeLocationEffects = new List<LocationStatusEffect>();

        GameManager.instance.activeGameSequence = activeGameSequence;

        return party + newSeed;
    } 

    public bool BuildFromSeed(string seed)   //loading just seed, NOT save data
    {
        GameManager.instance.currentScenarioIndex = -1;                                                              // <- different if loading save
        
        int holdIndoor = GameManager.instance.indoorCount;
        int holdOutdoor = GameManager.instance.outdoorCount;
        GameManager.instance.outdoorCount = 0;
        GameManager.instance.indoorCount = 0;

        PartyMemberStat[] partyMembers = new PartyMemberStat[4];


            // Check for seed validility
        if (seed.Length < 4 || (seed.Length - 4) % 3 != 0) //|| (seed.Length - 4) / 3 != GameManager.instance.scenarioCount
        {
            GameManager.instance.outdoorCount = holdOutdoor;
            GameManager.instance.indoorCount = holdIndoor;
            Debug.LogWarning("Invalid seed.");
            return false;
        }

        // still generate random names
        string[] partyNames = GenerateNamesList();
            
            // Load Party
        for(int i = 0; i < 4; i++)
        {
            if (partyMemberList.ContainsKey(seed[0]))
            {
                partyMembers[i] = new PartyMemberStat();
                partyMembers[i].name = partyNames[i];
                partyMembers[i].partyID = seed[0];
                partyMembers[i].health = partyMemberList[seed[0]].GetComponent<PartyMember>().health;          // <- different if loading save + status effects

                seed = seed.Substring(1);   // removes first letter
            }
            else
            {
                Debug.LogWarning("Error loading party member from save; char = " + seed[0]);
                GameManager.instance.outdoorCount = holdOutdoor;
                GameManager.instance.indoorCount = holdIndoor;
                return false;
            }
        }

        GameManager.instance.partyMembers = partyMembers;

            // Load Scenarios
        List<int> keys = new List<int>();
        for(int i = 0; i < seed.Length; i += 3)
        {
            string keyString = seed[i].ToString() + seed[i + 1].ToString() + seed[i + 2].ToString();
            int key;
            if (int.TryParse(keyString, out key)) keys.Add(key);
            else
            {
                Debug.LogWarning("Error loading scenario ID. Tried to create ID: " + keyString);
                GameManager.instance.outdoorCount = holdOutdoor;
                GameManager.instance.indoorCount = holdIndoor;
                return false;
            }
        }

        //Scenario[] activeGameSequence = new Scenario[GameManager.instance.outdoorCount + GameManager.instance.indoorCount];
        List<Scenario> gameSequence = new List<Scenario>();

        for (int i = 0; i < keys.Count; i++)
        {
            if (scenarioMasterList.ContainsKey(keys[i]))
            {
                gameSequence.Add(scenarioMasterList[keys[i]]);
                //activeGameSequence[i] = scenarioMasterList[keys[i]];

                if (keys[i] / 100 > 4) GameManager.instance.indoorCount++;
                else GameManager.instance.outdoorCount++;
            }
            else
            {
                Debug.LogWarning("Error finding scenario by ID number. Looking for ID: " + keys[i]);
                GameManager.instance.outdoorCount = holdOutdoor;
                GameManager.instance.indoorCount = holdIndoor;
                return false;
            }
        }

        Scenario[] activeGameSequence = new Scenario[GameManager.instance.outdoorCount + GameManager.instance.indoorCount];
        for (int i = 0; i < gameSequence.Count; i++)
        {
            activeGameSequence[i] = gameSequence[i];
        }

        GameManager.instance.activeLocationEffects = new List<LocationStatusEffect>();

        GameManager.instance.activeGameSequence = activeGameSequence;

        Debug.Log("Seed loaded successfully");
        return true;
    }

    public void LoadSeedFromSave(string seed)   // will probably need a GameData object parameter
    {
        //GameManager.instance.currentScenarioIndex = 0;                                                         // <- load current scenario index from save

            // Load Party
        PartyMemberStat[] partyMembers = new PartyMemberStat[4];

        for (int i = 0; i < 4; i++)
        {
            if (partyMemberList.ContainsKey(seed[0]))
            {
                partyMembers[i] = new PartyMemberStat();
                partyMembers[i].partyID = seed[0];
                //partyMembers[i].health = partyMemberList[seed[0]].GetComponent<PartyMember>().health;          // <- load in health and status effects here

                seed = seed.Substring(1);
            }
            else Debug.LogError("Error loading party member from save; char = " + seed[0]);
        }

        GameManager.instance.partyMembers = partyMembers;

            // Load Scenarios
        List<Scenario> gameSequence = new List<Scenario>();
        int holdIndoor = GameManager.instance.indoorCount;
        int holdOutdoor = GameManager.instance.outdoorCount;
        GameManager.instance.outdoorCount = 0;
        GameManager.instance.indoorCount = 0;

        //Scenario[] activeGameSequence = new Scenario[GameManager.instance.outdoorCount];

        List<int> keys = new List<int>();
        for (int i = 0; i < seed.Length; i += 3)
        {
            string keyString = seed[i].ToString() + seed[i + 1].ToString() + seed[i + 2].ToString();
            int key;
            if (int.TryParse(keyString, out key)) keys.Add(key);
            else
            {
                GameManager.instance.outdoorCount = holdOutdoor;
                GameManager.instance.indoorCount = holdIndoor;
                Debug.LogError("Error loading scenario ID. Tried to find ID: " + keyString);
            }
        }

        for (int i = 0; i < keys.Count; i++)
        {
            if (scenarioMasterList.ContainsKey(keys[i]))
            {
                gameSequence.Add(scenarioMasterList[keys[i]]);
                //activeGameSequence[i] = scenarioMasterList[keys[i]];

                if (keys[i] / 100 > 4) GameManager.instance.indoorCount++;
                else GameManager.instance.outdoorCount++;
            }
            else
            {
                GameManager.instance.outdoorCount = holdOutdoor;
                GameManager.instance.indoorCount = holdIndoor;
                Debug.LogError("Error finding scenario by ID number. Looking for ID: " + keys[i]);
            }
        }

        Scenario[] activeGameSequence = new Scenario[GameManager.instance.outdoorCount + GameManager.instance.indoorCount];
        for (int i = 0; i < gameSequence.Count; i++)
        {
            activeGameSequence[i] = gameSequence[i];
        }

        GameManager.instance.activeGameSequence = activeGameSequence;

        Debug.Log("Seed loaded successfully");
    }

    string[] GenerateNamesList()
    {
        int overload = 0;
        List<string> names = new List<string>();
        for(int i = 0; i < 4; i++)
        {
            int rand = Random.Range(0, nameGeneration.Length);
            while (names.Contains(nameGeneration[rand]))
            {
                overload++;
                rand = Random.Range(0, nameGeneration.Length);
                if(overload > 100) return new string[4] {"Error1", "Error2", "Error3", "Error4" };
            }
            names.Add(nameGeneration[rand]);
        }

        return names.ToArray();
    }

    #endregion

    #region Game Functions

    public GameObject GetPartyMemeber(char index)
    {
        if (partyMemberList.ContainsKey(index)) return partyMemberList[index];
        Debug.LogError("Party member not found.");
        return null;
    }

    public char GetTamerChar()
    {
        return requiredLetters[partyMembersPrefabs.Length - 1];
    }

    #endregion

    #region Save/Load Functions

    public void SaveGameData()
    {
        bool[] zardSkills = new bool[16];
        for(int i = 0; i < 16; i++)
        {
            if (i < 8) zardSkills[i] = PlayerStats.instance.attackSpells[i];
            else zardSkills[i] = PlayerStats.instance.summonSpells[i - 8];
        }

        GameData gameData = new GameData(GameManager.instance.activeSeed, GameManager.instance.currentScenarioIndex, GameManager.instance.activeLocationEffects, PlayerStats.instance.skillPoints, zardSkills, PlayerStats.instance.quickSlotIndexes, GameManager.instance.partyMembers);
        SaveSystem.SaveGame(gameData);

        Debug.Log("Game Data saved.");
    }

    public bool LoadGameData()  // return false if no data found or error loading
    {
        GameData gameData = SaveSystem.LoadGame();

        if (gameData == null) return false;

        GameManager.instance.currentScenarioIndex = gameData.sceneIndex;
        GameManager.instance.activeSeed = gameData.seed;

            // Loading Mana
        if (gameData.sceneIndex > -1)
        {
            PlayerStats.instance.fullMana = 10 + ((gameData.sceneIndex + 1) * 2);
            PlayerStats.instance.ReloadMana();
        }
        else PlayerStats.instance.fullMana = 10;

            // Loading Spells
        PlayerStats.instance.attackSpells = new bool[8];
        PlayerStats.instance.summonSpells = new bool[8];
        PlayerStats.instance.quickSlotIndexes = new int[4];
        for (int i = 0; i < 16; i++)
        {
            if (i < 8) PlayerStats.instance.attackSpells[i] = gameData.skills[i];
            else PlayerStats.instance.summonSpells[i - 8] = gameData.skills[i];
        }
        for (int i = 0; i < 4; i++) PlayerStats.instance.quickSlotIndexes[i] = gameData.quickSlots[i];
        PlayerStats.instance.skillPoints = gameData.unusedSkillPoints;

            // Loading Party
        if (gameData.partyStats == null) return false;
        GameManager.instance.partyMembers = new PartyMemberStat[4];
        for (int i = 0; i < 4; i++)
        {
            GameManager.instance.partyMembers[i] = new PartyMemberStat(gameData.partyStats[i].name, gameData.partyStats[i].partyID, gameData.partyStats[i].health, gameData.partyStats[i].healthStock, gameData.partyStats[i].statusEffects, gameData.partyStats[i].dead);
        }

        string seed = gameData.seed;
        for (int i = 0; i < 4; i++) seed = seed.Substring(1);

            // Loading Scenarios
        List<int> keys = new List<int>();
        for (int i = 0; i < seed.Length; i += 3)
        {
            string keyString = seed[i].ToString() + seed[i + 1].ToString() + seed[i + 2].ToString();
            int key;
            if (int.TryParse(keyString, out key)) keys.Add(key);
            else
            {
                Debug.LogWarning("Error loading scenario ID. Tried to find ID: " + keyString);
                return false;
            }
        }

        List<Scenario> gameSequence = new List<Scenario>();
        int holdIndoor = GameManager.instance.indoorCount;
        int holdOutdoor = GameManager.instance.outdoorCount;
        GameManager.instance.outdoorCount = 0;
        GameManager.instance.indoorCount = 0;

        //Scenario[] activeGameSequence = new Scenario[GameManager.instance.outdoorCount];

        for (int i = 0; i < keys.Count; i++)
        {
            if (scenarioMasterList.ContainsKey(keys[i]))
            {
                gameSequence.Add(scenarioMasterList[keys[i]]);
                //activeGameSequence[i] = scenarioMasterList[keys[i]];

                if (keys[i] / 100 > 4) GameManager.instance.indoorCount++;
                else GameManager.instance.outdoorCount++;
            }
            else
            {
                GameManager.instance.outdoorCount = holdOutdoor;
                GameManager.instance.indoorCount = holdIndoor;
                Debug.LogWarning("Error finding scenario by ID number. Looking for ID: " + keys[i]);
                return false;
            }
        }

        Scenario[] activeGameSequence = new Scenario[GameManager.instance.outdoorCount + GameManager.instance.indoorCount];
        for (int i = 0; i < gameSequence.Count; i++)
        {
            activeGameSequence[i] = gameSequence[i];
        }
        GameManager.instance.activeGameSequence = activeGameSequence;

        GameManager.instance.activeLocationEffects = new List<LocationStatusEffect>();
        foreach (LocationStatusEffect l in gameData.locationEffects)
            GameManager.instance.activeLocationEffects.Add(l);

        Debug.Log("Game Data loaded successfully");
        return true;
    }

    #endregion
}






[System.Serializable]
public class PartyMemberStat
{
    public string name;
    public char partyID;
    public int health;
    public int healthStock;
    public List<EntityStatusEffect> statusEffects;
    public bool dead;

    public PartyMemberStat()
    {
        name = "";
        partyID = 'z';
        health = -1;
        healthStock = 2;
        statusEffects = new List<EntityStatusEffect>();
        dead = false;
    }

    public PartyMemberStat(string n, char id, int h, int s, List<EntityStatusEffect> effects, bool d)
    {
        name = n;
        partyID = id;
        health = h;
        healthStock = s;
        statusEffects = new List<EntityStatusEffect>();
        foreach (EntityStatusEffect e in effects) statusEffects.Add(e);
        dead = d;
    }
}