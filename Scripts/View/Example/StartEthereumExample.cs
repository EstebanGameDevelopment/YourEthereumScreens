using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourEthereumManager;

public class StartEthereumExample : MonoBehaviour {

	private bool m_hasBeenInitializedBitcoin = false;

	// -------------------------------------------
	/* 
	* Buttons to open the bitcoin managment
	*/
	void OnGUI()
	{
		if (m_hasBeenInitializedBitcoin)
		{
			if (ScreenEthereumController.Instance.ScreensEnabled > 0)
			{
				return;
			}
		}
		

		float fontSize = 1.2f * 15;
		float yGlobalPosition = 10;
		if (GUI.Button(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "OPEN WALLET"))
		{
			ScreenEthereumController.Instance.InitializeEthereum(YourCommonTools.UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, ScreenEthereumPrivateKeyView.SCREEN_NAME);
			m_hasBeenInitializedBitcoin = true;
		}
		yGlobalPosition += 2.2f * fontSize;

		if (GUI.Button(new Rect(new Vector2(10, yGlobalPosition), new Vector2(Screen.width - 20, 2 * fontSize)), "SEND MONEY"))
		{
			ScreenEthereumController.Instance.InitializeEthereum(YourCommonTools.UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, ScreenEthereumSendView.SCREEN_NAME);
			m_hasBeenInitializedBitcoin = true;
		}
	}
}
