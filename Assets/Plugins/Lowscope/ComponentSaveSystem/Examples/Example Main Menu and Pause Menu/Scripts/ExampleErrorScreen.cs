using UnityEngine;
using UnityEngine.UI;

namespace Lowscope.Saving.Examples
{
    public class ExampleErrorScreen : MonoBehaviour
    {
        [SerializeField] private Text messageText;
        [SerializeField] private Text titleText;
        [SerializeField] private Button continueButton;

        private void Start()
        {
            continueButton.onClick.AddListener(() => this.gameObject.SetActive(false));
        }

        public void Configure(string title, string message)
        {
            titleText.text = title;
            messageText.text = message;
            this.gameObject.SetActive(true);
        }
    }
}