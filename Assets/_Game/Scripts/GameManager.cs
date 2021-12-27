using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;


public enum ENEMY_STATE
{
    IDLE,
    ALERT,
    EXPLORE,
    PATROL,
    FOLLOW,
    FURY,
    DIE
}

public class GameManager : MonoBehaviour
{

    [Header("info player")]
    [ SerializeField] private int amountOfGems;
    public Transform player;

    [Header("Slime IA")]
    public Transform[] slimeWayPoints;
    public float slimeIdleWaitTime = 5f;
    public float slimeDistanceToAttack = 2.3f;
    public float slimeAlertTime = 2.3f;
    public float slimeAttackDelay = 1f;
    public float slimeLookAtSpeed = 1f;

    [Header("Rain manager")]
    public PostProcessVolume processVolumeB;
    public ParticleSystem rainParticle;
    public int rainRateOvertime;
    public int rainIncrement;
    public float rainIncrementDelay;

    [Header("UI")]
    [SerializeField] private Text gemsText;

    [Header("Drop item")]
    public GameObject gemPrefab;
    [Range(0,100)]
    public int percentDrop = 25;

    private ParticleSystem.EmissionModule rainModule;

    private void Start()
    {
        rainModule = rainParticle.emission;
    }
    public void OnOffRain(bool isRain)
    {
        StopCoroutine(nameof(RainManager));
        StopCoroutine(nameof(PostBManager));

        StartCoroutine(nameof(RainManager), isRain);
        StartCoroutine(nameof(PostBManager), isRain);
    }

    IEnumerator RainManager(bool isRain)
    {
        switch (isRain)
        {
            case true: // aumentar chuva
                for (float r = rainModule.rateOverTime.constant; r < rainRateOvertime; r += rainIncrement)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }
                rainModule.rateOverTime = rainRateOvertime;
                break;
            case false: // diminuir chuva
                for (float r = rainModule.rateOverTime.constant; r > 0; r -= rainIncrement)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }

                rainModule.rateOverTime = 0;
                break;

        }
    }

    IEnumerator PostBManager(bool isRain)
    {
        switch (isRain)
        {
            case true:
                for (float w = processVolumeB.weight; w < 1; w += 1 * Time.deltaTime)
                {
                    processVolumeB.weight = w;
                    yield return new WaitForEndOfFrame();
                }
                processVolumeB.weight = 1;
                break;
            case false:
                for (float w = processVolumeB.weight; w > 0; w -= 1 * Time.deltaTime)
                {
                    processVolumeB.weight = w;
                    yield return new WaitForEndOfFrame();
                }

                processVolumeB.weight = 0;
                break;
        }
    }

    public void SetGem(int amount)
    {
        amountOfGems += amount;
        gemsText.text = amountOfGems.ToString();
    }
}
