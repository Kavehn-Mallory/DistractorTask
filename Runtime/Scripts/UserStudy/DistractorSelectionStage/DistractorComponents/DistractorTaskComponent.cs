using System;
using DistractorTask.Core;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Random = UnityEngine.Random;


namespace DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents
{
    public class DistractorTaskComponent : MonoBehaviour
    {
        
        [SerializeField] private Canvas canvas;
        [SerializeField] private DistractorComponent label;
        [SerializeField] private int numberOfDistractors = 5;
    
        [Header("Distractor Placement Settings")]
        [SerializeField] private float defaultDistanceFromCamera = 1f;
        [SerializeField] private float canvasWidth = 1f;
        [Tooltip("Determines the distance of the distractors from the center based on the horizontal viewing angle")]
        [SerializeField] private float targetDistractorAngleFromCenter = 5f;
        [Tooltip("Determines the distance of the peripheral distractor from the center based on the horizontal viewing angle")]
        [SerializeField] private float peripheralDistractorAngleFromCenter = 7f;
        [Tooltip("Determines the size of the distractors based on the horizontal viewing angle")]
        [SerializeField] private float targetDistractorViewAngle = 2f;
    
        [Header("Distractor Trial Settings")]
        [SerializeField] private DistractorShapeGroup[] distractorShapes;
        [Tooltip("Set to true if the target should never be the same target in two consecutiveTrials")]
        [SerializeField] private bool changeTargetAfterEveryTrial;
        
    
        

        private string[][] _distractorShapes;
        private string[][] _targetShapes;
        private DistractorComponent[] _distractors;
        private DistractorComponent _peripheralDistractor;
        private int _targetElementIndex;
        private Camera _mainCamera;
        private DistractorComponent _selectedDistractor;

        private TimeSpan _trialStartTime;
        


        private void Start()
        {
            _targetElementIndex = -1;
            _distractorShapes = new string[distractorShapes.Length][];
            _targetShapes = new string[distractorShapes.Length][];
            for (var i = 0; i < distractorShapes.Length; i++)
            {
                var shapeGroup = distractorShapes[i];
                _distractorShapes[i] = shapeGroup.distractorLetters.Split(',');
                _targetShapes[i] = shapeGroup.targetLetters.Split(',');
            }

            _distractors = new DistractorComponent[numberOfDistractors + 1];
            for (int i = 0; i < numberOfDistractors + 1; i++)
            {
                var labelInstance = Instantiate(label, canvas.transform, false);
                labelInstance.gameObject.name = "Distractor";
                labelInstance.distractorIndex = i;
                labelInstance.Component = this;
                _distractors[i] = labelInstance.GetComponent<DistractorComponent>();
            }

            var peripheralDistractor = Instantiate(label, canvas.transform, false);
            peripheralDistractor.Component = this;
            _peripheralDistractor = peripheralDistractor.GetComponent<DistractorComponent>();
            _peripheralDistractor.gameObject.name = "Peripheral Distractor";

            
            _mainCamera = Camera.main;
        
            if (!_mainCamera)
            {
                Debug.LogError("No main camera found. Is required for correct positioning of target", this);
                enabled = false;
                return;
            }

            RepositionCanvas(_mainCamera.transform.position + Vector3.forward * defaultDistanceFromCamera);
            DisableCanvas();
            //StartNextTrial();
        }

        public void IncreaseDistance()
        {
            var currentPosition = canvas.transform.position;
            var distance = math.distance(currentPosition, _mainCamera.transform.position);
            RepositionCanvas(_mainCamera.transform.position + (distance + 0.5f) * _mainCamera.transform.forward);
        }
        
        public void DecreaseDistance()
        {
            var currentPosition = canvas.transform.position;
            var distance = math.distance(currentPosition, _mainCamera.transform.position);
            var multiplier = math.max(0.5f, distance - 0.5f);
            RepositionCanvas(_mainCamera.transform.position +  multiplier * _mainCamera.transform.forward);
        }
    

