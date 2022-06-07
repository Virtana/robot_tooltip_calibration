using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//namespace UnityTipsTricks
//{
	public class newcamscript : MonoBehaviour
	{
		public int superSize = 2;
		private int _shotIndex = 0;

        IEnumerator SpawnCoroutine()
        {
            while(true)
            {
                
			   
			    ScreenCapture.CaptureScreenshot($"Image{_shotIndex}.png", superSize);
		        _shotIndex++;
                Debug.Log("PIcture taken");
                yield return new WaitForSeconds(1.0f);
            }

        }

        private void Start()
		{
			StartCoroutine(SpawnCoroutine());
		}

		private void Update()
		{
			
		}
	}
//}