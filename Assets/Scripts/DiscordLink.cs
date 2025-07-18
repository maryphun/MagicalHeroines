using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordLink : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private string discordInvitationLink;

    public void OnClickDiscordButton()
    {
        Application.OpenURL(discordInvitationLink);
    }
}
