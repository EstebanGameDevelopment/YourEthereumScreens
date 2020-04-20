using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;
using YourEthereumController;

namespace YourEthereumManager
{
	/******************************************
	 * 
	 * ScreenTransactionSummaryView
	 * 
	 * It ask for the confirmation of the user to run the transaction
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenTransactionSummaryView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_TRANSACTION_SUMMARY";

		// ----------------------------------------------
		// SUBS
		// ----------------------------------------------	
		public const string SUB_EVENT_PREMIUM_POST_CONFIRMATION = "SUB_EVENT_PREMIUM_POST_CONFIRMATION";
		public const string SUB_EVENT_PREMIUM_POST_DESTROY      = "SUB_EVENT_PREMIUM_POST_DESTROY";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private bool m_priceBitcoinLoaded = false;
		private Dictionary<string, Transform> m_iconsCurrencies = new Dictionary<string, Transform>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
            base.Initialize(_list);

            List<object> paramsTransaction = (List<object>)_list[0];

            decimal amount = (decimal)paramsTransaction[0];
			string currency = (string)paramsTransaction[1];
			string toAddressTarget = (string)paramsTransaction[2];
			string subjectTransaction = (string)paramsTransaction[3];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("To").GetComponent<Text>().text = LanguageController.Instance.GetText("message.to");
			m_container.Find("Subject").GetComponent<Text>().text = LanguageController.Instance.GetText("message.subject");

			m_container.Find("AddressTarget").GetComponent<Text>().text = toAddressTarget;
			m_container.Find("SubjectTransaction").GetComponent<Text>().text = subjectTransaction;

			m_container.Find("Amount").GetComponent<Text>().text = LanguageController.Instance.GetText("message.amount");

			m_container.Find("Button_Confirm").GetComponent<Button>().onClick.AddListener(OnConfirmationTransaction);
			m_container.Find("Button_Confirm/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.ethereum.send.confirm.right.and.pay");

			m_container.Find("Button_Cancel").GetComponent<Button>().onClick.AddListener(OnCancelTransaction);
			m_container.Find("Button_Cancel/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.cancel");

			m_container.Find("PriceBitcoin").GetComponent<Text>().text = Utilities.Trim(amount.ToString()) + " ETH";
			m_container.Find("PriceCurrency").GetComponent<Text>().text = Utilities.Trim((amount * EthereumController.Instance.CurrenciesExchange[currency]).ToString()) + " " + currency;

			m_iconsCurrencies.Clear();
			for (int i = 0; i < EthereumController.CURRENCY_CODE.Length; i++)
			{
				m_iconsCurrencies.Add(EthereumController.CURRENCY_CODE[i], m_container.Find("IconsCurrency/" + EthereumController.CURRENCY_CODE[i]));
				if (EthereumController.Instance.CurrentCurrency == EthereumController.CURRENCY_CODE[i])
				{
					m_iconsCurrencies[EthereumController.CURRENCY_CODE[i]].gameObject.SetActive(true);
				}
				else
				{
					m_iconsCurrencies[EthereumController.CURRENCY_CODE[i]].gameObject.SetActive(false);
				}
			}

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);			
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			UIEventController.Instance.UIEvent -= OnMenuEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnRentPurchase
		 */
		private void OnConfirmationTransaction()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenEthereumSendView.EVENT_SCREENETHEREUMSEND_USER_CONFIRMED_RUN_TRANSACTION);
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnClickCancel
		 */
		private void OnCancelTransaction()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				OnCancelTransaction();
			}
		}
	}
}