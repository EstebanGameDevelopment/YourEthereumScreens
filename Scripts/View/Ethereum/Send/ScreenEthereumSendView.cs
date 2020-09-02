using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;
using YourEthereumController;

namespace YourEthereumManager
{
    /******************************************
	 * 
	 * ScreenEthereumSendView
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenEthereumSendView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SEND";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENETHEREUMSEND_USER_CONFIRMED_RUN_TRANSACTION = "EVENT_SCREENETHEREUMSEND_USER_CONFIRMED_RUN_TRANSACTION";
		public const string EVENT_SCREENETHEREUMSEND_CANCELATION                    = "EVENT_SCREENETHEREUMSEND_CANCELATION";

		// ----------------------------------------------
		// SUBS
		// ----------------------------------------------	
		private const string SUB_EVENT_SCREENETHEREUM_CONFIRMATION_EXIT_TRANSACTION	= "SUB_EVENT_SCREENETHEREUM_CONFIRMATION_EXIT_TRANSACTION";
		private const string SUB_EVENT_SCREENETHEREUM_CONTINUE_WITH_LOW_FEE			= "SUB_EVENT_SCREENETHEREUM_CONTINUE_WITH_LOW_FEE";
		private const string SUB_EVENT_SCREENETHEREUM_USER_CONFIRMATION_MESSAGE		= "SUB_EVENT_SCREENETHEREUM_USER_CONFIRMATION_MESSAGE";
		private const string SUB_EVENT_SCREENETHEREUM_USER_ERROR_MESSAGE            = "SUB_EVENT_SCREENETHEREUM_USER_ERROR_MESSAGE";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private InputField m_publicAddressInput;
		private string m_publicAddressToSend = "";
		private bool m_validPublicAddressToSend = false;
		private GameObject m_validAddress;
		private GameObject m_saveAddress;

		private InputField m_amountInput;
		private string m_amountInCurrency = "0";
		private decimal m_amountInEther = 0;
		private Dropdown m_currencies;
		private string m_currencySelected;
		private decimal m_exchangeToEther;

		private InputField m_messageInput;

		private bool m_hasChanged = false;
		private bool m_transactionSuccess = false;
		private string m_transactionIDHex = "";

		private int m_idUser = -1;
		private string m_passwordUser = "";
		private long m_idRequest = -1;

		public bool HasChanged
		{
			get { return m_hasChanged; }
			set
			{
				m_hasChanged = value;
			}
		}
		public bool ValidPublicKeyToSend
		{
			set
			{
				m_validPublicAddressToSend = value;
				string labelAddress = EthereumController.Instance.AddressToLabel(m_publicAddressToSend);
				if (labelAddress != m_publicAddressToSend)
				{
					m_container.Find("Address/Label").GetComponent<Text>().text = labelAddress;
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.red;
				}
				else
				{
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.black;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
            base.Initialize(_list);

            string publicKeyAddress = "";
			string amountTransaction = "0";
			string messageTransaction = LanguageController.Instance.GetText("screen.send.explain.please");

            object[] objectParams = (object[])_list[0];

            if (objectParams != null)
            {
                if (objectParams.Length > 0)
                {
                    List<object> paramsSend = (List<object>)objectParams[0];
                    if (paramsSend != null)
                    {
                        if (paramsSend.Count > 0)
                        {
                            publicKeyAddress = (string)paramsSend[0];
                            if (publicKeyAddress == null) publicKeyAddress = "";
                            if (paramsSend.Count > 2)
                            {
                                amountTransaction = (string)paramsSend[1];
                                EthereumController.Instance.CurrentCurrency = (string)paramsSend[2];
                                if (paramsSend.Count > 3)
                                {
                                    messageTransaction = (string)paramsSend[3];
                                }
                            }
                        }
                    }
                }
            }

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			// DOLLAR
			m_currencySelected = EthereumController.Instance.CurrentCurrency;
			m_exchangeToEther = EthereumController.Instance.CurrenciesExchange[m_currencySelected];

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(OnBackButton);

			// YOUR WALLET			
			m_container.Find("YourWallet").GetComponent<Button>().onClick.AddListener(OnCheckWallet);
			UpdateWalletButtonInfo();

			// PUBLIC KEY TO SEND
			m_saveAddress = m_container.Find("Address/SaveAddress").gameObject;
			m_saveAddress.GetComponent<Button>().onClick.AddListener(OnSaveAddress);
			m_saveAddress.SetActive(false);

			m_validAddress = m_container.Find("Address/ValidAddress").gameObject;
			m_validAddress.GetComponent<Button>().onClick.AddListener(OnAddressValid);
			m_validAddress.SetActive(false);

			m_container.Find("Address/Label").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.send.write.destination.address");
			m_publicAddressInput = m_container.Find("Address/PublicKey").GetComponent<InputField>();
			m_publicAddressInput.onValueChanged.AddListener(OnValuePublicKeyChanged);			
			m_container.Find("Address/SelectAddress").GetComponent<Button>().onClick.AddListener(OnSelectAddress);
			m_container.Find("Address/SelectAddress/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.addresses");
#if !ENABLE_FULL_WALLET
			m_container.Find("Address/SelectAddress").gameObject.SetActive(false);
#endif
			m_publicAddressInput.text = publicKeyAddress;

            // AMOUNT
            m_container.Find("Amount/Label").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.send.amount.to.send");
			m_amountInput = m_container.Find("Amount/Value").GetComponent<InputField>();
			m_amountInput.onValueChanged.AddListener(OnValueAmountChanged);
			m_amountInCurrency = amountTransaction;
			m_amountInput.text = m_amountInCurrency;

			// CURRENCIES
			m_currencies = m_container.Find("Amount/Currency").GetComponent<Dropdown>();
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

			// MESSAGE
			m_container.Find("Pay/MessageTitle").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.send.write.description.transaction");
			m_messageInput = m_container.Find("Pay/Message").GetComponent<InputField>();
			m_messageInput.text = messageTransaction;
			m_container.Find("Pay/ExecutePayment").GetComponent<Button>().onClick.AddListener(OnExecutePayment);
			
			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
            EthereumEventController.Instance.EthereumEvent += new EthereumEventHandler(OnEthereumEvent);

			// UPDATE SELECTION CURRENCY
			m_currencies.value = 1;
			m_currencies.value = 0;
			m_currencySelected = EthereumController.Instance.CurrentCurrency;
			if (indexCurrentCurrency != -1)
			{
				m_currencies.value = indexCurrentCurrency;
			}
			m_exchangeToEther = EthereumController.Instance.CurrenciesExchange[m_currencySelected];

			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + "EthereumController.Instance.Network.ToString()";

            OnValuePublicKeyChanged(publicKeyAddress);
        }

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			UIEventController.Instance.UIEvent -= OnUIEvent;
			EthereumEventController.Instance.EthereumEvent -= OnEthereumEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnValuePublicKeyChanged
		 */
		private void OnValuePublicKeyChanged(string _newValue)
		{
			if ((_newValue.Length == EthereumController.TOTAL_SIZE_PUBLIC_KEY_ETHEREUM) &&
                (EthereumController.Instance.CurrentPublicKey != m_publicAddressToSend))
			{
				m_publicAddressToSend = m_publicAddressInput.text;
                // EthereumController.Instance.ValidatePublicKey(m_publicAddressToSend);
                EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_VALIDATE_PUBLIC_KEY, m_publicAddressToSend, true);
            }
		}

		// -------------------------------------------
		/* 
		 * OnValueAmountChanged
		 */
		private void OnValueAmountChanged(string _newValue)
		{
			if (_newValue.Length > 0)
			{
				m_amountInCurrency = m_amountInput.text;
				m_amountInEther = decimal.Parse(m_amountInCurrency) / m_exchangeToEther;
			}
		}

		// -------------------------------------------
		/* 
		 * OnCurrencyChanged
		 */
		private void OnCurrencyChanged(int _index)
		{
			EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_NEW_CURRENCY_SELECTED, m_currencies.options[_index].text);

			m_currencySelected = m_currencies.options[_index].text;
			m_exchangeToEther = (decimal)EthereumController.Instance.CurrenciesExchange[m_currencySelected];

			// UPDATE AMOUNT
			m_amountInCurrency = (m_amountInEther * m_exchangeToEther).ToString();
			m_amountInput.text = m_amountInCurrency;

			// UPDATE WALLET
			UpdateWalletButtonInfo();
		}

		// -------------------------------------------
		/* 
		 * UpdateWalletButtonInfo
		 */
		private void UpdateWalletButtonInfo()
		{
			string messageButton = "";
			string label = EthereumController.Instance.AddressToLabel(EthereumController.Instance.CurrentPublicKey);
			if (label != EthereumController.Instance.CurrentPublicKey)
			{
				messageButton = label;
			}
			decimal bitcoins = EthereumController.Instance.PrivateKeys[EthereumController.Instance.CurrentPrivateKey];
			m_exchangeToEther = EthereumController.Instance.CurrenciesExchange[m_currencySelected];
			if (messageButton.Length > 0)
			{
				messageButton += "/\n"; 
			}
			messageButton += Utilities.Trim(bitcoins.ToString()) + " ETH / \n";
			messageButton += Utilities.Trim((m_exchangeToEther * bitcoins).ToString()) + " " + m_currencySelected;
			m_container.Find("YourWallet/Text").GetComponent<Text>().text = messageButton;
		}

		// -------------------------------------------
		/* 
		 * OnCheckWallet
		 */
		private void OnCheckWallet()
		{
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
            Invoke("OnRealCheckWallet", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * OnRealCheckWallet
		 */
		public void OnRealCheckWallet()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
#if ENABLE_FULL_WALLET
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEthereumPrivateKeyView.SCREEN_NAME, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, false);
#else
            List<object> paramsWallet = new List<object>();
            paramsWallet.Add(EthereumController.Instance.CurrentPublicKey);
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEthereumPrivateKeyView.SCREEN_NAME, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, false, paramsWallet.ToArray());
#endif
        }


		// -------------------------------------------
		/* 
		 * OnSelectAddress
		 */
		private void OnSelectAddress()
		{
#if ENABLE_FULL_WALLET
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenSelectAddressFromView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false);
#endif
        }

		// -------------------------------------------
		/* 
		 * OnSaveAddress
		 */
		private void OnSaveAddress()
		{
#if ENABLE_FULL_WALLET
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEnterEmailView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, LanguageController.Instance.GetText("screen.enter.new.label.address"));
#endif
        }

		// -------------------------------------------
		/* 
		 * OnBackButton
		 */
		private void OnBackButton()
		{
			if (HasChanged)
			{
				string warning = LanguageController.Instance.GetText("message.warning");
				string description = LanguageController.Instance.GetText("message.exit.without.apply.changes");
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENETHEREUM_CONFIRMATION_EXIT_TRANSACTION);
            }
			else
			{
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_SCREENETHEREUMSEND_CANCELATION);
				Destroy();
			}
		}

		// -------------------------------------------
		/* 
		 * OnAddressValid
		 */
		private void OnAddressValid()
		{
			string description = "";
			if (m_validPublicAddressToSend)
			{
				description = LanguageController.Instance.GetText("screen.ethereum.send.valid.address");
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), description, null, "");
            }
			else
			{
				description = LanguageController.Instance.GetText("screen.ethereum.send.invalid.address");
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), description, null, "");
            }
		}

		// -------------------------------------------
		/* 
		 * OnExecutePayment
		 */
		private void OnExecutePayment()
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("m_messageInput.text=" + m_messageInput.text);
			Debug.Log("m_publicAddressToSend=" + m_publicAddressToSend);
			Debug.Log("m_amountInBitcoins=" + m_amountInEther);
