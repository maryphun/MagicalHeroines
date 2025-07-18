using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class DLCManager : MonoBehaviour
{
    public static bool isDLCEnabled;
    public static bool isEnterDLCStage;

    private string expectedDecryptedKey = "DLC_ACTIVATE"; // The key

    [Header("References")]
    [SerializeField] private GameObject text;

    // Start is called before the first frame update
    void Start()
    {
#if DEBUG_MODE
        DLCManager.isDLCEnabled = true;
        text.SetActive(true);
        return;
#endif
        // Check game version of the build
        float.TryParse(Application.version, out float applicationVersion);
        if (applicationVersion < 1.7f)
        {
            // no DLC available
            DLCManager.isDLCEnabled = false;
            return;
        }

        // Check if there is file for the DLC
        List<string> dlcKeyFileNames = new List<string> { "dlc_chapters.dat", "dlc_enemies.dat", "dlc_hisui.dat", "dlc_daiya.dat" };

        for (int i = 0; i < dlcKeyFileNames.Count; i++)
        {
            if (!CheckForDLCKey(dlcKeyFileNames[i]))
            {
                // files are not complete
                DLCManager.isDLCEnabled = false;
                return; 
            }
        }

        DLCManager.isDLCEnabled = true;
        text.SetActive(true);
    }

    bool CheckForDLCKey(string path)
    {
        // Build the full path to the DLC key file
        string dlcKeyFilePath = Path.Combine(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "DLC"), path);

        Debug.Log("Checking for files: " + dlcKeyFilePath);
        // Check if the file exists
        if (File.Exists(dlcKeyFilePath))
        {
            // Read the content of the file (only the first part where the key is stored)
            byte[] fileBytes = File.ReadAllBytes(dlcKeyFilePath);

            // The key is at the start of the file, so only read the first few bytes
            string keyContent = Encoding.UTF8.GetString(fileBytes, 0, expectedDecryptedKey.Length);

            if (keyContent == expectedDecryptedKey)
            {
                return true;
            }
        }

        return false;
    }
}
