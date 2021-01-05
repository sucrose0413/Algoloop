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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Algoloop.Model
{
    [Serializable]
    [DataContract]
    public class AccountModel : ModelBase
    {
        public enum AccountType { Backtest, Paper, Live };

        [Browsable(false)]
        [ReadOnly(false)]
        public BrokerModel Broker { get; set; }

        [Browsable(false)]
        [ReadOnly(false)]
        [DataMember]
        public bool Active { get; set; }

        [Category("Account")]
        [DisplayName("Account name")]
        [Description("Name of the account.")]
        [Browsable(true)]
        [ReadOnly(false)]
        [DataMember]
        public string Name { get; set; } = "Account";

        [Category("Account")]
        [DisplayName("Account number")]
        [Description("Account number.")]
        [Browsable(false)]
        [ReadOnly(false)]
        [DataMember]
        public string Id { get; set; } = string.Empty;

        [Browsable(false)]
        [ReadOnly(false)]
        [DataMember]
        public Collection<OrderModel> Orders { get; } = new Collection<OrderModel>();

        public string DisplayName => Broker == default ? Name : $"{Broker.Name}/{Name}";

        public void Refresh()
        {
        }
    }
}