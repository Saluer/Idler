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
            _animator.SetTrigger(IsWorking);

            GameManager.instance.IncreaseGold(1);
            yield return new WaitForSeconds(1f);
        }
    }
}