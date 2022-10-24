/*
MIT License

Copyright (c) 2022 enrico.speranza@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityCipher;

//ALGORAND
using Algorand;
using Algorand.Algod;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.Utils;
using Cysharp.Threading.Tasks;

public class AlgorandManager : Singleton<AlgorandManager>
{
    [Header("Player Configuration:")]
    [SerializeField]
    protected string m_PlayerName;
    protected string _Version = "0.20 Alfa";
    protected Account _AMAccount = null;
    private const string _InternalPassword = "0sIhlNRkMfDH8J9cC0Ky";

    [Header("Algorand Configuration:")]
    [Tooltip("ALGOD/PureStake URL Endpoint")]
    [SerializeField]
    public string ALGOD_URL_ENDPOINT = string.Empty;
    [Tooltip("ALGOD/PureStake Token")]
    [SerializeField]
    public string ALGOD_TOKEN = string.Empty;
    [Tooltip("INDEXER/PureStake URL Endpoint")]
    [SerializeField]
    public string ALGOD_URL_ENDPOINT_INDEXER = string.Empty;

    DefaultApi algodApiInstance;

    // OnEnable is called before Start
    protected virtual void OnEnable()
    {
        Debug.Log("Starting Algorand Manager...");
        this.ALGOD_URL_ENDPOINT = PlayerPrefs.GetString("AlgorandAccountSDKURL", "https://testnet-algorand.api.purestake.io/ps2");
        this.ALGOD_TOKEN = PlayerPrefs.GetString("AlgorandSDKToken", "IkwGyG4qWg8W6VegMFfCa3iIIj06wi0x6Vn7FO5j");
        this.ALGOD_URL_ENDPOINT_INDEXER = PlayerPrefs.GetString("AlgorandAccountSDKURLIndexer", "https://testnet-algorand.api.purestake.io/idx2");
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
        	Debug.unityLogger.filterLogType = LogType.Exception;
#endif
    }

    protected virtual void Start()
    {
        this.ConnectToNode(this.ALGOD_URL_ENDPOINT, this.ALGOD_TOKEN).Forget();
    }

    /// <summary>
    /// Get AlgorandSDK Version
    /// </summary>
    /// <returns>AlgorandDSK Version</returns>
    public string Version()
    {
        return _Version;
    }

    /// <summary>
    /// Get Actual Player Name
    /// </summary>
    /// <returns>Player Name</returns>
    public string GetPlayerName()
    {
        return m_PlayerName;
    }
    protected virtual void OnApplicationQuit()
    {
        Debug.Log("Algorand Manager stopped.");
    }
    //Publics Methods

    /// <summary>
    /// Generate new Algorand Account but not saved in Playprefs
    /// </summary>
    /// <returns>Algorand Account Mnemonic Passphrase</returns>
    public string GenerateAccount()
    {
        Account _Account = new Account();
        return _Account.ToMnemonic().ToString();
    }

    /// <summary>
    /// Load Algorand Account from Mnemonic Passphrase
    /// </summary>
    /// <param name="Passphrase"></param>
    /// <returns>Algorand Account Address</returns>
    public string LoadAccountFromPassphrase(string Passphrase)
    {
        if (_AMAccount == null)
        {
            _AMAccount = new Account(Passphrase);
            return _AMAccount.Address.ToString();
        }
        else
        {
            return _AMAccount.Address.ToString();
        }
    }

    /// <summary>
    /// Generate a new Algorand Account, save crypted in PlayerPrefs and in AlgorandManager instance
    /// </summary>
    /// <returns>Algorand Account Address generated</returns>
    public string GenerateAccountAndSave()
    {
        if (_AMAccount == null)
        {
            _AMAccount = new Account();
            //Save encrypted Mnemonic Algorand Account in PlayPrefs
            if (!PlayerPrefs.HasKey("AlgorandAccountSDK"))
            {
                PlayerPrefs.SetString("AlgorandAccountSDK", RijndaelEncryption.Encrypt(_AMAccount.ToMnemonic().ToString(), SystemInfo.deviceUniqueIdentifier + _InternalPassword));
                return _AMAccount.Address.ToString();
            }
            else
            {
                Debug.LogError("There is already an account saved in PlayerPrefs!");
                throw new InvalidOperationException("There is already an account saved in PlayerPrefs!");
            }
        }
        else
        {
            return _AMAccount.Address.ToString();
        }
    }

    /// <summary>
    /// Load Account from PlayPrefs and use in Algorand Manager instance
    /// </summary>
    /// <returns>Algorand Account Address saved in PlayPrefs</returns>
    public string LoadAccountFromPlayerPrefs()
    {
        //Load encrypted Mnemonic Algorand Account from PlayPrefs
        if (PlayerPrefs.HasKey("AlgorandAccountSDK"))
        {
            if (_AMAccount == null)
            {
                //Debug
                //Debug.Log("Start debug decrypt...");
                //string TestDecrypt = RijndaelEncryption.Decrypt(PlayerPrefs.GetString("AlgorandAccountSDK"), SystemInfo.deviceUniqueIdentifier + _InternalPassword);
                //Debug.LogWarning(TestDecrypt);
                _AMAccount = new Account(RijndaelEncryption.Decrypt(PlayerPrefs.GetString("AlgorandAccountSDK"), SystemInfo.deviceUniqueIdentifier + _InternalPassword));
                return _AMAccount.Address.ToString();
            }
            else
            {
                Debug.LogError("There is already an account loaded!");
                throw new InvalidOperationException("There is already an account loaded!");
            }
        }
        else
        {
            Debug.LogError("PlayPrefs does not contain a saved Algorand Account");
            throw new InvalidOperationException("PlayPrefs does not contain a saved Algorand Account");
        }
    }

    /// <summary>
    /// Save Algorand Account in encrypted PlayPrefs
    /// </summary>
    /// <param name="Passphrase">Mnemonic Algorand Account</param>
    /// <returns>True if saved</returns>
    public Boolean SaveAccountInPlayerPrefs(string Passphrase)
    {
        if (!String.IsNullOrEmpty(Passphrase))
        {
            //Save encrypted Mnemonic Algorand Account in PlayPrefs
            if (!PlayerPrefs.HasKey("AlgorandAccountSDK"))
            {
                PlayerPrefs.SetString("AlgorandAccountSDK", RijndaelEncryption.Encrypt(Passphrase, SystemInfo.deviceUniqueIdentifier + _InternalPassword));
                return true;
            }
            else
            {
                Debug.LogError("There is already an account saved in PlayerPrefs!");
                throw new InvalidOperationException("There is already an account saved in PlayerPrefs!");
            }
        }
        else
        {
            Debug.LogError("Passphrase passed is Null or empty!");
            throw new InvalidOperationException("Passphrase passed is Null or empty!");
        }
    }

    /// <summary>
    /// Delete actual Algorand Account from PlayerPrefs
    /// WARNING: this method will irrevocably delete your account from PlayerPrefs!
    /// </summary>
    /// <returns>Boolean true if procedure went ok</returns>
    public bool DeleteAccountFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("AlgorandAccountSDK"))
        {
            PlayerPrefs.DeleteKey("AlgorandAccountSDK");
            //Delete always hash too (If the key does not exist, DeleteKey has no impact.)
            PlayerPrefs.DeleteKey("AlgorandAccountSDKHash");
            return true;
        }
        else
        {
            Debug.LogError("PlayPrefs does not contain a saved Algorand Account");
            throw new InvalidOperationException("PlayPrefs does not contain a saved Algorand Account");
        }
    }

    /// <summary>
    /// Get Actual Account Address initialized in AlgorandManager
    /// </summary>
    /// <returns>Algorand Account Address</returns>
	public string GetAddressAccount()
    {

        if (_AMAccount != null)
        {
            return _AMAccount.Address.ToString();
        }
        else
        {
            Debug.LogError("Account not generated yet!");
            throw new InvalidOperationException("Account not generated yet!");
        }
    }

    /// <summary>
    /// Return Private Key of Algorand Account
    /// </summary>
    /// <returns>Byte Array</returns>
    public byte[] GetPrivateKey()
    {
        if (_AMAccount != null)
        {
            return _AMAccount.KeyPair.ClearTextPrivateKey;
        }
        else
        {
            Debug.LogError("Account not generated yet!");
            throw new InvalidOperationException("Account not generated yet!");
        }
    }

    /// <summary>
    /// Return Public Key of Algorand Account
    /// </summary>
    /// <returns>Byte Array</returns>
    public byte[] GetPublicKey()
    {
        if (_AMAccount != null)
        {
            return _AMAccount.KeyPair.ClearTextPublicKey;
        }
        else
        {
            Debug.LogError("Account not generated yet!");
            throw new InvalidOperationException("Account not generated yet!");
        }
    }

    /// <summary>
    /// Get Actual Mnemonic Passphrase initialized in AlgorandManager
    /// </summary>
    /// <returns>Algorand Account Mnemonic Passphrase</returns>
	public string GetMnemonicPassphrase()
    {
        if (_AMAccount != null)
        {
            return _AMAccount.ToMnemonic().ToString();
        }
        else
        {
            Debug.LogError("Account not generated yet!");
            throw new InvalidOperationException("Account not generated yet!");
        }
    }

    /// <summary>
    /// Verify if Algorand Address is well formated
    /// </summary>
    /// <param name="AddressPassed"></param>
    /// <returns>Simple Boolean: True or False</returns>
    public bool AddressIsValid(string AddressPassed)
    {
        if (Address.IsValid(AddressPassed))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Connect to ALGOD / Purestack Node
    /// </summary>
    public async UniTaskVoid ConnectToNode(string AlgodURLEndpoint, string AlgodToken)
    {
        if (string.IsNullOrEmpty(AlgodURLEndpoint) || string.IsNullOrEmpty(AlgodToken))
        {
            Debug.LogError("AlgodURLEndpoint or AlgodToken are null or empty!");
            throw new ArgumentException("AlgodURLEndpoint or AlgodToken are null or empty!");
        }
        else
        {
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_URL_ENDPOINT, ALGOD_TOKEN);
            algodApiInstance = new DefaultApi(httpClient);

            try
            {
                var supply = await algodApiInstance.GetSupplyAsync();
                Debug.Log("Total Algorand Supply: " + supply.TotalMoney);
                Debug.Log("Online Algorand Supply: " + supply.OnlineMoney);
            }
            catch (ApiException<ErrorResponse> e)
            {
                Debug.Log("Exception when calling algod#getSupply: " + e.Result.Message);
            }
        }
    }
}