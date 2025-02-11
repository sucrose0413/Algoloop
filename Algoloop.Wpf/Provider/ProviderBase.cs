/*
 * Copyright 2019 Capnode AB
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Algoloop.Model;
using Algoloop.Support;
using Algoloop.Wpf.Common;
using Newtonsoft.Json;
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Securities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Algoloop.Wpf.Provider
{
    abstract public class ProviderBase : IProvider
    {
        protected bool _isDisposed;
        private ConfigProcess _process;

        public virtual bool Register(SettingModel settings)
        {
            // Get name of virtual class
            string name = GetType().Name.ToLowerInvariant();

            // Make sure provider is registered
            if (Market.Encode(name) == null)
            {
                // be sure to add a reference to the unknown market, otherwise we won't be able to decode it coming out
                int code = 0;
                while (Market.Decode(code) != null)
                {
                    code++;
                }

                Market.Add(name, code);
            }

            // Make sure Market Hours Database exist
            string folder = Path.Combine(settings.DataFolder, "market-hours");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "market-hours-database.json");
            if (!File.Exists(path))
            {
                var emptyExchangeHours = new Dictionary<SecurityDatabaseKey, MarketHoursDatabase.Entry>();
                string jsonString = JsonConvert.SerializeObject(new MarketHoursDatabase(emptyExchangeHours));
                File.WriteAllText(path, jsonString);
            }

            return true;
        }

        public virtual void Login(ProviderModel provider)
        {
        }

        public virtual void Logout()
        {
            if (_process != null)
            {
                bool stopped = _process.Abort();
                Debug.Assert(stopped);
            }
        }

        public virtual void GetAccounts(ProviderModel market, Action<object> update)
        {
        }

        public virtual void GetMarketData(ProviderModel market, Action<object> update)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                if (_process != null)
                {
                    _process.Dispose();
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _process = null;
            _isDisposed = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        internal void RunProcess(string cmd, string[] args, IDictionary<string, string> configs)
        {
            bool ok = true;
            _process = new ConfigProcess(
                cmd,
                string.Join(" ", args),
                Directory.GetCurrentDirectory(),
                true,
                (line) => Log.Trace(line),
                (line) =>
                {
                    ok = false;
                    Log.Error(line, true);
                });

            // Set Environment
            StringDictionary environment = _process.Environment;

            // Set config file
            IDictionary<string, string> config = _process.Config;
            string exeFolder = MainService.GetProgramFolder();
            config["debug-mode"] = "true";
            config["composer-dll-directory"] = exeFolder;
            config["results-destination-folder"] = ".";
            config["plugin-directory"] = ".";
            config["log-handler"] = "ConsoleLogHandler";
            config["map-file-provider"] = "LocalDiskMapFileProvider";
            config["#command"] = cmd;
            config["#parameters"] = string.Join(" ", args);
            config["#work-directory"] = Directory.GetCurrentDirectory();
            foreach (KeyValuePair<string, string> item in configs)
            {
                config.Add(item);
            }

            // Start process
            try
            {
                _process.Start();
                _process.WaitForExit();
                if (!ok) throw new ApplicationException("See logs for details");
            }
            finally
            {
                _process.Dispose();
                _process = null;
            }
        }

        internal void UpdateAccounts(ProviderModel market, IEnumerable<AccountModel> accounts, Action<object> update)
        {
            if (!Collection.SmartCopy(accounts, market.Accounts)) return;
            foreach (AccountModel account in accounts)
            {
                update(account);
            }
        }

        protected static void UpdateSymbols(
            ProviderModel market,
            IEnumerable<SymbolModel> actual,
            bool defaultActive,
            bool addNew = true)
        {
            Contract.Requires(market != null, nameof(market));
            Contract.Requires(actual != null, nameof(actual));

            // Collect list of obsolete symbols
            Collection<SymbolModel> symbols = market.Symbols;
            List<SymbolModel> discarded = symbols.ToList();
            foreach (SymbolModel item in actual)
            {
                SymbolModel symbol = symbols.FirstOrDefault(x => x.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase)
                    && (x.Market.Equals(item.Market, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(x.Market))
                    && (x.Security.Equals(item.Security) || x.Security.Equals(SecurityType.Base)));
                if (symbol == default)
                {
                    if (addNew)
                    {
                        var symbolModel = new SymbolModel(item.Id, item.Market, item.Security)
                        {
                            Active = defaultActive,
                            Name = item.Name,
                            Properties = item.Properties
                        };
                        symbols.Add(symbolModel);
                    }
                }
                else
                {
                    // Update properties
                    symbol.Name = item.Name;
                    symbol.Market = item.Market;
                    symbol.Security = item.Security;
                    symbol.Properties = item.Properties;
                    discarded.Remove(symbol);
                }
            }

            foreach (SymbolModel old in discarded)
            {
                symbols.Remove(old);
            }
        }
    }
}
