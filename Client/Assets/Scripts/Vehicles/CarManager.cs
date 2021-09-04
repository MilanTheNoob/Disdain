using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    #region Singleton

    public static CarManager instance;
    void Awake() { instance = this; }

    #endregion

    public GameObject PlayerUI;
    public GameObject CarUI;

    [Space]

    public SteeringWheel Wheel;
    public GameObject Camera;

    public static WheelVehicle car;

    void Start()
    {
        LeanTween.alpha(instance.PlayerUI, 1f, 0.2f);
        LeanTween.alpha(instance.CarUI, 0f, 0.2f);
    }

    void FixedUpdate()
    {
        if (car != null)
        {
            car.steering = Wheel.OutPut * 50;

            if (Input.GetKeyDown(KeyCode.E)) { ExitCar(); }
        }
    }

    public void ExitCar()
    {
        if (car != null)
        {
            GameManager.ActivePlayer.SetActive(true);
            GameManager.ActivePlayer.transform.parent = null;

            LeanTween.alpha(instance.PlayerUI, 1f, 0.2f);
            LeanTween.alpha(instance.CarUI, 0f, 0.2f);

            car.Handbrake = true;
            car = null;
        }
    }

    /// <summary>
    /// Called for a player to interact & drive a car
    /// </summary>
    /// <param name="_car">The WheelVehicle script of the car</param>
    public static void EnterCar(WheelVehicle _car)
    {
        car = _car;
        car.Handbrake = false;

        GameManager.ActivePlayer.SetActive(false);
        GameManager.ActivePlayer.transform.parent = car.transform;
        GameManager.ActivePlayer.transform.position = new Vector3(GameManager.ActivePlayer.transform.position.x, 
            GameManager.ActivePlayer.transform.position.y + 5f, GameManager.ActivePlayer.transform.position.z);

        LeanTween.alpha(instance.PlayerUI, 0f, 0.2f);
        LeanTween.alpha(instance.CarUI, 1f, 0.2f);
    }
}
