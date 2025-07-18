using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class CreateDLCFiles : MonoBehaviour
{
    private string expectedDecryptedKey = "DLC_ACTIVATE"; // The key
    private int paddingSize = 1024 * 10; // 10 KB padding

    // Start is called before the first frame update
    void Start()
    {
        List<string> dlcKeyFileNames = new List<string> { "dlc_chapters.dat", "dlc_enemies.dat", "dlc_hisui.dat", "dlc_daiya.dat" };
        for (int i = 0; i < dlcKeyFileNames.Count; i++)
        {
            CreatePaddedDLCKeyFile(dlcKeyFileNames[i]);
        }
    }

    // To simulate a larger file, this method adds padding data to the file
    public void CreatePaddedDLCKeyFile(string path)
    {
        string dlcKeyFilePath = Path.Combine(Application.dataPath, path);

        // Your key content
        byte[] keyContent = Encoding.UTF8.GetBytes(expectedDecryptedKey);

        // Generate random padding
        byte[] padding = new byte[paddingSize];
        System.Random random = new System.Random();
        random.NextBytes(padding);

        // Combine the key and padding into one file
        using (FileStream fileStream = new FileStream(dlcKeyFilePath, FileMode.Create))
        {
            fileStream.Write(keyContent, 0, keyContent.Length);
            fileStream.Write(padding, 0, padding.Length); // Add the padding
        }

        Debug.Log($"DLC key file created with {paddingSize / 1024} KB of padding in " + dlcKeyFilePath);
    }
}
