using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{

    [SerializeField] GameObject smokePullingPool;

    private static VFXManager _instance;


    public static VFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject();
                _instance = go.AddComponent<VFXManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            throw new System.Exception("An instance of VFXManager singleton already exists.");
        }
        else
        {
            _instance = this;
        }
        if (smokePullingPool == null)
        {
            Debug.LogError($"Missing Pulling pool in VFXManager {name}");
        }
    }

    public void spawnSmokeAt(Transform transform, Vector3 hitPoint, Quaternion direction)
    {

        if (smokePullingPool.transform.childCount > 0)
        {
            StartCoroutine(playVFXCoroutine(transform, hitPoint, direction));
        }
        else
        {
            Debug.LogWarning("No available smoke VFX componenets at the moment");
        }
    }
    private IEnumerator playVFXCoroutine(Transform transform, Vector3 hitPoint, Quaternion direction)
    {

        Transform selectedChild = smokePullingPool.transform.GetChild(0);

        selectedChild.transform.SetLocalPositionAndRotation(hitPoint, direction);
        selectedChild.SetParent(transform);

        VisualEffect effect = selectedChild.gameObject.GetComponent<VisualEffect>();

        if (effect != null)
        {

            effect.Play();
            yield return new WaitForSeconds(0.3f);
            yield return new WaitWhile(() => effect.aliveParticleCount > 0);
            yield return new WaitForSeconds(0.5f);

        }
        else
        {
            Debug.LogWarning($"Could not find VFX componenet in child {selectedChild.name}");
        }

        selectedChild.SetParent(smokePullingPool.transform);
        selectedChild.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, 0f, 0f));



    }


    public void changeLayer(GameObject GO, string LayerName)
    {

        if (GO.GetComponent<VisualEffect>() != null) { return; }


        int Layer = LayerMask.NameToLayer(LayerName);
        if (Layer < 0)
        {
            Debug.LogWarning($"Could not find the expected layer {LayerName}");
            Layer = LayerMask.NameToLayer("Default");
        }

        GO.layer = Layer;
        foreach (Transform child in GO.transform)
        {
            changeLayer(child.gameObject, LayerName);
        }


    }

}