#endif
            if (!m_validPublicAddressToSend)
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.ethereum.send.no.valid.address.to.send"), null, "");
            }
			else
			{
				if (m_amountInEther == 0)
				{
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.ethereum.send.amount.no.zero"), null, "");
                }
				else
				{
					decimal amountTotalUSD = m_amountInEther * EthereumController.Instance.CurrenciesExchange[EthereumController.CODE_DOLLAR];
					if (amountTotalUSD < 1m)
					{
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("screen.ethereum.send.amount.too.low"), null, SUB_EVENT_SCREENETHEREUM_CONTINUE_WITH_LOW_FEE);
                    }
					else
					{
						SummaryTransactionForLastConfirmation();
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * SummaryTransaction
		 */
		private void SummaryTransactionForLastConfirmation()
		{
            List<object> paramsTransaction = new List<object>();
            paramsTransaction.Add(m_amountInEther);
            paramsTransaction.Add(m_currencySelected);
            paramsTransaction.Add(EthereumController.Instance.AddressToLabel(m_publicAddressToSend));
            paramsTransaction.Add(m_messageInput.text);
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 1, null, ScreenTransactionSummaryView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, paramsTransaction);
        }

		// -------------------------------------------
		/* 
		 * OnExecutePayment
		 */
		private void OnExecuteRealPayment()
		{
			EthereumController.Instance.Pay(EthereumController.Instance.CurrentPrivateKey,
								m_publicAddressToSend,
								m_messageInput.text,
								m_amountInEther);
        }

		// -------------------------------------------
		/* 
		 * OnBitcoinEvent
		 */
		private void OnEthereumEvent(string _nameEvent, params object[] _list)
		{
            if (_nameEvent == EthereumController.EVENT_ETHEREUMCONTROLLER_VALIDATE_PUBLIC_KEY)
            {
                string publicKeyAddress = (string)_list[0];
                ValidPublicKeyToSend = (bool)_list[1];
#if ENABLE_FULL_WALLET
                bool enableButtonSaveAddress = true;
                if (EthereumController.Instance.ContainsAddress(m_publicAddressToSend))
                {
                    enableButtonSaveAddress = false;
                }
                if (enableButtonSaveAddress)
                {
                    m_saveAddress.SetActive(true);
                }
#endif

                m_validAddress.SetActive(true);
                m_validAddress.transform.Find("IconValid").gameObject.SetActive(m_validPublicAddressToSend);
                m_validAddress.transform.Find("IconError").gameObject.SetActive(!m_validPublicAddressToSend);

                string labelAddress = EthereumController.Instance.AddressToLabel(publicKeyAddress);
                if ((labelAddress.Length > 0) && (labelAddress != publicKeyAddress))
                {
                    m_container.Find("Address/Label").GetComponent<Text>().text = labelAddress;
                    m_container.Find("Address/Label").GetComponent<Text>().color = Color.red;
                }
            }
            if (_nameEvent == EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_DONE)
			{
				UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
				m_transactionSuccess = (bool)_list[0];
				m_transactionIDHex = "";
				if ((bool)_list[0])
				{
					HasChanged = false;
					m_transactionIDHex = (string)_list[1];					 
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.ethereum.send.transaction.success"), null, SUB_EVENT_SCREENETHEREUM_USER_CONFIRMATION_MESSAGE);
                }
				else
				{								
					string messageError = LanguageController.Instance.GetText("screen.ethereum.send.transaction.error");
					if (_list.Length >= 2)
					{
						messageError = (string)_list[1];
					}
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), messageError, null, SUB_EVENT_SCREENETHEREUM_USER_ERROR_MESSAGE);
                }
			}
			if (_nameEvent == EthereumController.EVENT_ETHEREUMCONTROLLER_SELECTED_PUBLIC_KEY)
			{
				string publicKeyAddress = (string)_list[0];
				HasChanged = true;
				m_publicAddressInput.text = publicKeyAddress;
				m_publicAddressToSend = publicKeyAddress;
				// EthereumController.Instance.ValidatePublicKey(m_publicAddressToSend);
                EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_VALIDATE_PUBLIC_KEY, m_publicAddressToSend, true);
