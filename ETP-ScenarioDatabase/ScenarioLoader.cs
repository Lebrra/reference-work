using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScenarioLoader : MonoBehaviour
{
    public static ScenarioLoader inst;

    [Header("Scenario UI")]
    public GameObject myWindow;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI warning;
    public PromptButton[] buttons;

    [Header("Pre-Battle UI")]
    public GameObject preBattleWindow;
    public List<string> preMessages;
    public GameObject messageHolder;
    public GameObject messagePref;

    [Header("Battle Stuff")]
    public GameObject battleUI;
    public GameObject[] partySpawnpoints;
    public GameObject target;
    public GameObject[] party;

    public GameObject tutorialWindow;
    public GameObject tutorialSkip;

    [Header("Post-Battle UI")]
    public GameObject postBattleWindow;
    public List<string> postMessages;
    public GameObject postMessageHolder;
    List<Action> postActions;

    List<PartyMember> poisonedMembers;

    private void Start()
    {
        Time.timeScale = 1;

        if (inst) Destroy(inst.gameObject);
        else inst = this;

        if (!GameManager.instance)
        {
            Debug.LogError("No GameManager found. Unable to load.");
            Destroy(this);
        }

        GameManager.instance.activeScenarioLoader = this;
        Scenario data = GameManager.instance.GetActiveScenario();

        preMessages = new List<string>();
        postMessages = new List<string>();

        SpawnParty();

        title.text = data.scenarioName;
        description.text = data.description;

        //check for pre-conditions
        if(data.preConditions.Count > 0)
        {
            bool warningTextLoaded = false;
            foreach(ScenarioEvent a in data.preConditions)
            {
                Action myMethod = MethodsMasterList.instance.masterList[a.eventMethodName];
                if (myMethod == null) Debug.LogError("no method found", gameObject);
                else if (a.eventMethodName == "nothing") Debug.LogWarning("'nothing' method loaded, this action will not do anything", gameObject);
                else
                {
                    if (!warningTextLoaded)
                    {
                        if (a.partyCondition != EntityStatusEffect.None)
                        {
                            foreach(GameObject partyM in party)
                            {
                                if (partyM.GetComponent<PartyMember>().statusEffects.Contains(a.partyCondition))
                                {
                                    warning.gameObject.SetActive(true);
                                    warning.text = a.description;
                                    warningTextLoaded = true;
                                    break;
                                }
                            }
                        }
                        else if (a.locationCondition != LocationStatusEffect.None)
                        {
                            if (GameManager.instance.activeLocationEffects.Contains(a.locationCondition))
                            {
                                warning.gameObject.SetActive(true);
                                warning.text = a.description;
                                warningTextLoaded = true;
                            }
                        }
                        else if (GameManager.instance.tutorial)
                        {
                            warning.gameObject.SetActive(true);
                            warning.text = a.description;
                            warningTextLoaded = true;
                        }
                    }

                    myMethod();
                }
            }
        }

        //check for post-conditions
        if (data.postConditions.Count > 0)
        {
            postActions = new List<Action>();

            foreach (ScenarioEvent a in data.postConditions)
            {
                Action myMethod = MethodsMasterList.instance.masterList[a.eventMethodName];
                if (myMethod == null) Debug.LogError("no method found", gameObject);
                else if (a.eventMethodName == "nothing") Debug.LogWarning("'nothing' method loaded, this action will not do anything", gameObject);
                else postActions.Add(myMethod);
            }
        }

        for(int i = 0; i < data.prompts.Count; i++)
        {
            buttons[i].gameObject.SetActive(true);
            buttons[i].InputData(data.prompts[i].stylizedName, data.prompts[i].methodName, data.prompts[i].manaCost);
        }
    }

    public void StartBattle()
    {
        foreach(Entity a in FindObjectsOfType<Entity>())
        {
            a.canMove = true;
        }
        foreach(PartyMember p in poisonedMembers)
        {
            StartCoroutine(SpellPool.pool.PoisonTarget(p, 3F));
        }
    }

    void SpawnParty()
    {
        party = new GameObject[4];
        poisonedMembers = new List<PartyMember>();

        for (int i = 0; i < 4; i++)  // compensate for death later
        {
            party[i] = Instantiate(GameDataManager.instance.GetPartyMemeber(GameManager.instance.partyMembers[i].partyID), partySpawnpoints[i].transform.position, partySpawnpoints[i].transform.rotation);
            //party[i].name = party[i].GetComponent<PartyMember>().GetType() + " " + (i + 1).ToString();
            party[i].name = GameManager.instance.partyMembers[i].name + " the " + party[i].GetComponent<PartyMember>().GetType();

            party[i].GetComponent<PartyMember>().UpdateLife(GameManager.instance.partyMembers[i].health, GameManager.instance.partyMembers[i].healthStock);

            party[i].GetComponent<PartyMember>().statusEffects = GameManager.instance.partyMembers[i].statusEffects;
            if (party[i].GetComponent<PartyMember>().statusEffects.Contains(EntityStatusEffect.Poisoned))
            {
                poisonedMembers.Add(party[i].GetComponent<PartyMember>());
            }

            party[i].GetComponent<PartyMember>().startTarget = target.transform;
            party[i].GetComponent<PartyMember>().currTarget = target.transform;

            if (GameManager.instance.partyMembers[i].dead)
            {
                party[i].GetComponent<PartyMember>().dead = GameManager.instance.partyMembers[i].dead;
                party[i].GetComponent<PartyMember>().InstantSuccess();
                party[i].SetActive(false);
            }
        }
    }

    public void OpenPreBattle()
    {
        myWindow.SetActive(false);

        preBattleWindow.SetActive(true);

        if (preMessages.Count == 0)
        {
            GameObject text = Instantiate(messagePref, messageHolder.transform);
            text.GetComponent<TextMeshProUGUI>().text = "No battle messages to report.";
        }
        else
        {
            foreach (string m in preMessages)
            {
                GameObject text = Instantiate(messagePref, messageHolder.transform);
                text.GetComponent<TextMeshProUGUI>().text = m;
            }
        }
    }

    public void OpenBattle()
    {
        preBattleWindow.SetActive(false);
        battleUI.SetActive(true);

        if (GameManager.instance.currentScenarioIndex == -1) tutorialSkip.SetActive(true);
        if (GameManager.instance.tutorial) tutorialWindow.SetActive(true);

        AbilityLoader.inst.EnableAbilities();
        foreach (GameObject p in party) p.GetComponent<PartyMember>().abilityCooldown = p.GetComponent<PartyMember>().abilityCD;

        GameManager.instance.inBattle = true;
        Invoke("StartBattle", 2);
    }

    public void OpenPostBattle()
    {
        foreach(PartyMember p in poisonedMembers)
        {
            p.UseStatusEffect(EntityStatusEffect.Poisoned);
            postMessages.Add("<i>" + p.gameObject.name + " is no longer <#81B366><b>poisoned</b></color>.</i>");
        }

        if (postActions != null)
        {
            foreach (Action a in postActions) a();
        }

        battleUI.SetActive(false);

        PlayerStats.instance.IncreaseMana();
        postBattleWindow.SetActive(true);

        postMessages.Add("The party has completed this scenario.");

        if(GameManager.instance.currentScenarioIndex == GameManager.instance.outdoorCount - 1)
            postMessages.Add("<#352741><i>The party has entered your tower...</i></color>");

        foreach (string m in postMessages)
        {
            GameObject text = Instantiate(messagePref, postMessageHolder.transform);
            text.GetComponent<TextMeshProUGUI>().text = m;
        }
    }

    public void NextScene()
    {
        GameManager.instance.OpenSceneScenario();
    }

    public void PartyHeal(int amount)
    {
        if (amount > 0)
        {
            foreach (GameObject a in party)
            {
                if (!a.GetComponent<PartyMember>().dead)
                {
                    a.GetComponent<PartyMember>().health += amount;
                    if (a.GetComponent<PartyMember>().health > a.GetComponent<PartyMember>().maxHealth) a.GetComponent<PartyMember>().health = a.GetComponent<PartyMember>().maxHealth;
                }
            }
        }
    }

    public int ChooseRandomPartyMember()
    {
        int mem = UnityEngine.Random.Range(0, party.Length);
        while(party[mem].GetComponent<PartyMember>().dead)
        {
            mem = UnityEngine.Random.Range(0, party.Length);
        }

        return mem;
    }
        
        // this is only for the tutorial scenario
    public void SkipBattle()
    {
        foreach(GameObject a in party)
        {
            a.GetComponent<PartyMember>().InstantSuccess();
            //a.SetActive(false);
        }

        GameManager.instance.SkipBattleTutorial();
    }

    public IEnumerator DelayedDamage(int targetIndex, int damage)
    {
        yield return new WaitForSeconds(0.2F);

        party[targetIndex].GetComponent<PartyMember>().TakeDamage(damage);
    }

    public IEnumerator DelayedDamage(int targetIndex, float percentDamage)
    {
        yield return new WaitForSeconds(0.2F);

        party[targetIndex].GetComponent<PartyMember>().TakeDamage(Mathf.RoundToInt(party[targetIndex].GetComponent<PartyMember>().health * percentDamage));
    }

    public IEnumerator DelayedDamage(GameObject partymem, float percentDamage)
    {
        yield return new WaitForSeconds(0.2F);

        partymem.GetComponent<PartyMember>().TakeDamage(Mathf.RoundToInt(partymem.GetComponent<PartyMember>().health * percentDamage));
    }
}
