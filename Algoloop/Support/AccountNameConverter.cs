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

using Algoloop.Model;
using GalaSoft.MvvmLight.Ioc;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Algoloop.Support
{
    public class AccountNameConverter : TypeConverter
    {
        private readonly AccountsModel _accounts;

        public AccountNameConverter()
        {
            _accounts = SimpleIoc.Default.GetInstance<AccountsModel>();
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            // true means show a combobox
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            // true will limit to list. false will show the list, but allow free-form entry
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            // Request list of accounts
            IReadOnlyList<AccountModel> accounts = _accounts.GetAccounts();
            List<string> list = accounts
                .Select(m => m.DisplayName)
                .ToList();

            list.Sort();

            return new StandardValuesCollection(list);
        }
    }
}
