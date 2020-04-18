using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;
using YourEthereumController;

namespace YourEthereumManager
{
    /******************************************
	 * 
	 * ScreenEthereumTransactionsView
	 * 
	 * It will show a list with the transactions
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenEthereumTransactionsView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_TRANSACTIONS";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const int TRANSACTION_CONSULT_ALL		= 0;
		public const int TRANSACTION_CONSULT_INPUTS		= 1;
		public const int TRANSACTION_CONSULT_OUTPUTS	= 2;
		

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		public GameObject PrefabSlotTransaction;
		public GameObject[] Tabs;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private GameObject m_root;
		private Transform m_container;
		private Transform m_listKeys;
		private int m_transactionConsultType;
		private Dropdown m_currencies;
		private GameObject m_prefabSlotTransaction;
		private bool m_informationReady = false;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
            base.Initialize(_list);

            m_transactionConsultType = (int)_list[0];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.your.bitcoin.manager.title");

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);

			// CURRENCIES
			m_currencies = m_container.Find("Currency").GetComponent<Dropdown>();
			m_currencies.onValueChanged.AddListener(OnCurrencyChanged);
			m_currencies.options = new List<Dropdown.OptionData>();
			int indexCurrentCurrency = -1;
			for (int i = 0; i < EthereumController.CURRENCY_CODE.Length; i++)
			{
				if (EthereumController.Instance.CurrentCurrency == EthereumController.CURRENCY_CODE[i])
				{
					indexCurrentCurrency = i;
				}
				m_currencies.options.Add(new Dropdown.OptionData(EthereumController.CURRENCY_CODE[i]));
			}

			m_currencies.value = 1;
			m_currencies.value = 0;
			if (indexCurrentCurrency != -1)
			{
				m_currencies.value = indexCurrentCurrency;
			}

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);			
			EthereumEventController.Instance.EthereumEvent += new EthereumEventHandler(OnEthereumEvent);

			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + "EthereumController.Instance.Network.ToString()";

			Tabs[TRANSACTION_CONSULT_ALL].GetComponent<Button>().onClick.AddListener(OnConsultAll);
			Tabs[TRANSACTION_CONSULT_INPUTS].GetComponent<Button>().onClick.AddListener(OnConsultInputs);
			Tabs[TRANSACTION_CONSULT_OUTPUTS].GetComponent<Button>().onClick.AddListener(OnConsultOutputs);

			m_listKeys = m_container.Find("ListItems");
			m_listKeys.GetComponent<SlotManagerView>().ClearCurrentGameObject(true);
			Invoke("UpdateListItems", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			UIEventController.Instance.UIEvent -= OnMenuEvent;
            EthereumEventController.Instance.EthereumEvent -= OnEthereumEvent;

			if (m_listKeys!=null) m_listKeys.GetComponent<SlotManagerView>().Destroy();
			m_listKeys = null;

			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnCurrencyChanged
		 */
		private void OnCurrencyChanged(int _index)
		{
			EthereumController.Instance.CurrentCurrency = m_currencies.options[_index].text;
            EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_CURRENCY_CHANGED);
		}

		// -------------------------------------------
		/* 
		 * UpdateListItems
		 */
		public void UpdateListItems()
		{
			if (!m_informationReady)
			{
				m_informationReady = true;
				EthereumController.Instance.GetAllInformation(EthereumController.Instance.GetPublicKey(EthereumController.Instance.CurrentPrivateKey));				
			}
		}

		// -------------------------------------------
		/* 
		* OnConsultAll
		*/
		private void OnConsultAll()
		{
			m_transactionConsultType = TRANSACTION_CONSULT_ALL;
            EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_HISTORY);
        }

		// -------------------------------------------
		/* 
		* OnConsultInputs
		*/
		private void OnConsultInputs()
		{
			m_transactionConsultType = TRANSACTION_CONSULT_INPUTS;
            EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_HISTORY);
        }

        // -------------------------------------------
        /* 
		* OnConsultOutputs
		*/
        private void OnConsultOutputs()
		{
			m_transactionConsultType = TRANSACTION_CONSULT_OUTPUTS;
            EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_HISTORY);
        }

        // -------------------------------------------
        /* 
		* BackPressed
		*/
        private void BackPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		private void OnEthereumEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EthereumController.EVENT_ETHEREUMCONTROLLER_UPDATE_ACCOUNT_DATA)
			{
				m_listKeys.GetComponent<SlotManagerView>().ClearCurrentGameObject(true);
				Invoke("UpdateListItems", 0.1f);
			}
            if (_nameEvent == EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_HISTORY)
            {
                switch (m_transactionConsultType)
                {
                    case TRANSACTION_CONSULT_INPUTS:
                        m_container.Find("ListItems/Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.ethereum.transactions.inputs");
                        break;

                    case TRANSACTION_CONSULT_OUTPUTS:
                        m_container.Find("ListItems/Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.ethereum.transactions.outputs");
                        break;

                    case TRANSACTION_CONSULT_ALL:
                        m_container.Find("ListItems/Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.ethereum.transactions.all");
                        break;
                }

                for (int i = 0; i < Tabs.Length; i++)
                {
                    if (m_transactionConsultType == i)
                    {
                        Tabs[i].transform.Find("Background").gameObject.SetActive(true);
                    }
                    else
                    {
                        Tabs[i].transform.Find("Background").gameObject.SetActive(false);
                    }
                }

                List<ItemMultiObjectEntry> items = null;
                switch (m_transactionConsultType)
                {
                    case TRANSACTION_CONSULT_INPUTS:
                        items = EthereumController.Instance.InTransactionsHistory;
                        break;

                    case TRANSACTION_CONSULT_OUTPUTS:
                        items = EthereumController.Instance.OutTransactionsHistory;
                        break;

                    case TRANSACTION_CONSULT_ALL:
                        items = EthereumController.Instance.AllTransactionsHistory;
                        break;
                }

                m_listKeys.GetComponent<SlotManagerView>().ClearCurrentGameObject(true);
                m_listKeys.GetComponent<SlotManagerView>().Initialize(4, items, PrefabSlotTransaction, null);
            }
        }

		// -------------------------------------------
		/* 
		 * OnMenuEvent
		 */
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				Destroy();
			}
		}
	}
}