using UnityEngine;
using YourCommonTools;
using YourEthereumController;
using YourNetworkingTools;

namespace YourEthereumManager
{

	/******************************************
	 * 
	 * ScreenController
	 * 
	 * ScreenManager controller that handles all the screens's creation and disposal
	 * 
	 * 	To get Bitcoins in the Main Network:
	 *  
	 *  https://buy.blockexplorer.com/
	 *  
	 *  Or in the TestNet Network:
	 *  
	 *  https://testnet.manu.backend.hamburg/faucet
	 *
	 * @author Esteban Gallardo
	 */
	public class ScreenEthereumController : FunctionsScreenController
    {
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const int MAXIMUM_NUMBER_OF_STACKED_SCREENS = 20;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static ScreenEthereumController _instance;

		public static ScreenEthereumController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(ScreenEthereumController)) as ScreenEthereumController;
					DontDestroyOnLoad(_instance);
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public TextAsset ReadMeFile;

		public string ScreenToLoad = "";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------			
		private string m_screenToLoad = "";
		private object[] m_optionalParams = null;


		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	


		// -------------------------------------------
		/* 
		* Awake
		*/
		public override void Awake()
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            base.Awake();
		}

		// -------------------------------------------
		/* 
		 * Initialitzation listener
		 */
		public override void Start()
		{
			base.Start();

#if DEBUG_MODE_DISPLAY_LOG
            Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
#endif


#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        Screen.SetResolution(550, 900, false);
#endif

#if ENABLE_ETHEREUM
            EthereumEventController.Instance.EthereumEvent += new EthereumEventHandler(OnEthereumEvent);
            LanguageController.Instance.Initialize();

            if (ScreenToLoad.Length > 0)
			{
				InitializeEthereum(UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, ScreenToLoad);
			}		
#else
            EnableProcessEvents = false;
#endif
        }

        // -------------------------------------------
        /* 
		 * InitializeBitcoin
		 */
        public virtual void InitializeEthereum(UIScreenTypePreviousAction _typeAction, string _screenToLoad = "", params object[] _optionalParams)
		{
			m_screenToLoad = _screenToLoad;
			m_optionalParams = _optionalParams;
			if (m_hasBeenInitialized)
			{
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, m_screenToLoad, _typeAction, true, m_optionalParams);
            }
			else
			{
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INITIAL_CONNECTION, _typeAction, LanguageController.Instance.GetText("message.your.bitcoin.manager.title"), LanguageController.Instance.GetText("message.connecting.to.blockchain"), null, null);

                Invoke("InitializeRealEthereum", 0.1f);
			}
		}

        // -------------------------------------------
        /* 
		 * StartSplashScreen
		 */
        public override void StartSplashScreen()
        {
        }

        // -------------------------------------------
        /* 
		 * InitializeRealBitcoin
		 */
        public void InitializeRealEthereum()
		{
			EthereumController.Instance.Init();
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		public override void Destroy()
		{
			base.Destroy();

#if ENABLE_ETHEREUM
			EthereumEventController.Instance.EthereumEvent -= OnEthereumEvent;

			LanguageController.Instance.Destroy();
			CommController.Instance.Destroy();
			EthereumController.Instance.Destroy();
#endif
            Destroy(_instance);
			_instance = null;
		}



		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected virtual void OnEthereumEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EthereumController.EVENT_ETHEREUMCONTROLLER_ALL_DATA_COLLECTED)
			{
				if (!m_hasBeenInitialized)
				{
					m_hasBeenInitialized = true;
                    EthereumController.Instance.LoadPrivateKeys(true);

					if (EthereumController.Instance.CurrentPrivateKey.Length == 0)
					{
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEthereumPrivateKeyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
                    }
					else
					{
						if (m_screenToLoad.Length > 0)
						{
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, m_screenToLoad, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true, m_optionalParams);
                        }
					}
				}
				EthereumEventController.Instance.DispatchEthereumEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_ALL_DATA_INITIALIZED);
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected override void OnUIEvent(string _nameEvent, params object[] _list)
		{
            if (!PreProcessScreenEvents(_nameEvent, _list)) return;

            base.OnUIEvent(_nameEvent, _list);

			if (_nameEvent == ScreenController.EVENT_FORCE_DESTRUCTION_POPUP)
			{
                DestroyScreensFromLayerPool();
			}
			
		}
	}
}