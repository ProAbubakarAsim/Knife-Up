using System.IO;
using UnityEngine;

namespace OnefallGames
{
    public class ShareManager : MonoBehaviour
    {

        public static ShareManager Instance { get; private set; }

        [Header("Native Sharing Config")]
        [SerializeField] private string screenshotName = "screenshot.png";
        [SerializeField] private string shareText = "Can you beat my score!!!";
        //[SerializeField] private string shareSubject = "Share Via";
        //[SerializeField] private string appUrl = "https://play.google.com/store/apps/details?id=com.onefall.HeavenStairs";

        //[Header("Twitter Sharing Config")]
        //[SerializeField] private string titterAddress = "http://twitter.com/intent/tweet";
        //[SerializeField] private string textToDisplay = "Hey Guys! Check out my score: ";
        //[SerializeField] private string tweetLanguage = "en";

        //[Header("Facebook Sharing Config")]
        //[SerializeField] private string fbAppID = "1013093142200006";
        //[SerializeField] private string caption = "Check out My New Score: ";
        //[Tooltip("The URL of a picture attached to this post.The Size must be atleat 200px by 200px.If you dont want to share picture, leave this field empty.")]
        //[SerializeField] private string pictureUrl = "http://i-cdn.phonearena.com/images/article/85835-thumb/Google-Pixel-3-codenamed-Bison-to-be-powered-by-Andromeda-OS.jpg";
        //[SerializeField] private string description = "Enjoy Fun, free games! Challenge yourself or share with friends. Fun and easy to use games.";

        public string ScreenshotPath { private set; get; }
        public string AppUrl { private set; get; }

        private void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            //AppUrl = appUrl;
        }

        /// <summary>
        /// Create the screenshot
        /// </summary>
        public void CreateScreenshot()
        {
            ScreenshotPath = Path.Combine(Application.persistentDataPath, screenshotName);
#if UNITY_EDITOR
            ScreenCapture.CaptureScreenshot(ScreenshotPath);
#else
            ScreenCapture.CaptureScreenshot(screenshotName);
#endif
        }
        

    }
}
