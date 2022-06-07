using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//namespace UnityTipsTricks
//{
	public class camscript : MonoBehaviour
	{
		public int superSize = 2;
		private int _shotIndex = 0;

        IEnumerator PictureCoroutine()
        {
            while(true)
            {
                
			   
			    ScreenCapture.CaptureScreenshot($"Images/Image{_shotIndex}.png", superSize);
		        _shotIndex++;
                Debug.Log("PIcture taken");
                yield return new WaitForSeconds(1.0f);
            }

        }

        private void Start()
		{
			StartCoroutine(PictureCoroutine());
		}

		private void Update()
		{
			
		}
	}
//}