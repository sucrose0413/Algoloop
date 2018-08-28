﻿/*
 * Copyright 2018 Capnode AB
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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Algoloop.Model;
using Algoloop.ViewSupport;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using QuantConnect;
using QuantConnect.Brokerages;
using QuantConnect.Brokerages.Fxcm;
using QuantConnect.Logging;

namespace Algoloop.ViewModel
{
    public class AccountViewModel : ViewModelBase
    {
        private AccountsViewModel _parent;
        private CancellationTokenSource _cancel;

        public AccountViewModel(AccountsViewModel accountsViewModel, AccountModel accountModel)
        {
            _parent = accountsViewModel;
            Model = accountModel;

            EnabledCommand = new RelayCommand(() => OnEnabledCommand(Model.Enabled), true);
            StartAccountCommand = new RelayCommand(() => OnStartAccountCommand(), () => !Enabled);
            StopAccountCommand = new RelayCommand(() => StopTask(), () => Enabled);
            DeleteAccountCommand = new RelayCommand(() => _parent?.DeleteAccount(this), () => !Enabled);

            OnEnabledCommand(Model.Enabled);
        }

        public AccountModel Model { get; }

        public RelayCommand EnabledCommand { get; }
        public RelayCommand StartAccountCommand { get; }
        public RelayCommand StopAccountCommand { get; }
        public RelayCommand DeleteAccountCommand { get; }

        public bool Enabled
        {
            get => Model.Enabled;
            set
            {
                Model.Enabled = value;
                RaisePropertyChanged(() => Enabled);
                StartAccountCommand.RaiseCanExecuteChanged();
                StopAccountCommand.RaiseCanExecuteChanged();
                DeleteAccountCommand.RaiseCanExecuteChanged();
            }
        }

        public SyncObservableCollection<PositionViewModel> Positions { get; } = new SyncObservableCollection<PositionViewModel>();
        public SyncObservableCollection<BalanceViewModel> Balances { get; } = new SyncObservableCollection<BalanceViewModel>();

        private void OnEnabledCommand(bool value)
        {
            if (value)
            {
                StartTask();
            }
            else
            {
                StopTask();
            }
        }

        private void OnStartAccountCommand()
        {
            Enabled = true;
            StartTask();
        }

        private async void StartTask()
        {
            Log.Trace($"Connect Account {Model.Name}");
            _cancel = new CancellationTokenSource();
            await Task.Run(() => StartFxcm(_cancel.Token), _cancel.Token);
            _cancel = null;
            Log.Trace($"Disconnect Account {Model.Name}");
            Enabled = false;
        }

        private void StopTask()
        {
            if (_cancel != null)
            {
                _cancel.Cancel();
            }
        }

        private void StartFxcm(CancellationToken cancel)
        {
            Brokerage brokerage = null;
            try
            {
                var brokerageFactory = new FxcmBrokerageFactory();
                var data = brokerageFactory.BrokerageData;
                brokerage = new FxcmBrokerage(null, null, data["fxcm-server"], data["fxcm-terminal"], Model.Login, Model.Password, Model.Id);
                brokerage.AccountChanged += OnAccountChanged;
                brokerage.OptionPositionAssigned += OnOptionPositionAssigned;
                brokerage.OrderStatusChanged += OnOrderStatusChanged;
                brokerage.Message += OnMessage;
                brokerage.Connect();

                List<QuantConnect.Orders.Order> orders = brokerage.GetOpenOrders();

                bool stop = false;
                while (!stop)
                {
                    // Set Positions
                    List<Holding> holdings = brokerage.GetAccountHoldings();
                    if (Positions.Count != holdings.Count)
                    {
                        Positions.Clear();
                        holdings.ForEach(m => Positions.Add(new PositionViewModel(m)));
                    }
                    else
                    {
                        int i = 0;
                        foreach (var holding in holdings)
                        {
                            Positions[i++].Update(holding);
                        }
                    }

                    // Set Balance
                    List<QuantConnect.Securities.Cash> balances = brokerage.GetCashBalance();
                    if (Balances.Count != balances.Count)
                    {
                        Balances.Clear();
                        balances.ForEach(m => Balances.Add(new BalanceViewModel(m)));
                    }
                    else
                    {
                        int i = 0;
                        foreach (var balance in balances)
                        {
                            Balances[i++].Update(balance);
                        }
                    }

                    // Tick data

                    stop = cancel.WaitHandle.WaitOne(1000);
                }

                brokerage.Disconnect();
                Positions.Clear();
                Balances.Clear();
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.GetType()}: {ex.Message}");
                if (brokerage != null)
                {
                    brokerage.Disconnect();
                }
            }
        }

        private void OnMessage(object sender, QuantConnect.Brokerages.BrokerageMessageEvent e)
        {
            Log.Trace($"{e.GetType()}: {e.Code} {e.Message}");
        }

        private void OnOrderStatusChanged(object sender, QuantConnect.Orders.OrderEvent e)
        {
            Log.Trace($"{e.GetType()}: {e.Message}");
        }

        private void OnOptionPositionAssigned(object sender, QuantConnect.Orders.OrderEvent e)
        {
            Log.Trace($"{e.GetType()}: {e.Message}");
        }

        private void OnAccountChanged(object sender, QuantConnect.Securities.AccountEvent e)
        {
            Log.Trace($"{e.GetType()}: {e.CurrencySymbol} {e.CashBalance}");
        }
    }
}