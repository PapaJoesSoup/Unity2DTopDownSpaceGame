using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
  public class UIActions : MonoBehaviour
  {
    private GameObject _ship;
    private ShipController _shipController;
    private GameObject _deathPanel;
    private GameObject _btnReSpawn;
    private bool _shipDestroyed = false;

    void Start()
    {
      _ship = GameObject.Find("Ship");
      _shipController = _ship.GetComponent<ShipController>();

      // Death Panel objects
      _deathPanel = GameObject.Find("DeathPanel");
      _deathPanel.SetActive(true);
      _btnReSpawn = GameObject.Find("ReSpawn");
      _btnReSpawn.GetComponent<Button>().onClick.AddListener(OnReSpawnClick);
      _deathPanel.SetActive(false);
    }

    void Update()
    {
      if (_ship.activeInHierarchy || _shipDestroyed) return;
      _shipDestroyed = true;
      Invoke(nameof(DisplayDeathPanel), 3.5f);
    }

    private void DisplayDeathPanel()
    {
      _deathPanel.SetActive(true);
    }

    void OnReSpawnClick()
    {
      _deathPanel.SetActive(false);
      _ship.SetActive(true);
      _shipDestroyed = false;
      _shipController.RespawnShip();
    }

    private void OnDestroy()
    {
      if (_btnReSpawn != null)
        _btnReSpawn.GetComponent<Button>().onClick.RemoveAllListeners();
    }
  }
}