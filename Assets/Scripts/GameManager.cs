using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Vector3 playerPos;
    [SerializeField] private Button diceRollButton;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform parent;
    [SerializeField] private float moveSpeed;
    [SerializeField] private List<GameObject> playerMovementBlock = new List<GameObject>();

    private GameObject dice1RollAnimation, dice2RollAnimation, dice3RollAnimation;
    private GameObject dice4RollAnimation, dice5RollAnimation, dice6RollAnimation;
    private int playerStep;
    private int selectDiceNumAnimation;
    private bool isPlayerMoving = false;

    void Start()
    {
        playerPos = player.transform.position;
    }

    public void SetAnimationGameObjects(List<GameObject> gos)
    {
        dice1RollAnimation = Instantiate(gos[0], parent);
        dice2RollAnimation = Instantiate(gos[1], parent);
        dice3RollAnimation = Instantiate(gos[2], parent);
        dice4RollAnimation = Instantiate(gos[3], parent);
        dice5RollAnimation = Instantiate(gos[4], parent);
        dice6RollAnimation = Instantiate(gos[5], parent);
    }

    public void ResetPlayerPos()
    {
        player.transform.position = playerPos;
        playerStep = 0;
    }

    private void ResetDice()
    {
        if (playerStep != playerMovementBlock.Count - 1)
        {
            diceRollButton.interactable = true;
        }
        DeactivateDiceRollAnimations();
    }

    private void DeactivateDiceRollAnimations()
    {
        dice1RollAnimation.SetActive(false);
        dice2RollAnimation.SetActive(false);
        dice3RollAnimation.SetActive(false);
        dice4RollAnimation.SetActive(false);
        dice5RollAnimation.SetActive(false);
        dice6RollAnimation.SetActive(false);
    }

    public void DiceRoll()
    {
        StartCoroutine(FetchRandomNumberAndAnimate());
    }

    private IEnumerator FetchRandomNumberAndAnimate()
    {
        string apiKey = "cc1b86e0-a924-4a28-a769-256eae76f600";
        string apiUrl = "https://api.random.org/json-rpc/2/invoke";

        // Request data for generating a random integer.
        string requestData = "{\"jsonrpc\": \"2.0\", \"method\": \"generateIntegers\", \"params\": {\"apiKey\": \"" + apiKey + "\", \"n\": 1, \"min\": 1, \"max\": 6, \"replacement\": true}, \"id\": 1}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestData);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            // Set content type.
            request.SetRequestHeader("Content-Type", "application/json");

            // Set User-Agent header.
            request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:100.0) Gecko/20100101 Firefox/100.0");

            // Send the request.
            yield return request.SendWebRequest();

            // Handle the response.
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse the JSON response.
                string jsonResponse = request.downloadHandler.text;
                RandomNumberResponse response = JsonUtility.FromJson<RandomNumberResponse>(jsonResponse);

                if (response != null && response.result != null && response.result.random != null && response.result.random.data != null && response.result.random.data.Length > 0)
                {
                    // Get the random number from the response.
                    int randomNum = response.result.random.data[0];

                    // Animate the dice based on the fetched random number.
                    AnimateDice(randomNum);
                }
                else
                {
                    Debug.LogError("Invalid response format. Check the Random.org API documentation.");
                }
            }
            else
            {
                Debug.LogError("Error fetching random number: " + request.error);
            }
        }
    }

    private void AnimateDice(int randNo)
    {
        diceRollButton.interactable = false;
        selectDiceNumAnimation = randNo;

        switch (selectDiceNumAnimation)
        {
            case 1:
                dice1RollAnimation.SetActive(true);
                break;
            case 2:
                dice2RollAnimation.SetActive(true);
                break;
            case 3:
                dice3RollAnimation.SetActive(true);
                break;
            case 4:
                dice4RollAnimation.SetActive(true);
                break;
            case 5:
                dice5RollAnimation.SetActive(true);
                break;
            case 6:
                dice6RollAnimation.SetActive(true);
                break;
        }
    }

    private IEnumerator MovePlayer(int steps)
    {
        isPlayerMoving = true;

        for (int i = 0; i < steps; i++)
        {
            yield return StartCoroutine(MovePlayerOneStep());
        }

        isPlayerMoving = false;
        ResetDice();
        yield return null;
    }

    private IEnumerator MovePlayerOneStep()
    {
        if (playerStep < playerMovementBlock.Count)
        {
            Vector3 targetPosition = playerMovementBlock[playerStep].transform.position;

            while (Vector3.Distance(player.transform.position, targetPosition) > 0.01f)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            if (playerStep == playerMovementBlock.Count - 1)
            {
                Debug.Log("You win!");
                diceRollButton.interactable = false;
            }
            else
            {
                playerStep++;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public void MovePlayerButtonPressed()
    {
        if (!isPlayerMoving && !diceRollButton.interactable)
        {
            StartCoroutine(MovePlayer(selectDiceNumAnimation));
        }
    }
}

[Serializable]
public class RandomNumberResponse
{
    public Result result;

    [Serializable]
    public class Result
    {
        public RandomData random;
    }

    [Serializable]
    public class RandomData
    {
        public int[] data;
    }
}