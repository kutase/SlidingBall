using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using Random = UnityEngine.Random;

public enum Difficulty
{
    Easy,
    Medium,
    High
}

public class GameController : MonoBehaviour
{
    public List<Transform> EasyStartPoints;
    public List<Transform> MediumStartPoints;
    public List<Transform> HighStartPoints;

    public Difficulty Difficulty = Difficulty.Easy;

    public GameObject BlockPrefab;
    public GameObject DoubleBlockPrefab;
    public GameObject TripleBlockPrefab;

    public GameObject CrystalPrefab;

    public GameObject StartBlockPrefab;

    public Transform BlocksRoot;

    public float DistanceToGenerateLevel = 10f;

    public float BorderValue = 10f;

    public bool IsRandomCrystalSpawning = true;
    public float CrystalSpawnPossibility = 0.3f;

    public int BlocksGroupCount = 5;

    private int blocksCount = 0;
    private int blocksGroupsCount = 0;
    private int crystalsCounter = 0;

    private Vector3 lastGeneratePosition;

    private List<GameObject> BlocksToDelete = new List<GameObject>();

    private int crystalsCount = 0;

    private bool isGameStarted = false;

    public int CrystalsCount => crystalsCount;

    [Inject]
    private BallController ball;

    [Inject(Id = "CrystalCollectedEvent")]
    private UnityEvent crystalCollectedEvent;

    [Inject(Id = "GameOverEvent")]
    private UnityEvent gameOverEvent;

    [Inject(Id = "ResetGameEvent")]
    private UnityEvent resetGameEvent;

    [Inject(Id = "StartGameEvent")]
    private UnityEvent startGameEvent;

    [Inject(Id = "UICrystalsCountUpdateEvent")]
    private UnityEvent uiCrystalsCountUpdatedEvent;

    [Inject(Id = "UIGameOverEvent")]
    private UnityEvent uiGameOverEvent;

    [Inject(Id = "UIResetEvent")]
    private UnityEvent uiResetEvent;

    [Inject(Id = "UIStartGameEvent")]
    private UnityEvent uiStartGameEvent;

    private void Start()
    {
        crystalCollectedEvent.AddListener(OnCrystalCollected);
        gameOverEvent.AddListener(OnGameOver);
        startGameEvent.AddListener(OnStartGame);
        resetGameEvent.AddListener(() => StartCoroutine(ResetState()));

        PrepareLevel();
    }

    private GameObject GetBlockPrefab()
    {
        var blockPrefab = BlockPrefab;

        switch (Difficulty)
        {
            case Difficulty.Easy:
                blockPrefab = TripleBlockPrefab;
                break;
            
            case Difficulty.Medium:
                blockPrefab = DoubleBlockPrefab;
                break;
        }

        return blockPrefab;
    }

    private float GetBlockOffset()
    {
        switch (Difficulty)
        {
            case Difficulty.Easy:
                return 3f;
                break;
            
            case Difficulty.Medium:
                return 2f;
                break;

            case Difficulty.High:
                return 1f;
        }

        return 1f;
    }
    
    private Vector3 GetStartPoint()
    {
        switch (Difficulty)
        {
            case Difficulty.Easy:
                return EasyStartPoints[Random.Range(0, EasyStartPoints.Count)].localPosition;
            
            case Difficulty.Medium:
                return MediumStartPoints[Random.Range(0, MediumStartPoints.Count)].localPosition;

            case Difficulty.High:
                return HighStartPoints[Random.Range(0, HighStartPoints.Count)].localPosition;
        }

        return Vector3.zero;
    }

    private void PrepareLevel()
    {
        var startBlock = Instantiate(StartBlockPrefab, BlocksRoot);

        BlocksToDelete.Add(startBlock);

        lastGeneratePosition = GetStartPoint();

        var blockPrefab = GetBlockPrefab();

        var firstBlock = Instantiate(blockPrefab, lastGeneratePosition, BlocksRoot.rotation, BlocksRoot);
        firstBlock.transform.localPosition = lastGeneratePosition;

        BlocksToDelete.Add(firstBlock);

        var lastBlockPosition = BlocksRoot.TransformPoint(lastGeneratePosition);
        var distance = lastBlockPosition.z - ball.transform.position.z;

        while (distance < DistanceToGenerateLevel)
        {
            GenerateLevel();

            lastBlockPosition = BlocksRoot.TransformPoint(lastGeneratePosition);
            distance = lastBlockPosition.z - ball.transform.position.z;
        }
    }

