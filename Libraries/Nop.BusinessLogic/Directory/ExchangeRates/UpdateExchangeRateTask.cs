﻿//------------------------------------------------------------------------------
// The contents of this file are subject to the nopCommerce Public License Version 1.0 ("License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at  http://www.nopCommerce.com/License.aspx. 
// 
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
// See the License for the specific language governing rights and limitations under the License.
// 
// The Original Code is nopCommerce.
// The Initial Developer of the Original Code is NopSolutions.
// All Rights Reserved.
// 
// Contributor(s): _______. 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Xml;
using NopSolutions.NopCommerce.BusinessLogic.Audit;
using NopSolutions.NopCommerce.BusinessLogic.Tasks;
using NopSolutions.NopCommerce.BusinessLogic.Configuration.Settings;
using NopSolutions.NopCommerce.BusinessLogic.IoC;

namespace NopSolutions.NopCommerce.BusinessLogic.Directory.ExchangeRates
{
    /// <summary>
    /// Represents a task for uupdating exchange rates
    /// </summary>
    public partial class UpdateExchangeRateTask : ITask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        /// <param name="node">Xml node that represents a task description</param>
        public void Execute(XmlNode node)
        {
            if (!IoCFactory.Resolve<ISettingManager>().GetSettingValueBoolean("ExchangeRateProvider.AutoUpdateEnabled", false))
                return;

            long lastUpdateTimeTicks = IoCFactory.Resolve<ISettingManager>().GetSettingValueLong("ExchangeRateProvider.LastUpdateTime", 0);
            DateTime lastUpdateTime = DateTime.FromBinary(lastUpdateTimeTicks);
            lastUpdateTime = DateTime.SpecifyKind(lastUpdateTime, DateTimeKind.Utc);
            if (lastUpdateTime.AddHours(1) < DateTime.UtcNow)
            {
                //update rates each one hour
                var exchangeRates = IoCFactory.Resolve<ICurrencyManager>().GetCurrencyLiveRates(IoCFactory.Resolve<ICurrencyManager>().PrimaryExchangeRateCurrency.CurrencyCode);

                foreach (var exchageRate in exchangeRates)
                {
                    Currency currency = IoCFactory.Resolve<ICurrencyManager>().GetCurrencyByCode(exchageRate.CurrencyCode);
                    if (currency != null)
                    {
                        currency.Rate = exchageRate.Rate;
                        currency.UpdatedOn = DateTime.UtcNow;
                        IoCFactory.Resolve<ICurrencyManager>().UpdateCurrency(currency);
                    }
                }

                //save new update time value
                IoCFactory.Resolve<ISettingManager>().SetParam("ExchangeRateProvider.LastUpdateTime", DateTime.UtcNow.ToBinary().ToString());
            }
        }
    }
}
