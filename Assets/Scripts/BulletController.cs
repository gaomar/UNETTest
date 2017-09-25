using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletController : NetworkBehaviour
{
    //現在位置
    private Transform myTransform;

    //補間率
    private float lerpRate = 0.3f;

    [SyncVar] private Vector3 syncPos;
    [SyncVar] private float syncYRot;

    //1つ前のTransform情報
    private Vector3 lastPos;
    private Quaternion lastRot;

    // Use this for initialization
    void Start () {
        transform.SetParent(SharedCollection.Instance.transform, false);

        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        rb.velocity = transform.parent.TransformDirection(rb.velocity);

        myTransform = transform;

    }
	
	// Update is called once per frame
	void Update () {
        TransmitMotion();
        LerpMotion();
    }

    void TransmitMotion()
    {
        if (!isServer)
        {
            return;
        }

        //Transform情報更新(キャッシュされたTransformから取得するほうが早い)
        lastPos = myTransform.position;
        lastRot = myTransform.rotation;


        //SyncVar変数を変更し、全クライアントと同期を図る
        syncPos = myTransform.position;
        //localEulerAngles: Quaternion→オイラー角(360度表記)
        syncYRot = myTransform.localEulerAngles.y;

    }

    //現在のTransform情報とSyncVar情報とを補間する
    void LerpMotion()
    {
        if (isServer)
        {
            return;
        }

        //位置情報の補間
        myTransform.position = Vector3.Lerp(myTransform.position, syncPos, Time.deltaTime * lerpRate);

        //Y軸のみ変える
        Vector3 newRot = new Vector3(0, syncYRot, 0);

        //角度の補間
        //Euler: オイラー角→Quaternion
        myTransform.rotation = Quaternion.Lerp(myTransform.rotation, Quaternion.Euler(newRot), Time.deltaTime * lerpRate);
    }
}