    private void Update()
    {
        if (!isGameStarted)
        {
            return;
        }

        var lastBlockPosition = BlocksRoot.TransformPoint(lastGeneratePosition);

        var distance = lastBlockPosition.z - ball.transform.position.z;

        if (distance < DistanceToGenerateLevel)
        {
            GenerateLevel();
        }

        DestroyOldBlocks();
    }

    private void DestroyOldBlocks()
    {
        var blocksToAnimate = new List<GameObject>();

        foreach (var block in BlocksToDelete)
        {
            var distance = ball.transform.position.z - block.transform.position.z;

            if (distance > 10f)
            {
                blocksToAnimate.Add(block);
            }
        }

        foreach (var block in blocksToAnimate)
        {
            BlocksToDelete.Remove(block);

            block.transform.DOMoveY(-10f, 1f)
                .OnComplete(() => { Destroy(block); });
        }
    }

    private void GenerateLevel()
    {
        var isRight = Random.value < 0.5f;

        var lastBlockPosition = BlocksRoot.TransformPoint(lastGeneratePosition);

        var offset = GetBlockOffset() * 2f;

        if (Mathf.Abs(lastBlockPosition.x) >= BorderValue * (2 / offset))
        {
            isRight = lastBlockPosition.x < 0;
        }

        lastGeneratePosition += (isRight ? new Vector3(0f, 0f, offset) : new Vector3(-offset, 0f, 0f));

        var rotation = BlocksRoot.rotation.eulerAngles;

        if (!isRight)
        {
            rotation.y *= -1f;
        }

        var blockPrefab = GetBlockPrefab();

        var block = Instantiate(blockPrefab, lastGeneratePosition, Quaternion.Euler(rotation), BlocksRoot);
        block.transform.localPosition = lastGeneratePosition;
        
        blocksCount++;

        if ((blocksGroupsCount == 0) || (blocksCount % BlocksGroupCount == 0))
        {
            blocksGroupsCount++;
        }

        if (IsRandomCrystalSpawning)
        {
            if (Random.value < CrystalSpawnPossibility)
            {
                var crystalPosition = block.transform.position;
                crystalPosition.y = CrystalPrefab.transform.position.y;

                var crystal = Instantiate(CrystalPrefab, crystalPosition, CrystalPrefab.transform.rotation);
                crystal.transform.parent = block.transform;
            }
        }
        else
        {
            if (crystalsCounter != blocksGroupsCount && (blocksCount % BlocksGroupCount == ((crystalsCounter + 1) % BlocksGroupCount)))
            {
                var crystalPosition = block.transform.position;
                crystalPosition.y = CrystalPrefab.transform.position.y;

                var crystal = Instantiate(CrystalPrefab, crystalPosition, CrystalPrefab.transform.rotation);
                crystal.transform.parent = block.transform;

                crystalsCounter++;
            }
        }

        BlocksToDelete.Add(block);
    }

    private IEnumerator ResetState()
    {
        blocksCount = 0;
        blocksGroupsCount = 0;
        crystalsCounter = 0;

        yield return null;

        uiResetEvent.Invoke();

        foreach (var block in BlocksToDelete)
        {
            Destroy(block);
        }

        BlocksToDelete.Clear();

        PrepareLevel();
    }

    public void OnCrystalCollected()
    {
        crystalsCount++;
        uiCrystalsCountUpdatedEvent.Invoke();
    }

    private void OnGameOver()
    {
        isGameStarted = false;

        uiGameOverEvent.Invoke();
    }

    private void OnStartGame()
    {
        uiStartGameEvent.Invoke();
        isGameStarted = true;
    }
}
