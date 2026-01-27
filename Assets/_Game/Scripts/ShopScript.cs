using UnityEngine;

public class ShopScript : MonoBehaviour
{
    [SerializeField] private Canvas shopCanvas;

    private bool _playerInside;
    private bool _shopOpened;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInside = false;
        shopCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameManager.instance.gameMode = GameManager.GameMode.Active;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void Interact()
    {
        _shopOpened = !_shopOpened;
        if (_shopOpened)
        {
            shopCanvas.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameManager.instance.gameMode = GameManager.GameMode.Shop;
        }
        else
        {
            shopCanvas.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.instance.gameMode = GameManager.GameMode.Active;
        }
    }
}