        public void RepositionCanvas(Vector3 position)
        {
        
            //todo normalize the vector and place along the ray 
            var distanceFromCamera = math.distance(position, _mainCamera.transform.position);
            var targetOffset = DistractorPlacementExtension.CalculateActualSize(distanceFromCamera, targetDistractorAngleFromCenter);
            var targetSize = DistractorPlacementExtension.CalculateActualSize(distanceFromCamera, targetDistractorViewAngle);
            var peripheralOffset = DistractorPlacementExtension.CalculateActualSize(distanceFromCamera, peripheralDistractorAngleFromCenter);
        
            var dimensions = canvas.pixelRect;
        
            var scaleFactor = canvasWidth / dimensions.width;
        
            canvas.GetComponent<RectTransform>().localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        
            var distanceFromCenter = targetOffset / scaleFactor;
            var peripheralDistractorPosition = new Vector2(-peripheralOffset / scaleFactor, 0);
            var targetSizeInPixel = targetSize / scaleFactor;

            canvas.transform.SetPositionAndRotation(position, Quaternion.identity);

            var angle = 360f / (numberOfDistractors + 1);

            var currentAngle = 0f;
            foreach (var distractor in _distractors)
            {
                distractor.GetComponent<DistractorComponent>().UpdateDistractorSize(targetSizeInPixel);
                distractor.RectTransform.PlaceLabelsAtPosition(distanceFromCenter, currentAngle);
                currentAngle += angle;
            }

            _peripheralDistractor.GetComponent<DistractorComponent>().UpdateDistractorSize(targetSizeInPixel * 2f);
            _peripheralDistractor.RectTransform.anchoredPosition = peripheralDistractorPosition;
        }




        public Vector2 GetBoundsForDistractorArea()
        {
            //peripheral distractor is double the size of the normal ones (therefore we take the normal viewing angle to account for half of it reaching further out than the distance from center
            var horizontalDistance = DistractorPlacementExtension.CalculateActualSize(0.1f, targetDistractorViewAngle + peripheralDistractorAngleFromCenter);
            var verticalDistance = DistractorPlacementExtension.CalculateActualSize(0.1f, targetDistractorViewAngle / 2f + targetDistractorAngleFromCenter);
            return new Vector2(horizontalDistance, verticalDistance);
        }



        
        public void StartTrial(int loadLevel)
        {
            _trialStartTime = DateTime.Now.TimeOfDay;
            foreach (var distractor in _distractors)
            {

                distractor.Text.text = _distractorShapes[loadLevel].RandomElement();
            }

            if (changeTargetAfterEveryTrial && _targetElementIndex >= 0)
            {
                _targetElementIndex += Random.Range(1, _distractors.Length);
                _targetElementIndex %= _distractors.Length;
            }
            else
            {
                _targetElementIndex = Random.Range(0, _distractors.Length);
            }
        
            _distractors[_targetElementIndex].Text.text =
                _targetShapes[loadLevel].RandomElement();
        
            _peripheralDistractor.Text.text = _targetShapes[loadLevel].RandomElement();
        }
        
        
    
        public void OnHoverEnter(UIHoverEventArgs args)
        {
            Debug.Log("On hover enter");
            _selectedDistractor = args.uiObject?.GetComponent<DistractorComponent>();
        }

        public void OnHoverExit()
        {
            _selectedDistractor = null;
        }
        
        public void EnableCanvas()
        {
            canvas.enabled = true;
        }
        
        public void DisableCanvas()
        {
            canvas.enabled = false;
        }

        public int OnInputReceived()
        {
            if (_selectedDistractor)
            {
                return _selectedDistractor.distractorIndex;
            }

            return -1;
        }

        public DistractorSelectionResult CheckInput()
        {
            var endTime = DateTime.Now.TimeOfDay;
            var symbolOrder = GenerateSymbolOrder();
            if (!_selectedDistractor)
            {
                return new DistractorSelectionResult(-1, _targetElementIndex, symbolOrder, _trialStartTime);
            }

            return new DistractorSelectionResult(_selectedDistractor.distractorIndex, _targetElementIndex, symbolOrder, _trialStartTime);
        }

        private string GenerateSymbolOrder()
        {
            var result = "";

            foreach (var distractor in _distractors)
            {
                result += distractor.Text.text;
                result += ",";
            }

            result += _peripheralDistractor.Text.text;
            return result;
        }

        [Serializable]
        public struct DistractorSelectionResult
        {
            public int selectedDistractor;
            public int targetDistractor;
            public string symbolOrder;
            public TimeSpan startTime;

            public DistractorSelectionResult(int selectedDistractor, int targetDistractor, string symbolOrder, TimeSpan startTime)
            {
                this.selectedDistractor = selectedDistractor;
                this.targetDistractor = targetDistractor;
                this.symbolOrder = symbolOrder;
                this.startTime = startTime;

            }

            public bool WasSuccessful()
            {
                if (selectedDistractor == -1)
                {
                    return false;
                }

                return selectedDistractor == targetDistractor;
            }
        }
        
        

        [Serializable]
        public struct DistractorShapeGroup
        {

            public string groupName;
            [Tooltip("Separate options with a ','")]
            public string distractorLetters;

            [Tooltip("Separate options with a ','")]
            public string targetLetters;
        }

        


    }
}
