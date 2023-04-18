using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class AIALeaderboard : MonoBehaviour{
    public TextMeshProUGUI scoreText;
    public TMP_InputField playerNameInputField;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI leaderboardText;
    public PlayerScoreWrapper playerScoreWrapper;

    private string format = "F0";

    [SerializeField] string leaderboardId = "planet-collab";

    string memberID;
    List<PlayerScore> playerScores = new List<PlayerScore>();
 
    private async void OnEnable() {
        leaderboardText.text = "Loading...";
        scoreText.gameObject.SetActive(true);
        playerNameInputField.gameObject.SetActive(true);
        float _score = GameControl.Instance ? GameControl.Instance.Score : 0;
        scoreText.text = "SCORE: " + _score.ToString(format);
        await UpdateTop10Scores();
    }

    public void ResetButton() {
        GameControl.Instance.Restart();
	}

    private int GetLeaderboardIntValue(float _value, int _multiplier) {
        return (int)(_value * _multiplier);
	}

    public class PlayerScore {
        public string PlayerId;
        public string PlayerName;
        public int Rank;
        public float Score;
    }

    public class PlayerScoreWrapper {
        public int limit;
        public int total;
        public List<PlayerScore> results;
    }

    private float GetLeaderboardFloatValue(float _value, int _multiplier) {
        return _value / _multiplier;
    }
    public void UploadScoreButton() {
        UploadScore();
    }
	public async Task UploadScore() {
        await AuthenticationService.Instance.UpdatePlayerNameAsync(playerNameInputField.text);
        await LeaderboardsService.Instance
            .AddPlayerScoreAsync(leaderboardId, GameControl.Instance.Score);
        //Debug.Log(playerEntryInfo);
        infoText.text = playerNameInputField.text + " score was submitted...";
        //Debug.Log(infoText.text);
        await UpdateLeaderboardCentered();
    }

    public async Task UpdateTop10Scores() {
        var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId);
        var playerEntryInfo = JsonConvert.SerializeObject(scoresResponse);
        playerScoreWrapper = JsonConvert.DeserializeObject<PlayerScoreWrapper>(playerEntryInfo);

        if (playerScoreWrapper == null || playerScoreWrapper.results == null) {
            return;
        }

        string _leaderboardText = "";
        for (int i = 0; i < playerScoreWrapper.results.Count; i++) {
            if (playerScoreWrapper.results[i].PlayerId == AuthenticationService.Instance.PlayerId) {
                _leaderboardText += "<color=#5CD6F2>";
            }

            _leaderboardText += playerScoreWrapper.results[i].Rank + 1 + ". ";
            _leaderboardText += FormatUsername(playerScoreWrapper.results[i].PlayerName);
            _leaderboardText += " - ";
            _leaderboardText += playerScoreWrapper.results[i].Score;
            _leaderboardText += "\n";

            if (playerScoreWrapper.results[i].PlayerId == AuthenticationService.Instance.PlayerId) {
                _leaderboardText += "</color>";
            }
            _leaderboardText += "\n";
        }
        leaderboardText.text = _leaderboardText;

    }

    private string FormatUsername(string _username) {
        int hashIndex = _username.IndexOf('#');
        if (hashIndex != -1) {
            _username = _username.Substring(0, hashIndex);
        }
        _username = _username.Substring(0, Mathf.Min(_username.Length, 11));
        return _username;
    }
    async Task UpdateLeaderboardCentered() {
        var rangeLimit = 5;
        var scoresResponse = await LeaderboardsService.Instance
            .GetPlayerRangeAsync(leaderboardId,
                new GetPlayerRangeOptions { RangeLimit = rangeLimit }
            );
        var playerEntryInfo = JsonConvert.SerializeObject(scoresResponse);
        playerScoreWrapper = JsonConvert.DeserializeObject<PlayerScoreWrapper>(playerEntryInfo);

        if (playerScoreWrapper == null || playerScoreWrapper.results == null) {
            return;
        }

        string _leaderboardText = "";
        for (int i = 0; i < playerScoreWrapper.results.Count; i++) {
            if (playerScoreWrapper.results[i].PlayerId == AuthenticationService.Instance.PlayerId) {
                _leaderboardText += "<color=yellow>";
            }

            _leaderboardText += playerScoreWrapper.results[i].Rank + 1 + ". ";
            _leaderboardText += FormatUsername(playerScoreWrapper.results[i].PlayerName);
            _leaderboardText += " - ";
            _leaderboardText += playerScoreWrapper.results[i].Score;
            _leaderboardText += "\n";

            if (playerScoreWrapper.results[i].PlayerId == AuthenticationService.Instance.PlayerId) {
                _leaderboardText += "</color>";
            }
            _leaderboardText += "\n";
        }
        leaderboardText.text = _leaderboardText;
    }

    /*async Task UpdateLeaderboardTop10() {
        infoText.text = "Top 10 leaderboard updated";
        infoText.text = "Centered scores updated";
        string _scores = await UpdateScores();
        leaderboardText.text = _scores;
        /*for (int i = 0; i < response.items.Length; i++) {
            LootLockerLeaderboardMember currentEntry = response.items[i];
            _leaderboardText += currentEntry.rank + ". ";
            _leaderboardText += currentEntry.metadata;
            _leaderboardText += " - ";
            _leaderboardText += GetLeaderboardFloatValue(currentEntry.score, 1000).ToString(format);
            _leaderboardText += "\n\n";
        }
                
    }*/

    // Increment and save a string that goes from a to z, then za to zz, zza to zzz etc.
    string GetAndIncrementScoreCharacters() {
        // Get the current score string
        string incrementalScoreString = PlayerPrefs.GetString(nameof(incrementalScoreString), "a");

        // Get the current character
        char incrementalCharacter = PlayerPrefs.GetString(nameof(incrementalCharacter), "a")[0];

        // If the previous character we added was 'z', add one more character to the string
        // Otherwise, replace last character of the string with the current incrementalCharacter
        if (incrementalScoreString[incrementalScoreString.Length - 1] == 'z') {
            // Add one more character
            incrementalScoreString += incrementalCharacter;
        } else {
            // Replace character
            incrementalScoreString = incrementalScoreString.Substring(0, incrementalScoreString.Length - 1) + incrementalCharacter.ToString();
        }

        // If the letter int is lower than 'z' add to it otherwise start from 'a' again
        if ((int)incrementalCharacter < 122) {
            incrementalCharacter++;
        } else {
            incrementalCharacter = 'a';
        }

        // Save the current incremental values to PlayerPrefs
        PlayerPrefs.SetString(nameof(incrementalCharacter), incrementalCharacter.ToString());
        PlayerPrefs.SetString(nameof(incrementalScoreString), incrementalScoreString.ToString());

        // Return the updated string
        return incrementalScoreString;
    }
}