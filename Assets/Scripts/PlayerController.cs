using System;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Controls player behavior (local and remote).
/// </summary>
[NetworkSettings(sendInterval = 0.033f)]
public class PlayerController : NetworkBehaviour, IInputClickHandler
{
    public GameObject bullet;

    /// <summary>
    /// The transform of the shared world anchor.
    /// </summary>
    private Transform sharedWorldAnchorTransform;

    [SyncVar]
    private Vector3 localPosition;

    [SyncVar]
    private Quaternion localRotation;

    [Command]
    public void CmdTransform(Vector3 postion, Quaternion rotation)
    {
        if (!isLocalPlayer)
        {
            localPosition = postion;
            localRotation = rotation;
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (isLocalPlayer)
        {
            CmdFire();
        }
    }

    // Use this for initialization
    void Start () {
        if (isLocalPlayer)
        {
            // 全てのジェスチャーイベントをキャッチできるようにする
            InputManager.Instance.AddGlobalListener(gameObject);

        }
        else
        {
            // リモートプレイヤーは赤色にする
            GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        }

        sharedWorldAnchorTransform = SharedCollection.Instance.gameObject.transform;
        transform.SetParent(sharedWorldAnchorTransform);
    }

    private void OnDestroy()
    {
        if (isLocalPlayer)
        {
            InputManager.Instance.RemoveGlobalListener(gameObject);
        }
    }

    // 自キャラの位置更新
    void Update () {
        if (!isLocalPlayer)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, 0.3f);
            transform.localRotation = localRotation;
            return;
        }

        // カメラの位置で更新する
        transform.position = Camera.main.transform.position;
        transform.rotation = Camera.main.transform.rotation;

        localPosition = transform.localPosition;
        localRotation = transform.localRotation;

        // ホストへ更新命令投げる
        CmdTransform(localPosition, localRotation);

    }

    // ローカルプレイヤーは青色
    public override void OnStartLocalPlayer()
    {
        GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
    }

    public override void OnStartServer()
    {
        GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
    }


    // ホスト側で実行される
    [Command]
    void CmdFire()
    {
        Vector3 bulletDir = transform.forward;
        Vector3 bulletPos = transform.position + bulletDir;

        // プレイヤーの位置から玉を発射する
        GameObject nextBullet = (GameObject)Instantiate(bullet, sharedWorldAnchorTransform.InverseTransformPoint(bulletPos), Quaternion.Euler(bulletDir));
        nextBullet.GetComponentInChildren<Rigidbody>().velocity = bulletDir * 1.0f;
        NetworkServer.Spawn(nextBullet);

        // 8秒後に消える
        Destroy(nextBullet, 8.0f);
    }

}
