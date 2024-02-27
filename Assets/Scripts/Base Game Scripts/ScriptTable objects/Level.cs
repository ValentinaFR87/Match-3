using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="World",menuName ="Level")]
public class Level : ScriptableObject
{
    [Header("Board Dimensions")]
    public int width;
    public int height;

    [Header("Starting Tiles")]
    public TileType[] boardLayout;

    [Header("Availabel dots")]
    public GameObject[] dots;

    [Header("Score Goals")]
    public int[] scoreCoals;

    [Header("End Game Requarements")]
    public EndGameRequirements endGameRequirements;
    public BlankCoal[] levelGoals;
}
