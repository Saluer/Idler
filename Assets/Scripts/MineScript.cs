using System.Collections;
using UnityEngine;

public class MineScript : MonoBehaviour
{
    private static readonly int IsWorking = Animator.StringToHash("IsWorking");
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        StartCoroutine(YieldGold());
    }


    private IEnumerator YieldGold()
    {
        while (true)
        {
            // if (GameManager.instance.gameMode != GameManager.GameMode.Active)
            // {
            //     _animator.SetBool(IsWorking, false);
            //     yield return new WaitForSeconds(0.2f);
            //     continue;
            // }

            _animator.SetBool(IsWorking, true);

            GameManager.instance.IncreaseGold(1);
            yield return new WaitForSeconds(1f);
        }
    }
}