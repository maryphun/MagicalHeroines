using UnityEngine;

public static class VFXSpawner
{
    public static GameObject SpawnVFX(string name, Transform parent, Vector2 globalPosition, string path = "Prefabs/VFX/")
    {
        GameObject origin = Resources.Load<GameObject>(path + name);
        if (origin == null)
        {
            Debug.LogWarning(path + name + " Çå©Ç¬Ç©ÇËÇ‹ÇπÇÒÅB");
        }
        GameObject vfx = GameObject.Instantiate(origin, parent);
        vfx.name = name;
        vfx.GetComponent<RectTransform>().position = globalPosition;

        return vfx;
    }
}
