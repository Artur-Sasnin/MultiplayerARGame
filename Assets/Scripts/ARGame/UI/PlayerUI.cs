using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private Text _totalCoinsText;
    [SerializeField]
    private Text _hintText;

    public Text TotalCoinsText => _totalCoinsText;
    public Text HintText => _hintText;
}
