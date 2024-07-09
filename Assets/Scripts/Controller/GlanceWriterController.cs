using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Decoder;
using Elements;

using VirtualKey = Elements.VirtualKey;
using BasicKeyboardView = View.BasicKeyboardView;
using GlanceWriterController = Decoder.GlanceWriterDecoder;
using SuggestionsView = Controller.SuggestionView;
using TMPro;


namespace Controller
{
    public class GlanceWriterController : MonoBehaviour
    {
        public enum MovingState { DECODE, SELECTION, FINISH };

        GlanceWriterDecoder glanceWriterDecoder;

        BasicKeyboardView keyboardView;
        SuggestionView suggestionView;

        PromptView promptView;

        List<OnScreenButton> virtualKeys;
        List<OnScreenButton> functionalKeys;
        List<OnScreenButton> suggestionButtons;
        List<OnScreenButton> dwellHandledButtons;
        List<OnScreenButton> allButtons;

        Stack<string> undoText;
        MovingState state = MovingState.SELECTION;
        EventHandler keyEventHandler;

        public GameObject hitPointScriptObj;
        public GameObject suggestionViewScriptObj;

        public GameObject keyboardViewScriptObj;

        public GameObject promptViewScriptObj;

        public GameObject textConsole;

        public GameObject buttonClickScriptObject;

        public GameObject modalityHintViewScriptObject;

        private RControllerUtil hitPointScript;
        private BasicKeyboardView basicKeyboardViewScript;
        private ButtonClicked buttonClickedScript;
        private ModalityHintView modalityHintViewScript;

        private Queue<IEnumerator> decoderTasks = new Queue<IEnumerator>();
        private bool decoderRunning = false;
        // private int totalDecodeTaskExecuted = 0;

        private DwellBasedSelection dwellBasedSelection;

        void Start()
        {
            Debug.Log("GlanceWriterController Start");
            Debug.Log("GlanceWriterController Initializing");
            undoText = new Stack<string>();
            // keyEventHandler = GetKeyEventHandler();
            suggestionView = suggestionViewScriptObj.GetComponent<SuggestionView>();
            keyboardView = keyboardViewScriptObj.GetComponent<BasicKeyboardView>();
            promptView = promptViewScriptObj.GetComponent<PromptView>();
            hitPointScript = hitPointScriptObj.GetComponent<RControllerUtil>();
            buttonClickedScript = buttonClickScriptObject.GetComponent<ButtonClicked>();
            modalityHintViewScript = modalityHintViewScriptObject.GetComponent<ModalityHintView>();

            // Get keys/buttons on screen
            virtualKeys = keyboardView.GetVirtualKeys();
            functionalKeys = keyboardView.GetFunctionalKeys();
            suggestionButtons = suggestionView.GetSuggestionButtons();

            // Initilize the prompt text showing which modality is been used
            SetupModalityButtonText();

            // Setup Dwell
            dwellBasedSelection = new DwellBasedSelection();
            dwellHandledButtons = new List<OnScreenButton>();
            dwellHandledButtons.AddRange(suggestionButtons);
            dwellHandledButtons.AddRange(functionalKeys);
            dwellBasedSelection.RegisterTargets(dwellHandledButtons);

            allButtons = new List<OnScreenButton>();
            allButtons.AddRange(suggestionButtons);
            allButtons.AddRange(functionalKeys);
            allButtons.AddRange(virtualKeys);
            
            glanceWriterDecoder = new GlanceWriterDecoder(virtualKeys, textConsole);
            Debug.Log("GlanceWriterController initialized");

        }

        void Update()
        {
            List<Vector3> inputPoints = hitPointScript.GetHitPointQueueForDecoder();
            int inputPointsNum = inputPoints.Count;
            if (inputPointsNum == 0)
            {
                return;
            }
            Vector3 inputPoint = inputPoints[inputPointsNum - 1];

            HandleButtonAnimation(inputPoint);

            OnScreenButton dwellClickedButton = dwellBasedSelection.OnPoint(inputPoint);
            if (dwellClickedButton != null)
            {
                buttonClickedScript.OnButtonClicked(dwellClickedButton);
            }

            OnNewInputPoint(inputPoint);
        }

        private void HandleButtonAnimation(Vector3 inputPoint)
        {
            float x = inputPoint.x;
            float y = inputPoint.y;

            foreach (OnScreenButton button in allButtons)
            {
                GameObject buttonGameObject = button.mButtonGameObject;
                Image buttonImage = buttonGameObject.GetComponent<Image>();
                if (button.mButtonGameObject.activeSelf && button.containPoint(x, y))
                {
                    if (button.mButtonType != OnScreenButton.ButtonType.KEY) {
                        int progressCompensation = (int) (80f * button.accumulatedFrames / DwellBasedSelection.DWELL_THRESHOLD);
                        byte colorVal = (byte) (220 - progressCompensation);
                        buttonImage.color = new Color32(colorVal, colorVal, colorVal, 255);
                    } else {
                        buttonImage.color = new Color32(220, 220, 220, 255);
                    }
                } else {
                    buttonImage.color = new Color32(255, 255, 255, 255);
                }
            }
        }

