using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YourCommonTools;
using YourEthereumController;

namespace YourEthereumManager
{

	/******************************************
	 * 
	 * SlotTransactionView
	 * 
	 * Slot that will be used to display trasaction data
	 * 
	 * @author Esteban Gallardo
	 */
	public class SlotTransactionView : Button, ISlotView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SLOT_TRANSACTION_SELECTED = "EVENT_SLOT_TRANSACTION_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private Transform m_container;
		private string m_id;
		private string m_title;
		private decimal m_amount;
		private string m_gas;
		private DateTime m_date;

		private Dictionary<string, Transform> m_iconsCurrencies = new Dictionary<string, Transform>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			ItemMultiObjectEntry item = (ItemMultiObjectEntry)_list[0];

			m_container = this.gameObject.transform;
			
			m_id = (string)item.Objects[0];
			m_date = (DateTime)item.Objects[1];
			m_amount = (decimal)item.Objects[2];
			m_gas = (string)item.Objects[3];
			m_title = (string)item.Objects[4];

			List<ItemMultiTextEntry> transactionScriptPubKey = (List<ItemMultiTextEntry>)item.Objects[5];
			string addresses = "";
			for (int i = 0; i < transactionScriptPubKey.Count; i++)
			{
				ItemMultiTextEntry sitem = transactionScriptPubKey[i];
				if (addresses.Length > 0)
				{
					addresses += ":";
				}

				addresses += sitem.Items[1];
            }
			m_container.Find("Target").GetComponent<Text>().text = EthereumController.Instance.AddressToLabelUpperCase(addresses.Split(':'));

			m_container.Find("Title").GetComponent<Text>().text = m_title;
			string dateTrimmed = m_date.ToString();
			m_container.Find("Date").GetComponent<Text>().text = dateTrimmed;

			m_container.Find("Bitcoins").GetComponent<Text>().text = Utilities.Trim(EthereumController.FromWei(new BigInteger(m_amount)).ToString());

			if (m_amount < 0)
			{
				m_container.GetComponent<Image>().color = new Color(188f / 255f, 83f / 255f, 141f / 255f);
				m_container.Find("IconsCalendar/Input").gameObject.SetActive(false);
				m_container.Find("IconsCalendar/Output").gameObject.SetActive(true);
				m_container.Find("IconsType/Sent").gameObject.SetActive(true);
				m_container.Find("IconsType/Received").gameObject.SetActive(false);
			}
			else
			{
				m_container.GetComponent<Image>().color = new Color(53f / 255f, 174f / 255f, 64f/255f);
				m_container.Find("IconsCalendar/Input").gameObject.SetActive(true);
				m_container.Find("IconsCalendar/Output").gameObject.SetActive(false);
				m_container.Find("IconsType/Sent").gameObject.SetActive(false);
				m_container.Find("IconsType/Received").gameObject.SetActive(true);
			}

			m_iconsCurrencies.Clear();
			for (int i = 0; i < EthereumController.CURRENCY_CODE.Length; i++)
			{
				m_iconsCurrencies.Add(EthereumController.CURRENCY_CODE[i], m_container.Find("IconsCurrency/" + EthereumController.CURRENCY_CODE[i]));
			}


			UpdateCurrency();

			EthereumEventController.Instance.EthereumEvent += new EthereumEventHandler(OnEthereumEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public bool Destroy()
		{
            EthereumEventController.Instance.EthereumEvent -= OnEthereumEvent;
			GameObject.Destroy(this.gameObject);

			return true;
		}

		// -------------------------------------------
		/* 
		 * UpdateCurrency
		 */
		public void UpdateCurrency()
		{
			string balanceCurrencyWallet = (EthereumController.FromWei(new BigInteger(m_amount)) * EthereumController.Instance.CurrenciesExchange[EthereumController.Instance.CurrentCurrency]).ToString();
			m_container.Find("Price").GetComponent<Text>().text = Utilities.Trim(balanceCurrencyWallet);
			m_container.Find("Currency").GetComponent<Text>().text = EthereumController.Instance.CurrentCurrency;

			foreach (KeyValuePair<string, Transform> item in m_iconsCurrencies)
			{
				if (item.Key == EthereumController.Instance.CurrentCurrency)
				{
					item.Value.gameObject.SetActive(true);
				}
				else
				{
					item.Value.gameObject.SetActive(false);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnPointerClick
		 */
		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			UIEventController.Instance.DispatchUIEvent(EVENT_SLOT_TRANSACTION_SELECTED, m_id);
		}

		// -------------------------------------------
		/* 
		 * OnBitcoinEvent
		 */
		private void OnEthereumEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EthereumController.EVENT_ETHEREUMCONTROLLER_CURRENCY_CHANGED)
			{
				UpdateCurrency();
			}
		}

	}
}