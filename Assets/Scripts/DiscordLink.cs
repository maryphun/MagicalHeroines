using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordLink : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private string discordInvitationLink;

#if STEAM_REVIEW
    private void Start()
    {
        gameObject.SetActive(false);
    }
#endif

    public void OnClickDiscordButton()
    {
        Application.OpenURL(discordInvitationLink);
    }
}