        public void OnNewInputPoint(Vector3 inputPoint)
        {
            state = GetMovingState(inputPoint, state);

            if (state == MovingState.DECODE)
            {
                decoderTasks.Enqueue(Decode(inputPoint));
                if (!decoderRunning)
                {
                    StartCoroutine(ExecuteTraks());
                }

            }
            else if (state == MovingState.SELECTION)
            {
                // TODO: Call Dwell here but not necessarily
            }
            else if (state == MovingState.FINISH)
            {
                decoderTasks.Enqueue(StopDecode());
                if (!decoderRunning)
                {
                    StartCoroutine(ExecuteTraks());
                }
            }
        }

        private IEnumerator ExecuteTraks()
        {
            while (decoderTasks.Count > 0)
            {
                decoderRunning = true;
                var currentTask = decoderTasks.Dequeue();
                // totalDecodeTaskExecuted += 1;
                yield return StartCoroutine(currentTask);
            }
            decoderRunning = false;
        }

        private IEnumerator Decode(Vector3 inputPoint)
        {
            double filterScore = 1;
            filterScore = glanceWriterDecoder.UpdateGesture(inputPoint, promptView.GetInputText());
            // appendToDebug(inputPoint.x + "," + inputPoint.y + "\t" + filterScore + "");
            yield return null;
        }

        private IEnumerator StopDecode()
        {
            glanceWriterDecoder.StopDecoding();
            List<KeyValuePair<string, float>> newSuggestions = glanceWriterDecoder.GetDecodeResult();
            suggestionView.UpdateSuggestion(newSuggestions);
            InputWord(newSuggestions[0].Key);
            suggestionView.DisplaySuggestion();
            yield return null;
        }

        public MovingState GetMovingState(Vector3 inputPoint, MovingState state)
        {
            // textConsole.GetComponent<TextMeshPro>().SetText("inputPoint: " + inputPoint + "   ruiliu2   " + keyboardView.SPLIT_LINE_Y_LOWER);
            float x = inputPoint.x, y = inputPoint.y;
            MovingState newState = MovingState.DECODE;
            if (state == MovingState.DECODE)
            {
                if (y < keyboardView.SPLIT_LINE_Y_UPPER)
                {
                    newState = MovingState.DECODE;
                }
                else
                {
                    newState = MovingState.FINISH;
                }
            }
            else if (state == MovingState.FINISH)
            {
                newState = MovingState.SELECTION;
            }
            else if (state == MovingState.SELECTION)
            {
                if (y < keyboardView.SPLIT_LINE_Y_LOWER)
                {
                    newState = MovingState.DECODE;
                }
                else
                {
                    newState = MovingState.SELECTION;
                }
            }
            return newState;
        }

        public void UndoInputWord()
        {
            promptView.SetInputText(undoText.Count > 0 ? undoText.Pop() : "");
            //     inputText.Text = undoText.Count > 0 ? undoText.Pop() : "";
            //     inputText.SelectionStart = inputText.Text.Length;
            ClearSuggestion();
        }

        public void ClearInputWord()
        {
            undoText.Clear();
            promptView.SetInputText("");
            ClearSuggestion();
        }

        public void ClearSuggestion()
        {
            suggestionView.RemoveSuggestion();
        }

        public void InputWord(string word)
        {
            undoText.Push(promptView.GetInputText());
            string inputText = promptView.GetInputText();
            promptView.SetInputText(inputText + word + " ");
            //     undoText.Push(inputText.Text);
            //     inputText.AppendText(word + " ");
        }

        public void SetupModalityButtonText()
        {
            foreach(OnScreenButton button in functionalKeys)
            {
                GameObject buttonObj = button.GetGameObject();
                String bttnName = button.GetButtonName();
                if (bttnName == "SwitchModality")
                {
                    if (hitPointScript.GetIsGaze() == true) {
                        buttonObj.GetComponentInChildren<TMP_Text>().text = "Input mode: Gaze\nClick to switch to Controller";
                    } else {
                        buttonObj.GetComponentInChildren<TMP_Text>().text = "Input mode: Controller\nClick to switch to Gaze";
                    }
                }
            }

        }

        void printDebug(string info)
        {
            textConsole.GetComponent<TextMeshPro>().SetText(info);
        }

        void appendToDebug(string info)
        {
            textConsole.GetComponent<TextMeshPro>().SetText(textConsole.GetComponent<TextMeshPro>().text + "\n" + info);
        }

    }
}
