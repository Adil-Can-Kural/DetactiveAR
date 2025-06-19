using UnityEngine;
using System.Collections.Generic;

public class SaatSpawner : MonoBehaviour
{
    public GameObject saatPrefab;
    private bool saatSpawnEdildi = false;

    public void SpawnSaati(GameObject bigbenInstance)
    {
        if (saatSpawnEdildi || bigbenInstance == null || saatPrefab == null)
            return;

        Transform noktaKapsayici = FindInChildren(bigbenInstance.transform, "SaatSaklamaNoktalari");
        Transform hedefNokta = FindInChildren(bigbenInstance.transform, "SaatYuvasi");

        if (noktaKapsayici == null || hedefNokta == null)
        {
            Debug.LogError("SaatSaklamaNoktalari veya SaatYuvası bulunamadı!");
            return;
        }

        List<Transform> noktalar = new List<Transform>();
        foreach (Transform child in noktaKapsayici)
            noktalar.Add(child);

        if (noktalar.Count == 0)
        {
            Debug.LogWarning("Hiç saklama noktası yok!");
            return;
        }

        Transform rastgeleNokta = noktalar[Random.Range(0, noktalar.Count)];

        GameObject saatObj = Instantiate(saatPrefab, rastgeleNokta.position, rastgeleNokta.rotation);
        saatObj.GetComponent<SaatFinalTrigger>().hedef = hedefNokta;

        saatSpawnEdildi = true;
    }

    private Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindInChildren(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
