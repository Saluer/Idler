using System.Collections;
using UnityEngine;

public class MineScript : MonoBehaviour
{
    private static readonly int IsWorking = Animator.StringToHash("IsWorking");
    private Animator _animator;
    public static int GoldIncrement;

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

            GameManager.instance.IncrementGold(GoldIncrement);
            yield return new WaitForSeconds(3f);
        }
    }
}