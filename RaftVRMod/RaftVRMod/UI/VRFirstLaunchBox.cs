using RaftVR.Configs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RaftVR.UI
{
    class VRFirstLaunchBox : MenuBox
    {
        internal static VRFirstLaunchBox instance;

        private Button button1;
        private Button button2;

        private Text button1Text;
        private Text button2Text;
        private Text messageText;

        internal static bool alreadyPatched = false;
        internal static bool apiLoaded = false;
        internal static bool alreadyChoseSettings = false;

        private void Awake()
        {
            instance = this;

            RectTransform leftButtonTransform = transform.Find("YesButton") as RectTransform;
            leftButtonTransform.gameObject.name = "OculusButton";
            Destroy(leftButtonTransform.GetComponentInChildren<I2.Loc.Localize>());

            button1Text = leftButtonTransform.GetComponentInChildren<Text>();
            button1Text.text = "Left Choice";

            button1 = leftButtonTransform.GetComponent<Button>();

            RectTransform rightButtonTransform = transform.Find("NoButton") as RectTransform;
            rightButtonTransform.gameObject.name = "Right Button";
            Destroy(rightButtonTransform.GetComponentInChildren<I2.Loc.Localize>());

            button2Text = rightButtonTransform.GetComponentInChildren<Text>();

            button2 = rightButtonTransform.GetComponent<Button>();

            RectTransform message = transform.Find("Exit game") as RectTransform;
            message.gameObject.name = "RaftVR Message";
            Destroy(message.GetComponent<I2.Loc.Localize>());

            messageText = message.GetComponent<Text>();
        }

        private void Start()
        {
            if (apiLoaded)
            {
                StartDialog();
            }
            else
            {
                UpdateDialogSingleOption("Install and load ExtraSettingsAPI to setup RaftVR.", "Close", Button_Later);
            }
        }

        internal void StartDialog()
        {
            if (alreadyChoseSettings)
            {
                ChangeToFinished();
            }
            else
            {
                StartFirstTimeSetup();
            }
        }

        private void UpdateDialog(string message, string leftButtonText, string rightButtonText, UnityAction leftButtonAction, UnityAction rightButtonAction, bool reactivateFirstButton = true)
        {
            if (reactivateFirstButton && !button1.gameObject.activeSelf)
                button1.gameObject.SetActive(true);

            messageText.text = message;

            button1Text.text = leftButtonText;
            button2Text.text = rightButtonText;

            button1.onClick.RemoveAllListeners();
            button1.onClick.AddListener(leftButtonAction);

            button2.onClick.RemoveAllListeners();
            button2.onClick.AddListener(rightButtonAction);
        }

        private void UpdateDialogSingleOption(string message, string buttonText, UnityAction buttonAction)
        {
            UpdateDialog(message, buttonText, buttonText, buttonAction, buttonAction, false);
            button1.gameObject.SetActive(false);
        }

        internal void StartFirstTimeSetup()
        {
            if (!IsOpen)
                Open();

            UpdateDialog("RaftVR has loaded successfully! Choose your preferred VR runtime.", "Oculus", "SteamVR", Button_Oculus, Button_SteamVR);
        }

        private void ChangeToTurnMethod()
        {
            UpdateDialog("Choose your preferred turning method.", "Snap", "Smooth", Button_Snap, Button_Smooth);
        }

        private void ChangeToStandingSeated()
        {
            UpdateDialog("Do you prefer playing while standing or seated?", "Standing", "Seated", Button_Standing, Button_Seated);
        }

        private void ChangeToFinished()
        {
            if (alreadyPatched)
            {
                UpdateDialog("Put on your headset and press \"Start VR\" when ready.", "Later", "Start VR", Button_Later, Button_StartVR);
            }
            else
            {
                UpdateDialog("Restart the game to play in VR.", "Later", "Quit", Button_Later, Button_Quit);
            }
        }

        private void Button_Oculus()
        {
            VRConfigs.Runtime = VRConfigs.VRRuntime.Oculus;
            ChangeToTurnMethod();
        }

        private void Button_SteamVR()
        {
            VRConfigs.Runtime = VRConfigs.VRRuntime.SteamVR;
            ChangeToTurnMethod();
        }

        private void Button_Snap()
        {
            VRConfigs.SnapTurn = true;
            ChangeToStandingSeated();
        }

        private void Button_Smooth()
        {
            VRConfigs.SnapTurn = false;
            ChangeToStandingSeated();
        }

        private void Button_Standing()
        {
            VRConfigs.SeatedMode = false;
            ChangeToFinished();
        }

        private void Button_Seated()
        {
            VRConfigs.SeatedMode = true;
            ChangeToFinished();
        }

        private void Button_StartVR()
        {
            ModInitializer.instance.StartMod();
            Close();
        }

        private void Button_Quit()
        {
            Application.Quit();
        }

        private void Button_Later()
        {
            Close();
        }
    }
}
