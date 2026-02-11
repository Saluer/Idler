using DefaultNamespace;
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
        if (!other.CompareTag("Player"))
            return;

        _playerInside = false;

        _shopOpened = false;
        HideShopUi();
    }

    private void Update()
    {
        if (_playerInside && Input.GetKeyDown(KeyCode.E) && (
                GameManager.instance.gameMode == GameManager.GameMode.Active ||
                GameManager.instance.gameMode == GameManager.GameMode.Shop))
        {
            Interact();
        }
    }

    private void OnEnable()
    {
        GameManager.OnForceCloseShop += ForceCloseShop;
    }

    private void OnDisable()
    {
        GameManager.OnForceCloseShop -= ForceCloseShop;
    }

    private void Interact()
    {
        _shopOpened = !_shopOpened;

        if (_shopOpened)
            ShowShopUi();
        else
            HideShopUi();
    }

    private void ForceCloseShop()
    {
        if (!_shopOpened)
            return;

        _shopOpened = false;
        HideShopUi();
    }

    private void ShowShopUi()
    {
        Time.timeScale = 0f;
        shopCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameManager.instance.gameMode = GameManager.GameMode.Shop;
    }

    private void HideShopUi()
    {
        Time.timeScale = 1f;
        shopCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameManager.instance.gameMode = GameManager.GameMode.Active;
    }
}