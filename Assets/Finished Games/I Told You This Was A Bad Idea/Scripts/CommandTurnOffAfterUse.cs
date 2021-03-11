#if UNITY_WEBGL
using System.Collections.Generic;
using UnityEngine.Serialization;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UnityWebGLSpeechDetection
{
    public class CommandTurnOffAfterUse : MonoBehaviour
{
        private ISpeechDetectionPlugin _mSpeechDetectionPlugin = null;

        private List<string> _mWords = new List<string>();

        [SerializeField]
        private string[] wordsToDetect;

        private bool canBeUsed = true;

        [Serializable]
        public class StartThisEvent : UnityEvent { }

        [FormerlySerializedAs("IfCounterOver")]
        [SerializeField]
        private StartThisEvent getCounter = new StartThisEvent();

       // protected CommandTurnOffAfterUse() { }

        public StartThisEvent ifTextFound
        {
            get { return getCounter; }
            set { getCounter = value; }
        }

        private void Press()
        {
            UISystemProfilerApi.AddMarker("Button.onClick", this);
            getCounter.Invoke();
        }


        IEnumerator Start()
        {
            // get the singleton instance
            _mSpeechDetectionPlugin = SpeechDetectionUtils.GetInstance();

            // check the reference to the plugin
            if (null == _mSpeechDetectionPlugin)
            {
                Debug.LogError("WebGL Speech Detection Plugin is not set!");
                yield break;
            }

            // wait for plugin to become available
            while (!_mSpeechDetectionPlugin.IsAvailable())
            {
                yield return null;
            }

            // subscribe to events
            _mSpeechDetectionPlugin.AddListenerOnDetectionResult(HandleDetectionResult);


            foreach (string word in wordsToDetect)
            {
                _mWords.Add(word.ToLower());
            }
        }

        bool HandleDetectionResult(DetectionResult detectionResult)
        {
            if (null == detectionResult)
            {
                return false;
            }
            SpeechRecognitionResult[] results = detectionResult.results;
            if (null == results)
            {
                return false;
            }
            bool doAbort = false;
            foreach (SpeechRecognitionResult result in results)
            {
                SpeechRecognitionAlternative[] alternatives = result.alternatives;
                if (null == alternatives)
                {
                    continue;
                }
                foreach (SpeechRecognitionAlternative alternative in alternatives)
                {
                    if (string.IsNullOrEmpty(alternative.transcript))
                    {
                        continue;
                    }
                    string lower = alternative.transcript.ToLower();
                    Debug.LogFormat("Detected: {0}", lower);
                    foreach (string word in _mWords)
                    {
                        if (lower.Contains(word) && canBeUsed)
                        {
                            Press();
                            canBeUsed = false;
                            Debug.Log(string.Format("**** {0} ****", word));
                            doAbort = true;
                            break;
                        }
                    }
                }
                if (doAbort)
                {
                    break;
                }
            }

            // abort detection on match for faster matching on words instead of complete sentences
            if (doAbort)
            {
                _mSpeechDetectionPlugin.Abort();
                return true;
            }

            return false;
        }

        public void turnOffThisAnswer()
        {
            canBeUsed = false;
        }
        public void turnOnThisAnswer()
        {
            canBeUsed = true;
        }
    }

}
#endif