#if DEBUG_MODE_DISPLAY_LOG
                Debug.Log("EVENT_ETHEREUMCONTROLLER_SELECTED_PUBLIC_KEY::PUBLIC KEY ADDRESS=" + publicKeyAddress);
#endif
			}
		}

		// -------------------------------------------
		/* 
		 * OnUIEvent
		 */
		protected void OnUIEvent(string _nameEvent, params object[] _list)
		{
            OnMenuEvent(_nameEvent, _list);

#if ENABLE_FULL_WALLET
			if (_nameEvent == ScreenEnterEmailView.EVENT_SCREENENTEREMAIL_CONFIRMATION)
			{
				string label = (string)_list[0];
				EthereumController.Instance.SaveAddresses(m_publicAddressToSend, label);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.ethereum.send.address.saved"), null, "");
                m_saveAddress.SetActive(false);
				if (label.Length > 0)
				{
					m_container.Find("Address/Label").GetComponent<Text>().text = label;
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.red;
				}
			}
#endif
			if (_nameEvent == EVENT_SCREENETHEREUMSEND_USER_CONFIRMED_RUN_TRANSACTION)
			{
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
                Invoke("OnExecuteRealPayment", 0.1f);
			}

			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == ScreenController.EVENT_CONFIRMATION_POPUP)
			{
				string subEvent = (string)_list[2];
				if (subEvent == SUB_EVENT_SCREENETHEREUM_CONFIRMATION_EXIT_TRANSACTION)
				{
					if ((bool)_list[1])
					{
						Destroy();
					}
				}
				if (subEvent == SUB_EVENT_SCREENETHEREUM_CONTINUE_WITH_LOW_FEE)
				{
					if ((bool)_list[1])
					{
						SummaryTransactionForLastConfirmation();
					}
				}
				if (subEvent == SUB_EVENT_SCREENETHEREUM_USER_CONFIRMATION_MESSAGE)
				{
					EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_USER_ACKNOWLEDGE, m_transactionSuccess, m_transactionIDHex);					
				}
                if (subEvent == SUB_EVENT_SCREENETHEREUM_USER_ERROR_MESSAGE)
                {
                    EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_USER_ACKNOWLEDGE, false);
                }
            }
            if (this.gameObject.activeSelf)
            {
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
                {
                    OnBackButton();
                }
            }
        }
    }
}