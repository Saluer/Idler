using System.Collections;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public class MineScript : MonoBehaviour
{
    private static readonly int IsWorking = Animator.StringToHash("IsWorking");
    private Animator _animator;
    public static int DiamondsIncrement = 1;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        StartCoroutine(YieldGold());
        GameManager.instance.mines.Add(this);
    }

    private IEnumerator YieldGold()
    {
        while (true)
        {
            if (GameManager.instance.gameMode != GameManager.GameMode.Active)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            _animator.SetBool(IsWorking, true);

            GameManager.instance.IncrementDiamonds(DiamondsIncrement);
            SpawnGoldText(DiamondsIncrement);
            yield return new WaitForSeconds(5f);
        }
    }

    private void SpawnGoldText(int goldAmount)
    {
        var textObject = new GameObject("DiamondsText")
        {
            transform =
            {
                position = transform.position + Vector3.up * 3f
            }
        };

        var text = textObject.AddComponent<TextMeshPro>();
        text.text = $"+{goldAmount} diamonds";
        text.fontSize = 3;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.green;

        Destroy(textObject, 1f);
    }
}