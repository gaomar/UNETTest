using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class NetworkManagerTest : MonoBehaviour {

    // 接続部屋名
    public string matchName = "default";

    // 同時接続人数
    public uint matchSize = 4U;

    // Use this for initialization
    void Start () {
        // マルチプレイ初期化
        NetworkManager.singleton.matchName = matchName;
        NetworkManager.singleton.matchSize = matchSize;
        NetworkManager.singleton.StartMatchMaker();

        // マルチプレイ接続状況確認
        matchListCheck();

    }
	
	// Update is called once per frame
	void Update () {
    }

    // マルチプレイルーム作成
    private void createMatch()
    {
        NetworkManager.singleton.matchMaker.CreateMatch(NetworkManager.singleton.matchName, NetworkManager.singleton.matchSize, true, "", "", "", 0, 0, OnMatchCreate);
    }

    // マルチプレイ状態チェック
    private void matchListCheck()
    {
        NetworkManager.singleton.matchMaker.ListMatches(0, 10, "", true, 0, 0, OnMatchList);
    }

    public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        // 部屋作れた？
        if (success)
        {
            // ホストとして起動
            NetworkManager.singleton.StopHost();
            NetworkManager.singleton.StartHost(matchInfo);
        }
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        // ホストが存在しなければ部屋作ってホストになる
        if (matches == null)
        {
            createMatch();

        }
        else if (matches.Count == 0)
        {
            createMatch();

        }
        else
        {
            // すでにあればクライアントとして接続
            NetworkManager.singleton.matchMaker.JoinMatch(matches[0].networkId, "", "", "", 0, 0, OnMatchJoined);

        }

    }

    public void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            // クライアントとして接続
            NetworkManager.singleton.StopClient();
            NetworkManager.singleton.StartClient(matchInfo);
        }

    }
}
