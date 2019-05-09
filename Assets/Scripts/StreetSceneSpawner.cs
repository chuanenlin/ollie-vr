using System.Collections.Generic;
using UnityEngine;

public class StreetSceneSpawner : MonoBehaviour
{

    public GameObject container;
    public float spawnThreshold;
    public GameObject[] streetScenes;
    public Vector3[] streetSceneOffsets;
    public float[] streetSceneDepths;
    private System.Random random = new System.Random();
    private Transform playerTransform;
    private Queue<GameObject> lastScenes = new Queue<GameObject>();
    private Queue<int> lastSceneIndex = new Queue<int>();
    private float lastSceneZPos = -1;

    private GameObject sceneQueuedForDestory = null;
    private float queuedForDestoryZPos = -1;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        spawnNextStreetScene();
    }

    // Update is called once per frame
    void Update()
    {
        checkSpawnDespawn();        
    }

    void checkSpawnDespawn()
    {
        if (getLastSceneDistanceLeft() < spawnThreshold)
            spawnNextStreetScene();

        if (sceneQueuedForDestory != null && queuedForDestoryZPos < playerTransform.position.z)
            despawnLastStreetScene();
    }

    void spawnNextStreetScene()
    {
        int nextSceneIndex = selectRandomInt();
        GameObject nextScene = streetScenes[nextSceneIndex];
        Vector3 nextSceneOffset = streetSceneOffsets[nextSceneIndex];
        Vector3 scenePos = nextSceneOffset;
        scenePos.z += playerTransform.position.z;
        float distanceLeft = getLastSceneDistanceLeft();
        if (distanceLeft != -1)
            scenePos.z += distanceLeft;

        if (lastScenes.Count > 0)
        {
            lastSceneIndex.Dequeue();
            sceneQueuedForDestory = lastScenes.Dequeue();
            queuedForDestoryZPos = lastSceneZPos;
        }

        lastScenes.Enqueue(Instantiate(nextScene, scenePos, Quaternion.identity, container.transform));
        lastSceneIndex.Enqueue(nextSceneIndex);
        lastSceneZPos = getLastSceneFarestZPos();
    }

    void despawnLastStreetScene()
    {
        Destroy(sceneQueuedForDestory);
        sceneQueuedForDestory = null;
        queuedForDestoryZPos = -1;
    }

    float getLastSceneDistanceLeft()
    {
        float farestZPos = lastSceneZPos == -1 ? getLastSceneFarestZPos() : lastSceneZPos;
        if (farestZPos == -1) return -1;
        return farestZPos - playerTransform.position.z;
    }

    float getLastSceneFarestZPos()
    {
        if (lastScenes.Count <= 0) return -1;
        int lastIndex = lastSceneIndex.Peek();
        float depth = streetSceneDepths[lastIndex];
        float farestZPos = lastScenes.Peek().transform.position.z - streetSceneOffsets[lastIndex].z + depth;
        return farestZPos;
    }

    int selectRandomInt()
    {
        return random.Next(0, streetScenes.Length - 1);
    }
}
