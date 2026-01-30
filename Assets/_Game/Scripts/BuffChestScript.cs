using System.Collections;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Collider))]
    public class BuffChestScript : MonoBehaviour
    {
        [SerializeField] private BuffConfig buff;
        [SerializeField] private string displayText;
        private Canvas _displayTextCanvas;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Start()
        {
            _displayTextCanvas = GameObject.FindGameObjectWithTag("Buff display text").GetComponent<Canvas>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerScript>();
            if (!player)
                return;

            ApplyBuff(player);
        }

        private void ApplyBuff(PlayerScript player)
        {
            if (!buff)
                return;

            buff.Apply(player);
            var tmp = _displayTextCanvas.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = displayText;
            StartCoroutine(DisposeText());
            return;

            IEnumerator DisposeText()
            {
                yield return new WaitForSeconds(1.5f);
                tmp.text = "";
                Destroy(gameObject);
                yield return null;
            }
        }
    }
}