using TMPro;
using UnityEngine;

public class UpdateShardCounter : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (_text != null)
            _text.text = ShardProgress.PlayerShards.ToString();
    }
}