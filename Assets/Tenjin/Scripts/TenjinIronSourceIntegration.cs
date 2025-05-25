//
//  Copyright (c) 2022 Tenjin. All rights reserved.
//

using System;
using System.Globalization;
using UnityEngine;

public class TenjinIronSourceIntegration
{
    private static bool _subscribed_ironsource = false;

    public TenjinIronSourceIntegration()
    {
    }

    public static void ListenForImpressions(Action<string> callback)
    {
#if tenjin_ironsource_enabled
        if (_subscribed_ironsource)
        {
            Debug.Log("Ignoring duplicate IronSource subscription");
            return;
        }
        IronSourceEvents.onImpressionDataReadyEvent += (impressionData) =>
        {
            double parsedDoubleLifetimeRevenue = 0.0;
            double parsedDoubleRevenue = 0.0;

            CultureInfo invCulture = CultureInfo.InvariantCulture;

            if (impressionData.lifetimeRevenue != null && impressionData.revenue != null) {
                double.TryParse(string.Format(invCulture, "{0}", impressionData.lifetimeRevenue), NumberStyles.Any, invCulture, out parsedDoubleLifetimeRevenue);
                double.TryParse(string.Format(invCulture, "{0}", impressionData.revenue), NumberStyles.Any, invCulture, out parsedDoubleRevenue);
            }

            try {
                IronSourceAdImpressionData ironSourceAdImpressionData = new IronSourceAdImpressionData()
                {
                    ab = impressionData.ab,
                    ad_network = impressionData.adNetwork,
                    ad_unit = impressionData.adUnit,
                    auction_id = impressionData.auctionId,
                    country = impressionData.country,
                    instance_id = impressionData.instanceId,
                    instance_name = impressionData.instanceName,
                    lifetime_revenue = parsedDoubleLifetimeRevenue,
                    placement = impressionData.placement,
                    precision = impressionData.precision,
                    revenue = parsedDoubleRevenue,
                    segment_name = impressionData.segmentName,
                    encrypted_cpm = impressionData.encryptedCPM
                };

                string json = JsonUtility.ToJson(ironSourceAdImpressionData);
                callback(json);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error parsing impression data: " + ex.ToString());
            }
        };
        _subscribed_ironsource = true;
#endif
    }
}

[System.Serializable]
internal class IronSourceAdImpressionData
{
    public string ab;
    public string ad_network;
    public string ad_unit;
    public string auction_id;
    public string country;
    public string instance_id;
    public string instance_name;
    public double lifetime_revenue;
    public string placement;
    public string precision;
    public double revenue;
    public string segment_name;
    public string encrypted_cpm;
}
