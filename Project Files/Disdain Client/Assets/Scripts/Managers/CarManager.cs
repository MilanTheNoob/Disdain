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

    public Button Exit;
    public SteeringWheel Wheel;
    public Slider Accelerator;

    [Space]

    public GameObject PlayerUI;
    public GameObject CarUI;

    static WheelVehicle currentCar;

    GameObject player;

    void Start()
    {
        TweeningLibrary.FadeIn(instance.PlayerUI, 0.2f);
        TweeningLibrary.FadeOut(instance.CarUI, 0.2f);

        player = GameManager.ActivePlayer;
        Exit.onClick.AddListener(ExitCar);
    }

    void FixedUpdate()
    {
        if (currentCar != null)
        {
            currentCar.steering = Wheel.OutPut * 50;
            currentCar.throttle = Accelerator.value;

            if (Input.GetKeyDown(KeyCode.E)) { ExitCar(); }
        }
    }

    void ExitCar()
    {
        if (currentCar != null)
        {
            player.SetActive(true);
            player.transform.parent = null;

            TweeningLibrary.FadeIn(instance.PlayerUI, 0.2f);
            TweeningLibrary.FadeOut(instance.CarUI, 0.2f);

            currentCar.Handbrake = true;
            currentCar = null;
        }
    }

    /// <summary>
    /// Called for a player to interact & drive a car
    /// </summary>
    /// <param name="car">The WheelVehicle script of the car</param>
    public static void EnterCar(WheelVehicle car)
    {
        currentCar = car;
        currentCar.Handbrake = false;

        instance.player.SetActive(false);
        instance.player.transform.parent = currentCar.transform;
        instance.player.transform.position = new Vector3(instance.player.transform.position.x, instance.player.transform.position.y + 5f, instance.player.transform.position.z);

        TweeningLibrary.FadeOut(instance.PlayerUI, 0.2f);
        TweeningLibrary.FadeIn(instance.CarUI, 0.2f);
    }
}
