﻿/*
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ALGORAND
using Algorand;
using Algorand.Algod;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.Utils;

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

    // OnEnable is called before Start
    protected virtual void OnEnable()
    {
        Debug.Log("Starting Algorand Manager...");
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

}
