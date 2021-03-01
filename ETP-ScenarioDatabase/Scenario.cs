using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A scenario object.
/// </summary>
[CreateAssetMenu(fileName = "Scenario", menuName = "ScriptableObjects/Scenario", order = 2)]
public class Scenario : ScriptableObject
{
    [Tooltip("ID of scenario (pulled from name)")]
    public int id;
    [Tooltip("String name of the scenario")]
    public string scenarioName;
    [TextArea(3, 10)][Tooltip("Block of text for the description (bulk of scenario)")]
    public string description;

    [Header("Scenario Conditions")]
    public List<ScenarioEvent> preConditions;
    public List<ScenarioEvent> postConditions;

    [Header("Prompts")]
    [Tooltip("List of possible options")]
    public List<Prompt> prompts;
}

[System.Serializable]
public class Prompt
{
    public string name;
    public string stylizedName;

    [Header("Outcome Stuff")]
    public string methodName = "nothing";
    public int manaCost = 0;
}

/// <summary>
/// Event executed either before or after battle phase
/// </summary>
[System.Serializable]
public class ScenarioEvent
{
    [HideInInspector]
    public string name;

    [Tooltip("Party member condition requirements")]
    public EntityStatusEffect partyCondition;

    [Tooltip("Location condition requirements")]
    public LocationStatusEffect locationCondition;

    [Tooltip("Method triggered by event")]
    public string eventMethodName = "nothing";

    [Tooltip("Text to be shown if triggered\nIf none, leave blank")]
    [TextArea(3, 7)]
    public string description;

    private void OnValidate()
    {
        if (partyCondition != EntityStatusEffect.None) name = partyCondition.ToString() + " party member";
        else if (locationCondition != LocationStatusEffect.None) name = locationCondition.ToString() + " at location";
        else name = "Default Condition";
    }
}